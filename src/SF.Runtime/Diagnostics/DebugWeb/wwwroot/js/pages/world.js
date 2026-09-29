import { store } from "../store.js";
import { esc, fmtVec, fmtCompact, fmtNumber, fmtOpt, fmtColor, fmtAttachment, fmtLabelAttachment, trimText } from "../utils.js";
import { ws } from "../ws.js";
import { ensureMap, renderZones, syncSelection, invalidateSize } from "../components/gangzone-map.js";

let _root = null;
let _worldConfigTimer = 0;

const poolDefs = [
    { key: "overview", label: "Overview" },
    { key: "players", label: "Players" },
    { key: "vehicles", label: "Vehicles" },
    { key: "objects", label: "Objects" },
    { key: "pickups", label: "Pickups" },
    { key: "labels", label: "Labels" },
    { key: "textdraws", label: "TextDraws" },
    { key: "gangzones", label: "GangZones" },
    { key: "actors", label: "Actors" },
];

const poolCards = [
    { key: "players", name: "Players", meta: "Connected players in the server pool" },
    { key: "vehicles", name: "Vehicles", meta: "Spawned vehicles in stream range" },
    { key: "objects", name: "Objects", meta: "Dynamic objects in the world" },
    { key: "pickups", name: "Pickups", meta: "Active pickup entities" },
    { key: "labels", name: "Labels", meta: "3D text labels in the world" },
    { key: "textdraws", name: "TextDraws", meta: "Server + local textdraws" },
    { key: "gangzones", name: "GangZones", meta: "Map zone overlays" },
    { key: "actors", name: "Actors", meta: "NPC actors in the world" },
    { key: "menus", name: "Menus", meta: "Server-side menu instances" },
];

function render() {
    const poolBtns = poolDefs.map(p =>
        `<button class="pool-type-btn${p.key === "overview" ? " active" : ""}" data-pool="${p.key}">${p.label}</button>`
    ).join("");

    const cards = poolCards.map(c =>
        `<div class="pool-card"><div class="pool-header"><span class="pool-name">${c.name}</span><span class="pool-count" id="wPool${cap(c.key)}">0 / 0</span></div><div class="pool-bar"><div class="pool-bar-fill" id="wBar${cap(c.key)}" style="width:0%"></div></div><div class="pool-meta">${c.meta}</div></div>`
    ).join("");

    return `
<div class="world-toolbar">
    ${poolBtns}
    <div class="sep"></div>
    <input type="text" id="worldSearchBox" placeholder="Search by id/text...">
    <button id="btnWorldStreamZone">Stream Zone</button>
    <div class="sep"></div>
    <span class="world-status" id="worldStatus">No data</span>
</div>
<div class="world-content">
    <div class="world-main" id="poolView-overview">
        <div class="pool-section"><div class="pool-grid">${cards}</div></div>
    </div>
    ${poolTable("objects", 7, ["ID","Model","Position","Rotation","Camera Dist","Attached","Materials"], "60px,72px,260px,260px,92px,,80px")}
    ${poolTable("vehicles", 8, ["ID","Model","Health","Position","Driver","Passengers","Color","Siren"], "60px,72px,84px,260px,80px,96px,92px,72px")}
    ${poolTable("players", 9, ["ID","Name","Score","Ping","Health","Armour","Weapon","Vehicle","State"], "60px,,70px,70px,82px,82px,74px,74px,74px")}
    ${poolTable("pickups", 5, ["ID","Model","Type","Position","Distance"], "60px,72px,72px,,96px")}
    ${poolTable("labels", 6, ["ID","Text","Color","Position","Distance","Attached"], "60px,,116px,260px,96px,120px")}
    ${poolTable("textdraws", 6, ["ID","Text","Type","Position","Font","Color"], "60px,,72px,140px,72px,116px")}
    ${gangzoneView()}
    ${poolTable("actors", 6, ["ID","Model","Health","Position","Rotation","Invulnerable"], "60px,72px,84px,,92px,100px")}
    <div class="world-sidebar" id="worldSidebar">
        <h3>World Info</h3>
        <div class="stat-row"><div class="stat-card"><div class="label">Game State</div><div class="value small-value" id="wGameState">---</div></div></div>
        <div class="stat-row"><div class="stat-card"><div class="label">Local Player</div><div class="value small-value" id="wLocalPlayer">---</div></div></div>
        <h3>Selected Entity</h3>
        <div class="detail-view world-detail" id="worldDetailView">Select an entity to inspect.</div>
    </div>
</div>`;
}

