const MAX_ENTRIES = 50000;
const MAX_DOM_ROWS = 1000;

let ring = new Array(MAX_ENTRIES);
let ringHead = 0;
let ringCount = 0;
let totalReceived = 0;

function ringPush(e) {
    ring[ringHead] = e;
    ringHead = (ringHead + 1) % MAX_ENTRIES;
    if (ringCount < MAX_ENTRIES) ringCount++;
}

function ringGet(i) {
    return ring[(ringHead - ringCount + i + MAX_ENTRIES) % MAX_ENTRIES];
}

let config = { capture: false, incoming: true, outgoing: true, rpc: true, packets: true };
let paused = false;
let autoScroll = true;
let ws = null;
let selectedSeq = null;
let worldSelectionKey = null;
let latestWorldSnapshot = null;
let idFilters = {};
let hasIncludedFilter = false;
let knownIds = new Map();
let pendingEntries = [];
let rafId = 0;
let domRowCount = 0;
let _tbody;
let _tableBody;
let _emptyMsg;
let _entryInfo;
let _searchBox;
let _cachedSearch = "";
let _applyViewTimer = 0;

function initRefs() {
    _tbody = document.getElementById("tbody");
    _tableBody = document.getElementById("tableBody");
    _emptyMsg = document.getElementById("emptyMsg");
    _entryInfo = document.getElementById("entryInfo");
    _searchBox = document.getElementById("searchBox");
}

function switchTab(tabName) {
    for (const page of document.querySelectorAll(".tab-page")) {
        page.classList.toggle("active", page.id === `tab-${tabName}`);
    }

    for (const btn of document.querySelectorAll(".tab-btn")) {
        btn.classList.toggle("active", btn.dataset.tab === tabName);
    }
}

function switchPoolView(poolName) {
    for (const view of document.querySelectorAll("[id^='poolView-']")) {
        view.hidden = view.id !== `poolView-${poolName}`;
    }

    for (const btn of document.querySelectorAll(".pool-type-btn")) {
        btn.classList.toggle("active", btn.dataset.pool === poolName);
    }
}

function connectWs() {
    const url = `ws://${location.host}/ws`;
    ws = new WebSocket(url);
    ws.onopen = () => {
        document.getElementById("wsBadge").textContent = "WS: OK";
        document.getElementById("wsBadge").className = "badge ws-ok";
    };
    ws.onclose = () => {
        document.getElementById("wsBadge").textContent = "WS: OFF";
        document.getElementById("wsBadge").className = "badge ws-err";
        setTimeout(connectWs, 2000);
    };
    ws.onerror = () => ws.close();
    ws.onmessage = ev => {
        const msg = JSON.parse(ev.data);
        if (msg.type === "entry") onEntry(msg.data);
        else if (msg.type === "batch") {
            for (const d of msg.data) onEntry(d);
        }
        else if (msg.type === "config") {
            config = msg.data;
            updateConfigUI();
        }
        else if (msg.type === "stats") updateStats(msg.data);
        else if (msg.type === "world") onWorld(msg.data);
        else if (msg.type === "clear") {
            ringHead = 0;
            ringCount = 0;
            totalReceived = 0;
            knownIds.clear();
            idFilters = {};
            hasIncludedFilter = false;
            pendingEntries.length = 0;
            applyView();
            rebuildFilterTags();
        }
    };
}

function onEntry(e) {
    ringPush(e);
    totalReceived++;

    const key = (e.kind === 0 ? "r" : "p") + e.id;
    const prev = knownIds.get(key);
    if (prev) prev.count++;
    else {
        knownIds.set(key, { id: e.id, name: e.name, kind: e.kind, count: 1 });
        rebuildFilterTags();
    }

    if (!paused) {
        pendingEntries.push(e);
        scheduleFlush();
    }
}

function scheduleFlush() {
    if (!rafId) rafId = requestAnimationFrame(flushPending);
}

