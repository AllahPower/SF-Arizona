import {
  useEffect,
  useLayoutEffect,
  useMemo,
  useRef,
  useState,
} from "preact/hooks";
import type { ComponentChildren } from "preact";
import {
  api,
  decodeRaw,
  hexByte,
  hexOffset,
  payloadBitsInByte,
  rawRowText,
  toPlain,
} from "./model";
import type { Details, Entry, Node } from "./model";

export function VirtualRows<T>({
  items,
  rowHeight,
  render,
  className = "",
  follow = false,
  onFollowChange,
  itemKey,
}: {
  items: T[];
  rowHeight: number;
  render: (item: T, index: number) => ComponentChildren;
  className?: string;
  follow?: boolean;
  onFollowChange?: (follow: boolean) => void;
  itemKey?: (item: T) => number | string;
}) {
  const viewport = useRef<HTMLDivElement>(null);
  const [window, setWindow] = useState({ top: 0, height: 600 });
  const anchor = useRef<{ key: number | string; offset: number } | null>(null);
  const previousHead = useRef<number | string | undefined>(undefined);
  const head = items.length && itemKey ? itemKey(items[0]) : undefined;
  const maximum = Math.max(0, items.length * rowHeight - window.height);
  let top = follow ? maximum : Math.min(window.top, maximum);
  if (!follow && itemKey && anchor.current && head !== previousHead.current) {
    const index = items.findIndex(
      (item) => itemKey(item) === anchor.current!.key,
    );
    top =
      index < 0
        ? 0
        : Math.min(maximum, index * rowHeight + anchor.current.offset);
  }
  const start = Math.max(0, Math.floor(top / rowHeight) - 8);
  const count = Math.ceil(window.height / rowHeight) + 16;

  useLayoutEffect(() => {
    const element = viewport.current!;
    const observer = new ResizeObserver(() => {
      setWindow((old) =>
        old.height === element.clientHeight
          ? old
          : { ...old, height: element.clientHeight },
      );
    });
    observer.observe(element);
    return () => observer.disconnect();
  }, []);
  useLayoutEffect(() => {
    const element = viewport.current!;
    // Position the DOM and the rendered window in the same commit, before paint.
    if (element.scrollTop !== top) element.scrollTop = top;
    const index = Math.floor(top / rowHeight);
    anchor.current =
      itemKey && items[index]
        ? { key: itemKey(items[index]), offset: top % rowHeight }
        : null;
    previousHead.current = head;
    if (!follow) setWindow((old) => (old.top === top ? old : { ...old, top }));
  });
  return (
    <div
      ref={viewport}
      class={`virtual ${className}`}
      onScroll={(event) => {
        const element = event.currentTarget;
        if (
          follow &&
          Math.abs(
            element.scrollHeight - element.clientHeight - element.scrollTop,
          ) <= 1
        )
          return;
        if (follow) onFollowChange?.(false);
        const index = Math.floor(element.scrollTop / rowHeight);
        anchor.current =
          itemKey && items[index]
            ? {
                key: itemKey(items[index]),
                offset: element.scrollTop % rowHeight,
              }
            : null;
        setWindow((old) => ({ ...old, top: element.scrollTop }));
      }}
    >
      <div
        style={{
          height: `${items.length * rowHeight}px`,
          position: "relative",
        }}
      >
        <div
          style={{
            position: "absolute",
            top: `${start * rowHeight}px`,
            width: "100%",
          }}
        >
          {items
            .slice(start, start + count)
            .map((item, index) => render(item, start + index))}
        </div>
      </div>
    </div>
  );
}