function poolTable(key, cols, headers, widths) {
    const ws = widths.split(",");
    const colgroup = ws.map(w => w ? `<col style="width:${w}">` : "<col>").join("");
    const ths = headers.map(h => `<th>${h}</th>`).join("");
    return `
<div class="world-main" id="poolView-${key}" hidden>
    <div class="table-head"><table><colgroup>${colgroup}</colgroup><thead><tr>${ths}</tr></thead></table></div>
    <div class="table-body world-table-body"><table><colgroup>${colgroup}</colgroup><tbody id="w${cap(key)}Body"><tr><td colspan="${cols}" class="empty">No data yet.</td></tr></tbody></table></div>
</div>`;
}

function gangzoneView() {
    return `
<div class="world-main" id="poolView-gangzones" hidden>
    <div class="gangzones-map-panel">
        <div class="gangzones-map-header">
            <div>
                <div class="gangzones-map-title">Territory Map</div>
                <div class="gangzones-map-subtitle">Hover a zone to inspect its footprint and color owner marker.</div>
            </div>
            <div class="gangzones-map-legend" id="wGzLegend">No gang zones loaded.</div>
        </div>
        <div class="gangzones-map-shell">
            <div class="gangzones-map" id="wGzMap"></div>
            <div class="gangzones-map-tooltip" id="wGzMapTooltip">Hover a zone to inspect it.</div>
        </div>
    </div>
    <div class="table-head"><table><colgroup><col style="width:60px"><col style="width:92px"><col style="width:92px"><col style="width:92px"><col style="width:92px"><col style="width:116px"><col style="width:86px"></colgroup><thead><tr><th>ID</th><th>Min X</th><th>Min Y</th><th>Max X</th><th>Max Y</th><th>Color</th><th>Flashing</th></tr></thead></table></div>
    <div class="table-body world-table-body"><table><colgroup><col style="width:60px"><col style="width:92px"><col style="width:92px"><col style="width:92px"><col style="width:92px"><col style="width:116px"><col style="width:86px"></colgroup><tbody id="wGangzonesBody"><tr><td colspan="7" class="empty">No data yet.</td></tr></tbody></table></div>
</div>`;
}

function cap(s) {
    return s.charAt(0).toUpperCase() + s.slice(1);
}

function mount(root) {
    _root = root;

    for (const btn of root.querySelectorAll(".pool-type-btn")) {
        btn.addEventListener("click", () => switchPool(btn.dataset.pool));
    }

    root.querySelector("#worldSearchBox").addEventListener("input", () => {
        store.worldViewState.search = root.querySelector("#worldSearchBox").value;
        clearTimeout(_worldConfigTimer);
        _worldConfigTimer = setTimeout(() => ws.pushWorldViewState(), 150);
    });

    root.querySelector("#btnWorldStreamZone").addEventListener("click", () => {
        store.worldViewState.streamZone = !store.worldViewState.streamZone;
        updateFilterUi();
        ws.pushWorldViewState();
    });

    root.addEventListener("click", ev => {
        const tr = ev.target.closest(".world-main tbody tr[data-world-key]");
        if (!tr || !tr._worldEntry) return;
        selectWorldEntry(tr.dataset.worldKey, tr._worldEntry);
    });

    store.on("world:snapshot", onSnapshot);
    store.on("world:select", ({ key, entry }) => selectWorldEntry(key, entry));

    updateFilterUi();
}

function onShow() {
    if (store.worldViewState.section === "gangzones") {
        requestAnimationFrame(() => requestAnimationFrame(() => {
            const host = _root?.querySelector("#wGzMap");
            if (host) ensureMap(host);
            invalidateSize();
            if (store.latestWorldSnapshot?.gangZones) {
                renderZones(
                    store.latestWorldSnapshot.gangZones,
                    _root?.querySelector("#wGzMapTooltip"),
                    _root?.querySelector("#wGzLegend")
                );
            }
        }));
    }
}