function flushPending() {
    rafId = 0;
    if (pendingEntries.length === 0) return;

    const batch = pendingEntries;
    pendingEntries = [];
    _cachedSearch = _searchBox.value.toLowerCase();

    const filtered = [];
    for (let i = 0; i < batch.length; i++) {
        if (passesFilter(batch[i])) filtered.push(batch[i]);
    }

    if (filtered.length === 0) return;

    _emptyMsg.style.display = "none";
    const html = filtered.map(buildRowHtml).join("");
    _tbody.insertAdjacentHTML("beforeend", html);

    const children = _tbody.children;
    const newCount = children.length;
    for (let i = newCount - filtered.length; i < newCount; i++) {
        children[i]._entry = filtered[i - (newCount - filtered.length)];
    }

    domRowCount = newCount;

    if (domRowCount > MAX_DOM_ROWS) {
        const excess = domRowCount - MAX_DOM_ROWS;
        for (let i = 0; i < excess; i++) _tbody.removeChild(_tbody.firstChild);
        domRowCount = MAX_DOM_ROWS;
    }

    if (autoScroll) _tableBody.scrollTop = _tableBody.scrollHeight;
    _entryInfo.textContent = `${domRowCount} shown (${ringCount} cached, ${totalReceived} total)`;
}

function buildRowHtml(e) {
    const dirCls = e.direction === 0 ? "dir-in" : "dir-out";
    const dirTxt = e.direction === 0 ? "IN" : "OUT";
    const kindCls = e.kind === 0 ? "kind-rpc" : "kind-pkt";
    const kindTxt = e.kind === 0 ? "RPC" : "PKT";
    return `<tr data-seq="${e.seq}"><td class="time-col">${e.seq}</td><td class="${dirCls}">${dirTxt}</td><td class="${kindCls}">${kindTxt}</td><td class="id-col">${e.id}</td><td class="name-col" title="${esc(e.name)}">${esc(e.name) || "?"}</td><td class="parsed-col" title="${esc(e.parsed)}">${esc(e.parsed) || ""}</td><td class="detail-col" title="${esc(e.detail)}">${esc(e.detail) || ""}</td><td class="size-col">${e.dataBytes}B</td><td class="time-col">now</td></tr>`;
}

function passesFilter(e) {
    const key = (e.kind === 0 ? "r" : "p") + e.id;
    if (hasIncludedFilter) {
        if (idFilters[key] !== "included") return false;
    } else if (idFilters[key] === "excluded") {
        return false;
    }

    const search = _cachedSearch;
    if (search && !(e.name || "").toLowerCase().includes(search) && !String(e.id).includes(search)) return false;
    return true;
}

function scheduleApplyView() {
    clearTimeout(_applyViewTimer);
    _applyViewTimer = setTimeout(applyView, 150);
}

function applyView() {
    _cachedSearch = _searchBox.value.toLowerCase();

    const rows = [];
    for (let i = ringCount - 1; i >= 0 && rows.length < MAX_DOM_ROWS; i--) {
        const e = ringGet(i);
        if (passesFilter(e)) rows.push(e);
    }
    rows.reverse();

    if (rows.length === 0) {
        _tbody.innerHTML = "";
        domRowCount = 0;
        _emptyMsg.style.display = "";
        updateInfo();
        return;
    }

    _emptyMsg.style.display = "none";
    _tbody.innerHTML = rows.map(buildRowHtml).join("");

    const trs = _tbody.children;
    for (let i = 0; i < trs.length; i++) trs[i]._entry = rows[i];
    domRowCount = trs.length;

    if (selectedSeq !== null) {
        const sel = _tbody.querySelector(`tr[data-seq="${selectedSeq}"]`);
        if (sel) sel.classList.add("selected");
    }

    if (autoScroll) _tableBody.scrollTop = _tableBody.scrollHeight;
    updateInfo();
}

document.addEventListener("click", ev => {
    const tr = ev.target.closest("#tbody tr");
    if (!tr || !tr._entry) return;
    selectEntry(tr._entry);
});

document.addEventListener("click", ev => {
    const tr = ev.target.closest(".world-main tbody tr[data-world-key]");
    if (!tr || !tr._worldEntry) return;
    selectWorldEntry(tr.dataset.worldKey, tr._worldEntry);
});

function selectEntry(e) {
    selectedSeq = e.seq;
    const prev = _tbody.querySelector("tr.selected");
    if (prev) prev.classList.remove("selected");
    const row = _tbody.querySelector(`tr[data-seq="${e.seq}"]`);
    if (row) row.classList.add("selected");

    const dv = document.getElementById("detailView");
    dv.style.display = "block";
    const dir = e.direction === 0 ? "Incoming" : "Outgoing";
    const kind = e.kind === 0 ? "RPC" : "Packet";
    let text = `${dir} ${kind} #${e.seq}\nID: ${e.id} (${e.name || "Unknown"})\nSize: ${e.dataBytes} bytes\n`;
    if (e.parsed) text += `\nParsed:\n${e.parsed}`;
    if (e.detail) text += `\nDetail:\n${e.detail}`;
    dv.textContent = text;
}