function Tree({ node }: { node: Node }) {
  const [open, setOpen] = useState(new Set(["Parsed", "Parsed.Payload"]));
  const [search, setSearch] = useState("");
  const [chosen, setChosen] = useState<Node | null>(null);
  const rows: { node: Node; path: string; depth: number }[] = [];
  function visit(item: Node, path: string, depth: number) {
    if (
      !search ||
      `${path} ${item.type} ${item.value ?? ""}`
        .toLowerCase()
        .includes(search.toLowerCase())
    )
      rows.push({ node: item, path, depth });
    if (search || open.has(path))
      for (const child of item.children)
        visit(child, `${path}.${child.name}`, depth + 1);
  }
  visit(node, "Parsed", 0);
  return (
    <div class="tree-pane">
      <input
        aria-label="Search structure"
        placeholder="Find a field, type or value"
        value={search}
        onInput={(event) => setSearch(event.currentTarget.value)}
      />
      <VirtualRows
        items={rows}
        rowHeight={24}
        render={({ node: item, path, depth }) => (
          <div class="tree-row" key={path}>
            <span class="tree-indent" aria-hidden="true">
              {Array.from({ length: depth }, (_, index) => (
                <span class="tree-guide" key={index} />
              ))}
            </span>
            <button
              class="expander"
              aria-label={`Toggle ${path}`}
              aria-expanded={
                item.children.length ? !!search || open.has(path) : undefined
              }
              title={
                item.children.length
                  ? `${search || open.has(path) ? "Collapse" : "Expand"} ${item.name}`
                  : undefined
              }
              disabled={!item.children.length}
              onClick={() => {
                const next = new Set(open);
                if (next.has(path)) next.delete(path);
                else next.add(path);
                setOpen(next);
              }}
            >
              {item.children.length ? (
                <svg viewBox="0 0 16 16" aria-hidden="true">
                  <path d="m6 3 5 5-5 5" />
                </svg>
              ) : (
                <span class="tree-leaf" aria-hidden="true" />
              )}
            </button>
            <button
              class="field"
              onClick={() => setChosen(item)}
              title={item.value ?? item.type}
            >
              {item.name}
            </button>
            <span class="type">{item.type}</span>
            <span class={`scalar ${item.kind}`}>
              {item.value ?? (item.kind === "null" ? "null" : "")}
            </span>
          </div>
        )}
      />
      {chosen && (
        <div class="field-value">
          <strong>{chosen.name}</strong>
          <button
            onClick={() =>
              navigator.clipboard.writeText(
                chosen.value ?? JSON.stringify(toPlain(chosen), null, 2),
              )
            }
          >
            Copy value
          </button>
          <pre>{chosen.value ?? JSON.stringify(toPlain(chosen), null, 2)}</pre>
        </div>
      )}
    </div>
  );
}

function Raw({
  details,
  encoding,
  setEncoding,
}: {
  details: Details;
  encoding: string;
  setEncoding: (value: string) => void;
}) {
  const bytes = useMemo(() => decodeRaw(details.raw), [details.raw]);
  const offsets = useMemo(
    () =>
      Array.from(
        { length: Math.ceil(bytes.length / 16) },
        (_, index) => index * 16,
      ),
    [bytes.length],
  );
  return (
    <div class="raw-pane">
      <div class="raw-toolbar">
        <span>
          {details.dataBitLength} valid bits / {bytes.length} bytes
        </span>
        <select
          aria-label="Raw text encoding"
          value={encoding}
          onChange={(event) => setEncoding(event.currentTarget.value)}
        >
          <option value="ascii">ASCII</option>
          <option value="utf-8">UTF-8</option>
          <option value="windows-1251">Windows-1251</option>
        </select>
        <a href={`/api/entries/${details.summary.seq}/raw`} download>
          Download .bin
        </a>
      </div>
      <p class="hint">
        {details.reconstructed &&
          "Reconstructed source control frame, not an original wire header. "}
        Offset is relative to the retained buffer. Highlight: payload bits{" "}
        {details.payloadBitOffset}..
        {details.payloadBitOffset + details.payloadBitLength}. RPC buffers
        contain only the bytes supplied by the host API.
      </p>
      <div class="hex-head">
        <span class="offset">OFFSET</span>
        <span class="hex-bytes">
          {Array.from({ length: 16 }, (_, index) => (
            <span key={index}>{hexByte(index)}</span>
          ))}
        </span>
        <span class="hex-text">TEXT</span>
      </div>
      <VirtualRows
        items={offsets}
        rowHeight={24}
        className="hex-scroll"
        render={(offset) => (
          <div class="hex-row" key={offset}>
            <span class="offset">{hexOffset(offset)}</span>
            <span class="hex-bytes">
              {Array.from({ length: 16 }, (_, index) => {
                const at = offset + index;
                return (
                  <span
                    class={
                      payloadBitsInByte(
                        at,
                        details.payloadBitOffset,
                        details.payloadBitLength,
                      )
                        ? "payload-byte"
                        : ""
                    }
                    title={`Offset 0x${hexOffset(at)}; ${Math.max(0, Math.min(8, details.dataBitLength - at * 8))} valid bits`}
                    key={at}
                  >
                    {at < bytes.length ? hexByte(bytes[at]) : "  "}
                  </span>
                );
              })}
            </span>
            <span class="hex-text">{rawRowText(bytes, offset, encoding)}</span>
          </div>
        )}
      />
    </div>
  );
}

