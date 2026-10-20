import { render } from "preact";
import { useEffect, useLayoutEffect, useRef, useState } from "preact/hooks";
import {
  api,
  matches,
  mergeEntries,
  parseTrafficFilter,
  matchesTrafficFilter,
  trafficRoute,
} from "./model";
import type { Entry, Settings, Snapshot, Stats } from "./model";
import { Inspector, VirtualRows } from "./inspect";
import "./style.css";

function saved<T>(key: string, fallback: T): T {
  try {
    return JSON.parse(localStorage.getItem(key) ?? "null") ?? fallback;
  } catch {
    return fallback;
  }
}

function SettingsPanel({
  settings,
  save,
}: {
  settings: Settings;
  save: (settings: Settings) => Promise<void>;
}) {
  const [form, setForm] = useState(settings);
  const [message, setMessage] = useState("");
  const [filters, setFilters] = useState({
    packetIds: settings.packetIds.join(", "),
    rpcIds: settings.rpcIds.join(", "),
    subIds: settings.subIds.join(", "),
  });
  useLayoutEffect(() => setForm(settings), [settings]);
  return (
    <section class="settings-page">
      <span class="index-mark">03 / CONFIGURATION</span>
      <h1>Capture on your terms.</h1>
      <p class="hint">
        Server settings are saved in this module's user-data folder. Capture
        starts OFF after every module restart.
      </p>
      <form
        onSubmit={async (event) => {
          event.preventDefault();
          setMessage("");
          try {
            const ids = (value: string) =>
              value.trim()
                ? value.split(",").map((part) => {
                    if (!/^\s*\d+\s*$/.test(part))
                      throw new Error("Use comma-separated numeric IDs.");
                    return Number(part.trim());
                  })
                : [];
            await save({
              ...form,
              packetIds: ids(filters.packetIds),
              rpcIds: ids(filters.rpcIds),
              subIds: ids(filters.subIds),
            });
            setMessage(
              "Saved. Port changes apply after restarting the module.",
            );
          } catch (reason) {
            setMessage(String((reason as Error).message));
          }
        }}
      >
        <div class="settings-grid">
          <fieldset>
            <legend>Transport</legend>
            {(["incoming", "outgoing", "rpc", "packets"] as const).map(
              (key) => (
                <label class="checkbox" key={key}>
                  <input
                    type="checkbox"
                    checked={form[key]}
                    onChange={(event) =>
                      setForm({ ...form, [key]: event.currentTarget.checked })
                    }
                  />
                  {key}
                </label>
              ),
            )}
            <label>
              Listening port
              <input
                aria-label="Listening port"
                type="number"
                min="1024"
                max="65535"
                value={form.port}
                onInput={(event) =>
                  setForm({ ...form, port: Number(event.currentTarget.value) })
                }
              />
            </label>
          </fieldset>
          <fieldset>
            <legend>Retention</legend>
            <label>
              Maximum entries
              <input
                aria-label="Maximum entries"
                type="number"
                min="1"
                max="100000"
                value={form.maxEntries}
                onInput={(event) =>
                  setForm({
                    ...form,
                    maxEntries: Number(event.currentTarget.value),
                  })
                }
              />
            </label>
            <label>
              Raw buffer, MiB
              <input
                aria-label="Raw buffer MiB"
                type="number"
                min="1"
                max="128"
                value={form.rawBudgetMiB}
                onInput={(event) =>
                  setForm({
                    ...form,
                    rawBudgetMiB: Number(event.currentTarget.value),
                  })
                }
              />
            </label>
            <p class="hint">
              Oldest entries are evicted when either limit is reached. Full
              bytes remain available for every retained entry.
            </p>
          </fieldset>
          <fieldset>
            <legend>ID allowlists</legend>
            {(["packetIds", "rpcIds", "subIds"] as const).map((key) => (
              <label key={key}>
                {key}
                <input
                  aria-label={key}
                  placeholder="Empty = all IDs"
                  value={filters[key]}
                  onInput={(event) =>
                    setFilters({ ...filters, [key]: event.currentTarget.value })
                  }
                />
              </label>
            ))}
            <p class="hint">
              Comma-separated decimal IDs. Packet filters use the effective ID
              inside a timestamp envelope.
            </p>
          </fieldset>
        </div>
        <button class="primary" type="submit">
          Save configuration
        </button>
        <p role="status">{message}</p>
      </form>
    </section>
  );
}