function recalcHasIncluded() {
    hasIncludedFilter = false;
    for (const k in idFilters) {
        if (idFilters[k] === "included") {
            hasIncludedFilter = true;
            return;
        }
    }
}

function rebuildFilterTags() {
    const container = document.getElementById("filterTags");
    const sorted = [...knownIds.entries()].sort((a, b) => b[1].count - a[1].count);
    const parts = [];
    for (const [key, info] of sorted) {
        const cls = idFilters[key] === "excluded" ? " excluded" : idFilters[key] === "included" ? " included" : "";
        const label = (info.kind === 0 ? "R" : "P") + ":" + info.id;
        const title = (info.name || "?") + " (" + info.count + "x)";
        parts.push(`<span class="ftag${cls}" data-fkey="${key}" title="${esc(title)}">${label}</span>`);
    }
    container.innerHTML = parts.join("");
}

document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("filterTags").addEventListener("click", ev => {
        const tag = ev.target.closest(".ftag");
        if (!tag) return;
        ev.stopPropagation();
        toggleIdFilter(tag.dataset.fkey);
    });

    document.getElementById("filterTags").addEventListener("dblclick", ev => {
        const tag = ev.target.closest(".ftag");
        if (!tag) return;
        ev.stopPropagation();
        soloIdFilter(tag.dataset.fkey);
    });
});

function toggleIdFilter(key) {
    if (idFilters[key] === "excluded" || idFilters[key] === "included") delete idFilters[key];
    else idFilters[key] = "excluded";
    recalcHasIncluded();
    rebuildFilterTags();
    applyView();
}

function soloIdFilter(key) {
    idFilters = {};
    idFilters[key] = "included";
    recalcHasIncluded();
    rebuildFilterTags();
    applyView();
}

function resetIdFilters() {
    idFilters = {};
    hasIncludedFilter = false;
    rebuildFilterTags();
    applyView();
}

async function postConfig() {
    await fetch("/api/config", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(config)
    });
}

function toggleCapture() {
    config.capture = !config.capture;
    postConfig();
    updateConfigUI();
}

function toggleFilter(k) {
    config[k] = !config[k];
    postConfig();
    updateConfigUI();
}

function togglePause() {
    paused = !paused;
    document.getElementById("btnPause").classList.toggle("active", paused);
    document.getElementById("btnPause").textContent = paused ? "Resume" : "Pause";
    if (!paused) applyView();
}

function toggleAutoScroll() {
    autoScroll = !autoScroll;
    document.getElementById("btnScroll").classList.toggle("active", autoScroll);
}

function clearLogs() {
    ringHead = 0;
    ringCount = 0;
    totalReceived = 0;
    pendingEntries.length = 0;
    _tbody.innerHTML = "";
    domRowCount = 0;
    _emptyMsg.style.display = "";
    updateInfo();
}

async function clearAll() {
    await fetch("/api/clear", { method: "POST" });
    ringHead = 0;
    ringCount = 0;
    totalReceived = 0;
    pendingEntries.length = 0;
    knownIds.clear();
    idFilters = {};
    hasIncludedFilter = false;
    _tbody.innerHTML = "";
    domRowCount = 0;
    _emptyMsg.style.display = "";
    rebuildFilterTags();
    updateInfo();
}

function updateConfigUI() {
    const b = document.getElementById("capBadge");
    b.textContent = config.capture ? "ON" : "OFF";
    b.className = "badge " + (config.capture ? "on" : "off");
    tog("btnCap", config.capture);
    tog("btnIn", config.incoming);
    tog("btnOut", config.outgoing);
    tog("btnRpc", config.rpc);
    tog("btnPkt", config.packets);
}

function tog(id, on) {
    document.getElementById(id).classList.toggle("active", on);
}

function updateStats(s) {
    document.getElementById("sRpcIn").textContent = s.totalInRpc;
    document.getElementById("sRpcOut").textContent = s.totalOutRpc;
    document.getElementById("sPktIn").textContent = s.totalInPkt;
    document.getElementById("sPktOut").textContent = s.totalOutPkt;
    renderTop("topRpc", s.topRpc);
    renderTop("topPkt", s.topPkt);
}

function renderTop(id, items) {
    const ul = document.getElementById(id);
    if (!items || !items.length) {
        ul.innerHTML = '<li class="muted">-</li>';
        return;
    }
    ul.innerHTML = items.slice(0, 10).map(i => `<li><span>${esc(i.name) || "ID:" + i.id}</span><span class="muted">${i.count}</span></li>`).join("");
}