export function Inspector({
  entry,
  session,
}: {
  entry: Entry | null;
  session: string;
}) {
  const [details, setDetails] = useState<Details | null>(null);
  const [error, setError] = useState("");
  const [tab, setTab] = useState("structure");
  const [encoding, setEncoding] = useState(
    localStorage.getItem("sf.debug.encoding") ?? "ascii",
  );
  useEffect(() => {
    localStorage.setItem("sf.debug.encoding", encoding);
  }, [encoding]);
  useEffect(() => {
    setDetails(null);
    setError("");
    if (!entry) return;
    const controller = new AbortController();
    api<Details>(`/api/entries/${entry.seq}`, undefined, controller.signal)
      .then(setDetails)
      .catch((reason) => {
        if (!controller.signal.aborted) setError(String(reason.message));
      });
    return () => controller.abort();
  }, [entry?.seq, session]);
  if (!entry)
    return (
      <aside class="inspector empty">
        <span class="index-mark">01 / INSPECT</span>
        <h2>Follow the bytes.</h2>
        <p>Select a packet to inspect its structure and original buffer.</p>
      </aside>
    );
  return (
    <aside class="inspector">
      <div class="inspect-heading">
        <span class={`direction ${entry.direction}`}>
          {entry.direction.toUpperCase()}
        </span>
        <h2>{entry.name}</h2>
        <span>#{entry.seq}</span>
      </div>
      <div class="inspect-meta">
        {entry.source && (
          <span title="Optional native observation source">
            SOURCE {entry.source}
          </span>
        )}
        <span>
          {entry.kind.toUpperCase()} {entry.id}
          {entry.subId !== null ? ` / ${entry.subId}` : ""}
        </span>
        <span>{entry.dataBytes} B</span>
      </div>
      <nav class="tabs" aria-label="Inspection view">
        {["structure", "json", "raw"].map((value) => (
          <button
            class={tab === value ? "active" : ""}
            onClick={() => setTab(value)}
            key={value}
          >
            {value.toUpperCase()}
          </button>
        ))}
      </nav>
      {error && (
        <div role="alert" class="error">
          {error}
        </div>
      )}
      {!details && !error && <p class="hint">Decoding on the game thread...</p>}
      {details && (
        <>
          {details.error && (
            <p class="error">
              {details.parser}: {details.error}. Original bytes are available in
              Raw.
            </p>
          )}
          {tab === "structure" &&
            (details.parsed ? (
              <Tree key={`${session}:${entry.seq}`} node={details.parsed} />
            ) : (
              <p class="hint">
                No parsed structure. Inspect the original bytes in Raw.
              </p>
            ))}
          {tab === "json" && (
            <div class="json-pane">
              <button
                onClick={() =>
                  navigator.clipboard.writeText(
                    JSON.stringify(
                      details.parsed ? toPlain(details.parsed) : null,
                      null,
                      2,
                    ),
                  )
                }
              >
                Copy JSON
              </button>
              <pre>
                {JSON.stringify(
                  details.parsed ? toPlain(details.parsed) : null,
                  null,
                  2,
                )}
              </pre>
            </div>
          )}
          {tab === "raw" && (
            <Raw
              details={details}
              encoding={encoding}
              setEncoding={setEncoding}
            />
          )}
        </>
      )}
    </aside>
  );
}