function App() {
  const [page, setPage] = useState(saved("sf.debug.page", "traffic"));
  const [settings, setSettings] = useState<Settings | null>(null);
  const [entries, setEntries] = useState<Entry[]>([]);
  const [stats, setStats] = useState<Stats | null>(null);
  const [session, setSession] = useState("");
  const [selected, setSelected] = useState<Entry | null>(null);
  const [connected, setConnected] = useState(false);
  const [paused, setPaused] = useState(false);
  const [error, setError] = useState("");
  const [search, setSearch] = useState(saved("sf.debug.search", ""));
  const [routeFilter, setRouteFilter] = useState(saved("sf.debug.routes", ""));
  const [directions, setDirections] = useState(
    saved<string[]>("sf.debug.directions", ["in", "out"]),
  );
  const [kinds, setKinds] = useState(
    saved<string[]>("sf.debug.kinds", ["rpc", "packet"]),
  );
  const [groups, setGroups] = useState(
    saved<string[] | null>("sf.debug.groups", null),
  );
  const [autoScroll, setAutoScroll] = useState(saved("sf.debug.scroll", true));
  const [split, setSplit] = useState(saved("sf.debug.split", 50));
  const dragging = useRef(false);
  const latest = useRef({
    limit: 10_000,
    oldest: 0,
    paused: false,
    session: "",
  });
  latest.current.limit = settings?.maxEntries ?? 10_000;
  latest.current.paused = paused;

  useLayoutEffect(() => {
    localStorage.setItem("sf.debug.page", JSON.stringify(page));
    localStorage.setItem("sf.debug.search", JSON.stringify(search));
    localStorage.setItem("sf.debug.routes", JSON.stringify(routeFilter));
    localStorage.setItem("sf.debug.directions", JSON.stringify(directions));
    localStorage.setItem("sf.debug.kinds", JSON.stringify(kinds));
    localStorage.setItem("sf.debug.groups", JSON.stringify(groups));
    localStorage.setItem("sf.debug.scroll", JSON.stringify(autoScroll));
    localStorage.setItem("sf.debug.split", JSON.stringify(split));
  }, [page, search, routeFilter, directions, kinds, groups, autoScroll, split]);

  useEffect(() => {
    let disposed = false;
    let socket: WebSocket | null = null;
    let timer: ReturnType<typeof setTimeout>;
    const cancellation = new AbortController();
    let pending: Entry[] = [];
    let flushTimer: ReturnType<typeof setTimeout> | undefined;
    const connect = () => {
      if (disposed) return;
      socket = new WebSocket(
        `${location.protocol === "https:" ? "wss" : "ws"}://${location.host}/ws`,
      );
      socket.onopen = () => {
        setConnected(true);
        setError("");
      };
      socket.onclose = () => {
        setConnected(false);
        if (!disposed) timer = setTimeout(connect, 2000);
      };
      socket.onerror = () => socket?.close();
      socket.onmessage = (event) => {
        const snapshot: Snapshot = JSON.parse(event.data);
        if (snapshot.session !== latest.current.session) {
          latest.current.session = snapshot.session;
          latest.current.oldest = 0;
          pending = [];
          setEntries([]);
          setSelected(null);
          setSession(snapshot.session);
        }
        if (latest.current.paused) return;
        for (const entry of snapshot.entries) pending.push(entry);
        if (pending.length > latest.current.limit)
          pending.splice(0, pending.length - latest.current.limit);
        if (flushTimer === undefined)
          flushTimer = setTimeout(() => {
            const batch = pending;
            pending = [];
            flushTimer = undefined;
            if (latest.current.paused || disposed) return;
            setEntries((old) =>
              mergeEntries(
                old,
                batch,
                latest.current.limit,
                latest.current.oldest,
              ),
            );
          }, 100);
      };
    };
    const poll = async () => {
      try {
        const next = await api<Stats>(
          "/api/stats",
          undefined,
          cancellation.signal,
        );
        if (!disposed) {
          setStats(next);
          latest.current.oldest = next.oldestSeq;
          setEntries((old) =>
            old.length && old[0].seq < next.oldestSeq
              ? old.filter((entry) => entry.seq >= next.oldestSeq)
              : old,
          );
        }
      } catch (reason) {
        if (!disposed) setError(String((reason as Error).message));
      }
    };
    api<Settings>("/api/config", undefined, cancellation.signal)
      .then((value) => {
        if (!disposed) setSettings(value);
      })
      .catch((reason) => {
        if (!disposed) setError(String(reason.message));
      });
    connect();
    void poll();
    const interval = setInterval(poll, 1000);
    return () => {
      disposed = true;
      cancellation.abort();
      clearTimeout(timer);
      clearInterval(interval);
      clearTimeout(flushTimer);
      socket?.close();
    };
  }, []);

  let filterError = "";
  let routeRules: ReturnType<typeof parseTrafficFilter> = [];
  try {
    routeRules = parseTrafficFilter(routeFilter);
  } catch (reason) {
    filterError = (reason as Error).message;
  }
  const categories = [
    ...new Set(
      entries
        .map((entry) => entry.category)
        .filter((value): value is string => !!value),
    ),
  ];
  const availableGroups = [
    ...new Set(
      entries
        .filter((entry) => entry.category === page)
        .map((entry) => entry.group)
        .filter((value): value is string => !!value),
    ),
  ];
  const activeGroups = groups ?? availableGroups;
  const visible = entries.filter(
    (entry) =>
      directions.includes(entry.direction) &&
      kinds.includes(entry.kind) &&
      (page !== "traffic" ||
        (!filterError && matchesTrafficFilter(entry, routeRules))) &&
      (page === "traffic" ||
        (entry.category === page &&
          entry.group !== null &&
          activeGroups.includes(entry.group))) &&
      matches(entry, search),
  );

  async function save(value: Settings) {
    const next = await api<Settings>("/api/config", value);
    setSettings(next);
  }
  async function toggleCapture() {
    if (settings)
      try {
        await save({ ...settings, capture: !settings.capture });
      } catch (reason) {
        setError(String((reason as Error).message));
      }
  }
  function toggle(
    value: string,
    list: string[],
    setter: (next: string[]) => void,
  ) {
    setter(
      list.includes(value)
        ? list.filter((item) => item !== value)
        : [...list, value],
    );
  }
  async function resume() {
    if (!paused) {
      setPaused(true);
      return;
    }
    try {
      const snapshot = await api<Snapshot>("/api/entries");
      setEntries(snapshot.entries);
      setPaused(false);
    } catch (reason) {
      setError(String((reason as Error).message));
    }
  }
  return (
    <div class="app-shell">
      <header>
        <div class="brand">
          <span class="brand-icon">SF</span>
          <div>
            <strong>DEBUGWEB</strong>
            <span>NETWORK OBSERVATORY</span>
          </div>
        </div>
        <nav aria-label="Main navigation">
          {["traffic", ...categories, "settings"].map((value) => (
            <button
              class={page === value ? "active" : ""}
              onClick={() => setPage(value)}
              key={value}
            >
              {value.toUpperCase()}
            </button>
          ))}
        </nav>
        <div class="connection">
          <i class={connected ? "online" : ""} />
          {connected ? "CONNECTED" : "RECONNECTING"}
          <span>:{stats?.listeningPort ?? settings?.port ?? 7777}</span>
        </div>
      </header>
      <div class="status-strip">
        <span>
          {stats?.retained ?? 0} <small>retained</small>
        </span>
        <span>
          {((stats?.rawBytes ?? 0) / 1048576).toFixed(2)} <small>MiB raw</small>
        </span>
        <span>
          {stats?.evicted ?? 0} <small>history evictions</small>
        </span>
        <span class={stats?.droppedBroadcastEntries ? "loss" : ""}>
          {stats?.droppedBroadcastEntries ?? 0} <small>delivery drops</small>
        </span>
        <span class="session">SESSION {session.slice(0, 8) || "--------"}</span>
      </div>
      {error && (
        <div class="error" role="alert">
          {error}
          <button onClick={() => setError("")}>Dismiss</button>
        </div>
      )}
      {page === "settings" ? (
        settings && <SettingsPanel settings={settings} save={save} />
      ) : (
        <>
          <div class="toolbar">
            <button
              class={`capture-button ${settings?.capture ? "running" : ""}`}
              disabled={!settings}
              onClick={toggleCapture}
            >
              {settings?.capture ? "STOP CAPTURE" : "START CAPTURE"}
            </button>
            <input
              aria-label="Search traffic"
              placeholder="Search ID, sub-ID or name..."
              value={search}
              onInput={(event) => setSearch(event.currentTarget.value)}
            />
            {["in", "out"].map((dir) => (
              <button
                class={`filter-direction ${dir} ${directions.includes(dir) ? "active" : ""}`}
                aria-pressed={directions.includes(dir)}
                onClick={() => toggle(dir, directions, setDirections)}
                key={dir}
              >
                {dir.toUpperCase()}
              </button>
            ))}
            {["rpc", "packet"].map((kind) => (
              <button
                class={`filter-kind ${kind} ${kinds.includes(kind) ? "active" : ""}`}
                aria-pressed={kinds.includes(kind)}
                onClick={() => toggle(kind, kinds, setKinds)}
                key={kind}
              >
                {kind.toUpperCase()}
              </button>
            ))}
            <button class={paused ? "active" : ""} onClick={resume}>
              {paused ? "RESUME VIEW" : "PAUSE VIEW"}
            </button>
            <button
              class={autoScroll ? "active" : ""}
              aria-pressed={autoScroll}
              onClick={() => setAutoScroll(!autoScroll)}
            >
              FOLLOW
            </button>
            <a class="button" href="/api/export" download>
              EXPORT
            </a>
            <button
              onClick={async () => {
                try {
                  const next = await api<Stats>("/api/clear", {});
                  latest.current.oldest = next.oldestSeq;
                  setStats(next);
                  setEntries([]);
                  setSelected(null);
                } catch (reason) {
                  setError(String((reason as Error).message));
                }
              }}
            >
              CLEAR
            </button>
          </div>
          {page === "traffic" && (
            <div class="traffic-filters">
              <label for="route-filter">ROUTES</label>
              <input
                id="route-filter"
                aria-label="Traffic route filter"
                aria-invalid={Boolean(filterError)}
                aria-describedby="route-filter-help"
                placeholder="220, 220/17, rpc:93, !207"
                value={routeFilter}
                onInput={(event) => setRouteFilter(event.currentTarget.value)}
              />
              <button
                disabled={!selected}
                onClick={() => {
                  if (selected) {
                    setRouteFilter(trafficRoute(selected));
                    setDirections(["in", "out"]);
                    setKinds(["rpc", "packet"]);
                    setSearch("");
                  }
                }}
              >
                Only selected route
              </button>
              <button
                disabled={!selected || Boolean(filterError)}
                onClick={() => {
                  if (selected) {
                    const rule = `!${trafficRoute(selected)}`;
                    setRouteFilter((old) =>
                      old.split(",").some((part) => part.trim() === rule)
                        ? old
                        : [old, rule].filter(Boolean).join(", "),
                    );
                  }
                }}
              >
                Hide selected route
              </button>
              <button
                onClick={() => {
                  setRouteFilter("");
                  setSearch("");
                  setDirections(["in", "out"]);
                  setKinds(["rpc", "packet"]);
                }}
              >
                Reset filters
              </button>
              <span
                id="route-filter-help"
                class={filterError ? "loss" : "filter-help"}
              >
                {filterError ||
                  "View only. Comma = OR; ! excludes. Capture and export stay unchanged."}
              </span>
            </div>
          )}
          {page !== "traffic" && (
            <div class="extension-groups">
              {availableGroups.map((group) => (
                <button
                  class={activeGroups.includes(group) ? "active" : ""}
                  onClick={() => toggle(group, activeGroups, setGroups)}
                  key={group}
                >
                  {group}
                </button>
              ))}
            </div>
          )}
          <div
            class="workspace"
            style={{
              gridTemplateColumns: `minmax(300px, ${split}fr) 6px minmax(380px, ${100 - split}fr)`,
            }}
          >
            <section class="traffic-panel">
              <div class="section-heading">
                <span class="index-mark">
                  {page === "traffic"
                    ? "01 / TRAFFIC"
                    : `02 / ${page.toUpperCase()}`}
                </span>
                <span>
                  {visible.length} visible{" "}
                  {paused ? "/ view paused, server still captures" : ""}
                </span>
              </div>
              <div class="traffic-head">
                <span>SEQ</span>
                <span>DIR</span>
                <span>KIND</span>
                <span>ID / SUB</span>
                <span>NAME</span>
                <span>BYTES</span>
              </div>
              {visible.length === 0 ? (
                <div class="empty">
                  <h2>
                    {settings?.capture
                      ? "Listening for traffic."
                      : "Ready when you are."}
                  </h2>
                  <p>
                    {settings?.capture
                      ? "Adjust filters or interact with the game."
                      : "Start capture to inspect packets and RPCs."}
                  </p>
                </div>
              ) : (
                <VirtualRows
                  items={visible}
                  rowHeight={26}
                  follow={autoScroll && !paused}
                  onFollowChange={setAutoScroll}
                  itemKey={(entry) => entry.seq}
                  render={(entry) => (
                    <button
                      class={`traffic-row ${selected?.seq === entry.seq ? "selected" : ""}`}
                      key={entry.seq}
                      onClick={() => setSelected(entry)}
                    >
                      <span class="seq">{entry.seq}</span>
                      <span class={`direction ${entry.direction}`}>
                        {entry.direction.toUpperCase()}
                      </span>
                      <span class={`kind ${entry.kind}`}>
                        {entry.kind === "packet" ? "PKT" : "RPC"}
                      </span>
                      <span class="route-id">
                        {entry.id}
                        {entry.subId !== null ? ` / ${entry.subId}` : ""}
                        {entry.effectiveId !== entry.id
                          ? ` > ${entry.effectiveId}`
                          : ""}
                      </span>
                      <span class="entry-name">{entry.name}</span>
                      <span>{entry.dataBytes}</span>
                    </button>
                  )}
                />
              )}
            </section>
            <div
              class="splitter"
              role="separator"
              aria-label="Resize inspector"
              aria-orientation="vertical"
              aria-valuenow={split}
              tabIndex={0}
              onPointerDown={(event) => {
                dragging.current = true;
                event.currentTarget.setPointerCapture(event.pointerId);
              }}
              onPointerUp={() => {
                dragging.current = false;
              }}
              onPointerCancel={() => {
                dragging.current = false;
              }}
              onPointerMove={(event) => {
                if (dragging.current) {
                  const bounds =
                    event.currentTarget.parentElement!.getBoundingClientRect();
                  setSplit(
                    Math.max(
                      25,
                      Math.min(
                        70,
                        (100 * (event.clientX - bounds.left)) / bounds.width,
                      ),
                    ),
                  );
                }
              }}
              onKeyDown={(event) => {
                if (event.key === "ArrowLeft" || event.key === "ArrowRight") {
                  event.preventDefault();
                  setSplit(
                    Math.max(
                      25,
                      Math.min(
                        70,
                        split + (event.key === "ArrowLeft" ? -5 : 5),
                      ),
                    ),
                  );
                }
              }}
            />
            <Inspector entry={selected} session={session} />
          </div>
        </>
      )}
      <footer>
        <a
          href="https://github.com/AllahPower/SF-Arizona"
          target="_blank"
          rel="noreferrer"
        >
          AllahPower / SF-Arizona
        </a>
        <span>DebugWeb plugin / local inspector</span>
      </footer>
    </div>
  );
}

render(<App />, document.getElementById("app")!);