function onWorld(snapshot) {
    latestWorldSnapshot = snapshot;
    renderWorld(snapshot);
}

function renderWorld(snapshot) {
    renderWorldOverview(snapshot.overview ? snapshot.overview.pools : []);
    renderWorldMeta(snapshot);
    renderWorldTable("wPlayerBody", 9, snapshot.players, player => [
        player.id,
        esc(player.name) || (player.isLocal ? "Local Player" : "-"),
        player.score,
        player.ping,
        fmtNumber(player.health),
        fmtNumber(player.armour),
        player.weapon,
        fmtOpt(player.vehicleId),
        player.state
    ], "player");
    renderWorldTable("wVehBody", 8, snapshot.vehicles, vehicle => [
        vehicle.id,
        vehicle.model,
        fmtNumber(vehicle.health),
        fmtVec(vehicle.position),
        vehicle.hasDriver ? "yes" : "no",
        vehicle.isOccupied ? "occupied" : "-",
        `${vehicle.primaryColor}/${vehicle.secondaryColor}`,
        vehicle.sirenEnabled ? "on" : "off"
    ], "vehicle");
    renderWorldTable("wObjBody", 7, snapshot.objects, worldObject => [
        worldObject.id,
        worldObject.model,
        fmtVec(worldObject.position),
        fmtVec(worldObject.rotation),
        fmtNumber(worldObject.distanceToCamera),
        fmtAttachment(worldObject.attachedToVehicleId, worldObject.attachedToObjectId),
        worldObject.materialCount
    ], "object");
    renderWorldTable("wPickupBody", 5, snapshot.pickups, pickup => [
        pickup.index,
        pickup.model,
        pickup.type,
        fmtVec(pickup.position),
        "-"
    ], "pickup");
    renderWorldTable("wLabelBody", 6, snapshot.labels, label => [
        label.id,
        esc(trimText(label.text, 80)) || "-",
        fmtColor(label.color),
        fmtVec(label.position),
        fmtNumber(label.drawDistance),
        fmtLabelAttachment(label)
    ], "label");
    renderWorldTable("wTdBody", 6, snapshot.textDraws, textDraw => [
        textDraw.id,
        esc(trimText(textDraw.text, 80)) || "-",
        textDraw.style,
        `(${fmtCompact(textDraw.x)}, ${fmtCompact(textDraw.y)})`,
        textDraw.model,
        fmtColor(textDraw.color)
    ], "textdraw");
    renderWorldTable("wGzBody", 7, snapshot.gangZones, zone => [
        zone.id,
        fmtCompact(zone.minX),
        fmtCompact(zone.minY),
        fmtCompact(zone.maxX),
        fmtCompact(zone.maxY),
        fmtColor(zone.color),
        zone.isFlashing ? "yes" : "no"
    ], "gangzone");
    renderWorldTable("wActorBody", 6, snapshot.actors, actor => [
        actor.id,
        "-",
        fmtNumber(actor.health),
        fmtVec(actor.position),
        fmtCompact(actor.rotation),
        actor.isInvulnerable ? "yes" : "no"
    ], "actor");
}

function renderWorldOverview(pools) {
    const idMap = {
        players: "Players",
        vehicles: "Vehicles",
        objects: "Objects",
        pickups: "Pickups",
        labels: "Labels",
        textdraws: "Textdraws",
        gangzones: "Gangzones",
        actors: "Actors",
        menus: "Menus"
    };

    for (const pool of pools || []) {
        const suffix = idMap[pool.key];
        if (!suffix) continue;
        const countEl = document.getElementById(`wPool${suffix}`);
        const barEl = document.getElementById(`wBar${suffix}`);
        if (countEl) countEl.textContent = `${pool.count} / ${pool.max}`;
        if (barEl) {
            const pct = pool.max > 0 ? Math.max(0, Math.min(100, (pool.count / pool.max) * 100)) : 0;
            barEl.style.width = `${pct}%`;
        }
    }
}