function onHide() {}

function switchPool(poolName) {
    store.worldViewState.section = poolName;

    for (const view of _root.querySelectorAll("[id^='poolView-']")) {
        view.hidden = view.id !== `poolView-${poolName}`;
    }
    for (const btn of _root.querySelectorAll(".pool-type-btn")) {
        btn.classList.toggle("active", btn.dataset.pool === poolName);
    }

    updateFilterUi();
    ws.pushWorldViewState();

    if (poolName === "gangzones") {
        requestAnimationFrame(() => requestAnimationFrame(() => {
            const host = _root.querySelector("#wGzMap");
            if (host) ensureMap(host);
            invalidateSize();
            if (store.latestWorldSnapshot?.gangZones) {
                renderZones(
                    store.latestWorldSnapshot.gangZones,
                    _root.querySelector("#wGzMapTooltip"),
                    _root.querySelector("#wGzLegend")
                );
            }
        }));
    }
}

function updateFilterUi() {
    if (!_root) return;
    const streamBtn = _root.querySelector("#btnWorldStreamZone");
    const searchBox = _root.querySelector("#worldSearchBox");
    const isPlayers = store.worldViewState.section === "players";
    streamBtn.classList.toggle("active", store.worldViewState.streamZone);
    streamBtn.disabled = !isPlayers;
    searchBox.placeholder = isPlayers ? "Search by name/id..." : "Search by id/text...";
}

function onSnapshot(snapshot) {
    if (!_root) return;
    renderOverview(snapshot.overview ? snapshot.overview.pools : []);
    renderMeta(snapshot);

    const section = store.worldViewState.section;
    if (section === "players") {
        renderTable("wPlayersBody", 9, snapshot.players, p => [
            p.id, esc(p.name) || (p.isLocal ? "Local Player" : "-"), p.score, p.ping,
            fmtNumber(p.health), fmtNumber(p.armour), p.weapon, fmtOpt(p.vehicleId), p.state
        ], "player");
    } else if (section === "vehicles") {
        renderTable("wVehiclesBody", 8, snapshot.vehicles, v => [
            v.id, v.model, fmtNumber(v.health), fmtVec(v.position),
            v.hasDriver ? "yes" : "no", v.isOccupied ? "occupied" : "-",
            `${v.primaryColor}/${v.secondaryColor}`, v.sirenEnabled ? "on" : "off"
        ], "vehicle");
    } else if (section === "objects") {
        renderTable("wObjectsBody", 7, snapshot.objects, o => [
            o.id, o.model, fmtVec(o.position), fmtVec(o.rotation),
            fmtNumber(o.distanceToCamera), fmtAttachment(o.attachedToVehicleId, o.attachedToObjectId), o.materialCount
        ], "object");
    } else if (section === "pickups") {
        renderTable("wPickupsBody", 5, snapshot.pickups, p => [
            p.index, p.model, p.type, fmtVec(p.position), fmtNumber(p.distanceToLocal)
        ], "pickup");
    } else if (section === "labels") {
        renderTable("wLabelsBody", 6, snapshot.labels, l => [
            l.id, esc(trimText(l.text, 80)) || "-", fmtColor(l.color),
            fmtVec(l.position), fmtNumber(l.drawDistance), fmtLabelAttachment(l)
        ], "label");
    } else if (section === "textdraws") {
        renderTable("wTextdrawsBody", 6, snapshot.textDraws, t => [
            t.id, esc(trimText(t.text, 80)) || "-", t.style,
            `(${fmtCompact(t.x)}, ${fmtCompact(t.y)})`, t.model, fmtColor(t.color)
        ], "textdraw");
    } else if (section === "gangzones") {
        renderZones(
            snapshot.gangZones,
            _root.querySelector("#wGzMapTooltip"),
            _root.querySelector("#wGzLegend")
        );
        renderTable("wGangzonesBody", 7, snapshot.gangZones, z => [
            z.id, fmtCompact(z.minX), fmtCompact(z.minY),
            fmtCompact(z.maxX), fmtCompact(z.maxY), fmtColor(z.color),
            z.isFlashing ? "yes" : "no"
        ], "gangzone");
    } else if (section === "actors") {
        renderTable("wActorsBody", 6, snapshot.actors, a => [
            a.id, "-", fmtNumber(a.health), fmtVec(a.position),
            fmtCompact(a.rotation), a.isInvulnerable ? "yes" : "no"
        ], "actor");
    }
}

function renderOverview(pools) {
    const idMap = {
        players: "Players", vehicles: "Vehicles", objects: "Objects",
        pickups: "Pickups", labels: "Labels", textdraws: "Textdraws",
        gangzones: "Gangzones", actors: "Actors", menus: "Menus"
    };
    for (const pool of pools || []) {
        const suffix = idMap[pool.key];
        if (!suffix) continue;
        const countEl = _root.querySelector(`#wPool${suffix}`);
        const barEl = _root.querySelector(`#wBar${suffix}`);
        if (countEl) countEl.textContent = `${pool.count} / ${pool.max}`;
        if (barEl) {
            const pct = pool.max > 0 ? Math.max(0, Math.min(100, (pool.count / pool.max) * 100)) : 0;
            barEl.style.width = `${pct}%`;
        }
    }
}

function renderMeta(snapshot) {
    const ws = _root.querySelector("#worldStatus");
    const gs = _root.querySelector("#wGameState");
    const lp = _root.querySelector("#wLocalPlayer");
    if (ws) {
        ws.textContent = snapshot.status === "live" ? "Live snapshot"
            : snapshot.status === "not-initialized" ? "Pools not initialized"
            : "Pools unavailable";
    }
    if (gs) gs.textContent = `${snapshot.gameState}`;
    if (lp) {
        lp.textContent = snapshot.localPlayer?.isConnected
            ? `${snapshot.localPlayer.id}: ${snapshot.localPlayer.name || "-"}`
            : "---";
    }
}

function renderTable(bodyId, colCount, items, cellsFactory, kind) {
    const tbody = _root.querySelector(`#${bodyId}`);
    if (!tbody) return;

    const rows = items || [];
    if (!rows.length) {
        tbody.innerHTML = `<tr><td colspan="${colCount}" class="empty">No data.</td></tr>`;
        return;
    }

    tbody.innerHTML = rows.map(item => {
        const key = `${kind}:${item.id ?? item.index}`;
        const selected = store.worldSelectionKey === key ? ' class="selected"' : "";
        const cells = cellsFactory(item).map(c => `<td>${c ?? "-"}</td>`).join("");
        return `<tr data-world-key="${key}"${selected}>${cells}</tr>`;
    }).join("");

    const trs = tbody.querySelectorAll("tr[data-world-key]");
    for (let i = 0; i < trs.length; i++) trs[i]._worldEntry = rows[i];
}

function selectWorldEntry(key, entry) {
    store.worldSelectionKey = key;

    for (const row of _root.querySelectorAll(".world-main tbody tr.selected")) {
        row.classList.remove("selected");
    }
    const selectedRow = _root.querySelector(`.world-main tbody tr[data-world-key="${key}"]`);
    if (selectedRow) selectedRow.classList.add("selected");

    const detail = _root.querySelector("#worldDetailView");
    if (detail) {
        detail.style.display = "block";
        detail.textContent = formatDetail(key, entry);
    }

    syncSelection(_root.querySelector("#wGzMapTooltip"));
}

function formatDetail(key, entry) {
    const lines = [key];
    for (const [name, value] of Object.entries(entry || {})) {
        lines.push(`${name}: ${formatValue(value)}`);
    }
    return lines.join("\n");
}

function formatValue(value) {
    if (value === null || value === undefined) return "-";
    if (typeof value === "object") {
        if ("x" in value && "y" in value && "z" in value) return fmtVec(value);
        return JSON.stringify(value);
    }
    if (typeof value === "number") return Number.isInteger(value) ? `${value}` : fmtCompact(value);
    if (typeof value === "boolean") return value ? "true" : "false";
    return `${value}`;
}

export const WorldPage = { render, mount, onShow, onHide };