function renderWorldMeta(snapshot) {
    const worldStatus = document.getElementById("worldStatus");
    const gameState = document.getElementById("wGameState");
    const localPlayer = document.getElementById("wLocalPlayer");
    if (worldStatus) {
        worldStatus.textContent = snapshot.status === "live"
            ? "Live snapshot"
            : snapshot.status === "not-initialized"
                ? "Pools not initialized"
                : "Pools unavailable";
    }
    if (gameState) {
        gameState.textContent = `${snapshot.gameState}`;
    }
    if (localPlayer) {
        localPlayer.textContent = snapshot.localPlayer && snapshot.localPlayer.isConnected
            ? `${snapshot.localPlayer.id}: ${snapshot.localPlayer.name || "-"}`
            : "---";
    }
}

function renderWorldTable(bodyId, colCount, items, cellsFactory, kind) {
    const tbody = document.getElementById(bodyId);
    if (!tbody) return;

    const rows = items || [];
    if (!rows.length) {
        tbody.innerHTML = `<tr><td colspan="${colCount}" class="empty">No data.</td></tr>`;
        return;
    }

    tbody.innerHTML = rows.map(item => {
        const key = `${kind}:${item.id ?? item.index}`;
        const selected = worldSelectionKey === key ? " class=\"selected\"" : "";
        const cells = cellsFactory(item).map(cell => `<td>${cell ?? "-"}</td>`).join("");
        return `<tr data-world-key="${key}"${selected}>${cells}</tr>`;
    }).join("");

    const trs = tbody.querySelectorAll("tr[data-world-key]");
    for (let i = 0; i < trs.length; i++) {
        trs[i]._worldEntry = rows[i];
    }
}

function selectWorldEntry(key, entry) {
    worldSelectionKey = key;
    for (const row of document.querySelectorAll(".world-main tbody tr.selected")) {
        row.classList.remove("selected");
    }
    const selectedRow = document.querySelector(`.world-main tbody tr[data-world-key="${key}"]`);
    if (selectedRow) selectedRow.classList.add("selected");

    const detail = document.getElementById("worldDetailView");
    if (!detail) return;
    detail.style.display = "block";
    detail.textContent = formatWorldDetail(key, entry);
}

function formatWorldDetail(key, entry) {
    const lines = [`${key}`];
    for (const [name, value] of Object.entries(entry || {})) {
        lines.push(`${name}: ${formatDetailValue(value)}`);
    }
    return lines.join("\n");
}

function formatDetailValue(value) {
    if (value === null || value === undefined) return "-";
    if (typeof value === "object") {
        if ("x" in value && "y" in value && "z" in value) {
            return fmtVec(value);
        }
        return JSON.stringify(value);
    }
    if (typeof value === "number") {
        return Number.isInteger(value) ? `${value}` : fmtCompact(value);
    }
    if (typeof value === "boolean") {
        return value ? "true" : "false";
    }
    return `${value}`;
}

function fmtVec(v) {
    if (!v) return "-";
    return `(${fmtCompact(v.x)}, ${fmtCompact(v.y)}, ${fmtCompact(v.z)})`;
}

function fmtCompact(v) {
    return Number(v ?? 0).toFixed(2).replace(/\.00$/, "");
}

function fmtNumber(v) {
    return v === null || v === undefined ? "-" : fmtCompact(v);
}

function fmtOpt(v) {
    return v === null || v === undefined ? "-" : `${v}`;
}

function fmtColor(value) {
    const num = Number(value >>> 0);
    return `0x${num.toString(16).toUpperCase().padStart(8, "0")}`;
}

function fmtAttachment(vehicleId, objectId) {
    if (vehicleId !== null && vehicleId !== undefined) return `vehicle:${vehicleId}`;
    if (objectId !== null && objectId !== undefined) return `object:${objectId}`;
    return "-";
}

function fmtLabelAttachment(label) {
    if (label.attachedToPlayer !== null && label.attachedToPlayer !== undefined) {
        return `player:${label.attachedToPlayer}`;
    }
    if (label.attachedToVehicle !== null && label.attachedToVehicle !== undefined) {
        return `vehicle:${label.attachedToVehicle}`;
    }
    return "-";
}

function trimText(value, maxLength) {
    if (!value) return value;
    return value.length > maxLength ? `${value.slice(0, maxLength - 3)}...` : value;
}

function updateInfo() {
    _entryInfo.textContent = `${domRowCount} shown (${ringCount} cached, ${totalReceived} total)`;
}

function esc(s) {
    return s ? s.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;") : "";
}

setInterval(async () => {
    try {
        const r = await fetch("/api/stats");
        const s = await r.json();
        updateStats(s);
    } catch {
    }
}, 3000);

initRefs();
switchTab("packets");
switchPoolView("overview");
connectWs();
