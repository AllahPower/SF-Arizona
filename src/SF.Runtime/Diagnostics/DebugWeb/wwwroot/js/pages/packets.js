import { store } from "../store.js";
import { esc } from "../utils.js";
import { postConfig, clearServer, fetchStats } from "../api.js";
import { ws } from "../ws.js";

const MAX_DOM_ROWS = 1000;
let pendingEntries = [];
let rafId = 0;
let domRowCount = 0;
let _tbody, _tableBody, _emptyMsg, _searchBox;
let _cachedSearch = "";
let _applyViewTimer = 0;
let _statsTimer = 0;

function render() {
    return `
<div class="toolbar">
    <button id="btnCap">Capture</button>
    <button id="btnIn">Incoming</button>
    <button id="btnOut">Outgoing</button>
    <button id="btnRpc">RPC</button>
    <button id="btnPkt">Packets</button>
    <div class="sep"></div>
    <input type="text" id="searchBox" placeholder="Filter by name/id...">
    <button id="btnPause">Pause</button>
    <button class="warn" id="btnClearLogs">Clear Logs</button>
    <button class="danger" id="btnClearAll">Clear All</button>
    <button id="btnScroll" class="active">Auto-scroll</button>
</div>
<div class="content">
    <div class="main" id="mainPanel">
        <div class="table-head">
            <table>
                <colgroup><col style="width:50px"><col style="width:36px"><col style="width:40px"><col style="width:36px"><col style="width:180px"><col><col><col style="width:46px"><col style="width:40px"></colgroup>
                <thead><tr><th>#</th><th>Dir</th><th>Type</th><th>ID</th><th>Name</th><th>Parsed</th><th>Detail</th><th class="right">Size</th><th>Time</th></tr></thead>
            </table>
        </div>
        <div class="table-body" id="tableBody">
            <table>
                <colgroup><col style="width:50px"><col style="width:36px"><col style="width:40px"><col style="width:36px"><col style="width:180px"><col><col><col style="width:46px"><col style="width:40px"></colgroup>
                <tbody id="tbody"></tbody>
            </table>
            <div class="empty" id="emptyMsg">Enable capture to start collecting traffic.</div>
        </div>
    </div>
    <div class="sidebar">
        <h3>Counters</h3>
        <div class="stat-row">
            <div class="stat-card"><div class="label">RPC In</div><div class="value" id="sRpcIn">0</div></div>
            <div class="stat-card"><div class="label">RPC Out</div><div class="value" id="sRpcOut">0</div></div>
        </div>
        <div class="stat-row">
            <div class="stat-card"><div class="label">Pkt In</div><div class="value" id="sPktIn">0</div></div>
            <div class="stat-card"><div class="label">Pkt Out</div><div class="value" id="sPktOut">0</div></div>
        </div>
        <h3>ID Filter</h3>
        <div class="filter-panel">
            <label>Click to exclude, double-click to solo. Right-click to reset.</label>
            <div class="filter-tags" id="filterTags"></div>
        </div>
        <h3>Selected Entry</h3>
        <div class="detail-view" id="detailView">Select a row to inspect.</div>
        <h3>Top RPC</h3>
        <ul class="top-list" id="topRpc"><li class="muted">-</li></ul>
        <h3>Top Packets</h3>
        <ul class="top-list" id="topPkt"><li class="muted">-</li></ul>
    </div>
</div>`;
}

function mount(root) {
    _tbody = root.querySelector("#tbody");
    _tableBody = root.querySelector("#tableBody");
    _emptyMsg = root.querySelector("#emptyMsg");
    _searchBox = root.querySelector("#searchBox");

    _searchBox.addEventListener("input", () => scheduleApplyView());

    root.querySelector("#btnCap").addEventListener("click", () => {
        store.serverSettings.capture = !store.serverSettings.capture;
        postConfig();
        updateConfigUI(root);
    });
    root.querySelector("#btnIn").addEventListener("click", () => toggleFilter("incoming", root));
    root.querySelector("#btnOut").addEventListener("click", () => toggleFilter("outgoing", root));
    root.querySelector("#btnRpc").addEventListener("click", () => toggleFilter("rpc", root));
    root.querySelector("#btnPkt").addEventListener("click", () => toggleFilter("packets", root));

    root.querySelector("#btnPause").addEventListener("click", () => {
        store.paused = !store.paused;
        const btn = root.querySelector("#btnPause");
        btn.classList.toggle("active", store.paused);
        btn.textContent = store.paused ? "Resume" : "Pause";
        if (!store.paused) applyView();
        ws.pushPacketViewState();
    });

    root.querySelector("#btnScroll").addEventListener("click", () => {
        store.autoScroll = !store.autoScroll;
        root.querySelector("#btnScroll").classList.toggle("active", store.autoScroll);
        ws.pushPacketViewState();
    });

    root.querySelector("#btnClearLogs").addEventListener("click", () => {
        store.resetPackets();
        pendingEntries.length = 0;
        _tbody.innerHTML = "";
        domRowCount = 0;
        _emptyMsg.style.display = "";
        updateInfo();
    });

    root.querySelector("#btnClearAll").addEventListener("click", async () => {
        await clearServer();
        store.resetAll();
        pendingEntries.length = 0;
        _tbody.innerHTML = "";
        domRowCount = 0;
        _emptyMsg.style.display = "";
        rebuildFilterTags(root);
        updateInfo();
    });

    const filterTags = root.querySelector("#filterTags");
    filterTags.addEventListener("contextmenu", ev => {
        ev.preventDefault();
        store.idFilters = {};
        store.hasIncludedFilter = false;
        rebuildFilterTags(root);
        applyView();
        ws.pushPacketViewState();
    });
    filterTags.addEventListener("click", ev => {
        const tag = ev.target.closest(".ftag");
        if (!tag) return;
        const key = tag.dataset.fkey;
        if (store.idFilters[key] === "excluded" || store.idFilters[key] === "included") delete store.idFilters[key];
        else store.idFilters[key] = "excluded";
        store.recalcHasIncluded();
        rebuildFilterTags(root);
        applyView();
        ws.pushPacketViewState();
    });
    filterTags.addEventListener("dblclick", ev => {
        const tag = ev.target.closest(".ftag");
        if (!tag) return;
        store.idFilters = {};
        store.idFilters[tag.dataset.fkey] = "included";
        store.recalcHasIncluded();
        rebuildFilterTags(root);
        applyView();
        ws.pushPacketViewState();
    });

    root.addEventListener("click", ev => {
        const tr = ev.target.closest("#tbody tr");
        if (!tr || !tr._entry) return;
        selectEntry(tr._entry, root);
    });

    store.on("traffic:entry", onEntry);
    store.on("settings:update", () => updateConfigUI(root));
    store.on("stats:update", s => updateStats(s, root));
    store.on("traffic:clear", () => {
        pendingEntries.length = 0;
        _tbody.innerHTML = "";
        domRowCount = 0;
        _emptyMsg.style.display = "";
        rebuildFilterTags(root);
        updateInfo();
    });

    _statsTimer = setInterval(async () => {
        try {
            const s = await fetchStats();
            updateStats(s, root);
        } catch {}
    }, 3000);

    updateConfigUI(root);
}

function onShow() {}

function onHide() {}

function onEntry(e) {
    store.ringPush(e);
    store.totalReceived++;

    const key = (e.kind === 0 ? "r" : "p") + e.id;
    const prev = store.knownIds.get(key);
    if (prev) prev.count++;
    else {
        store.knownIds.set(key, { id: e.id, name: e.name, kind: e.kind, count: 1 });
        const root = document.getElementById("page-packets");
        if (root) rebuildFilterTags(root);
    }

    if (!store.paused) {
        pendingEntries.push(e);
        if (!rafId) rafId = requestAnimationFrame(flushPending);
    }
}

function flushPending() {
    rafId = 0;
    if (pendingEntries.length === 0) return;

    const batch = pendingEntries;
    pendingEntries = [];
    _cachedSearch = _searchBox ? _searchBox.value.toLowerCase() : "";

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

    if (store.autoScroll) _tableBody.scrollTop = _tableBody.scrollHeight;
    updateInfo();
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
    if (store.hasIncludedFilter) {
        if (store.idFilters[key] !== "included") return false;
    } else if (store.idFilters[key] === "excluded") {
        return false;
    }
    const search = _cachedSearch;
    if (search && !(e.name || "").toLowerCase().includes(search) && !String(e.id).includes(search)) return false;
    return true;
}

function scheduleApplyView() {
    clearTimeout(_applyViewTimer);
    _applyViewTimer = setTimeout(() => {
        applyView();
        ws.pushPacketViewState();
    }, 150);
}

function applyView() {
    if (!_searchBox || !_tbody) return;
    _cachedSearch = _searchBox.value.toLowerCase();

    const rows = [];
    for (let i = store.ringCount - 1; i >= 0 && rows.length < MAX_DOM_ROWS; i--) {
        const e = store.ringGet(i);
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

    if (store.selectedSeq !== null) {
        const sel = _tbody.querySelector(`tr[data-seq="${store.selectedSeq}"]`);
        if (sel) sel.classList.add("selected");
    }

    if (store.autoScroll) _tableBody.scrollTop = _tableBody.scrollHeight;
    updateInfo();
}

function selectEntry(e, root) {
    store.selectedSeq = e.seq;
    const prev = _tbody.querySelector("tr.selected");
    if (prev) prev.classList.remove("selected");
    const row = _tbody.querySelector(`tr[data-seq="${e.seq}"]`);
    if (row) row.classList.add("selected");

    const dv = root.querySelector("#detailView");
    dv.style.display = "block";
    const dir = e.direction === 0 ? "Incoming" : "Outgoing";
    const kind = e.kind === 0 ? "RPC" : "Packet";
    let text = `${dir} ${kind} #${e.seq}\nID: ${e.id} (${e.name || "Unknown"})\nSize: ${e.dataBytes} bytes\n`;
    if (e.parsed) text += `\nParsed:\n${e.parsed}`;
    if (e.detail) text += `\nDetail:\n${e.detail}`;
    dv.textContent = text;
}

function toggleFilter(key, root) {
    store.serverSettings[key] = !store.serverSettings[key];
    postConfig();
    updateConfigUI(root);
}

function updateConfigUI(root) {
    const badge = document.getElementById("capBadge");
    if (badge) {
        badge.textContent = store.serverSettings.capture ? "ON" : "OFF";
        badge.className = "badge " + (store.serverSettings.capture ? "on" : "off");
    }
    tog(root, "btnCap", store.serverSettings.capture);
    tog(root, "btnIn", store.serverSettings.incoming);
    tog(root, "btnOut", store.serverSettings.outgoing);
    tog(root, "btnRpc", store.serverSettings.rpc);
    tog(root, "btnPkt", store.serverSettings.packets);
}

function tog(root, id, on) {
    const el = root.querySelector(`#${id}`);
    if (el) el.classList.toggle("active", on);
}

function rebuildFilterTags(root) {
    const container = root.querySelector("#filterTags");
    if (!container) return;
    const sorted = [...store.knownIds.entries()].sort((a, b) => b[1].count - a[1].count);
    const parts = [];
    for (const [key, info] of sorted) {
        const cls = store.idFilters[key] === "excluded" ? " excluded" : store.idFilters[key] === "included" ? " included" : "";
        const label = (info.kind === 0 ? "R" : "P") + ":" + info.id;
        const title = (info.name || "?") + " (" + info.count + "x)";
        parts.push(`<span class="ftag${cls}" data-fkey="${key}" title="${esc(title)}">${label}</span>`);
    }
    container.innerHTML = parts.join("");
}

function updateStats(s, root) {
    const set = (id, val) => { const el = root.querySelector(`#${id}`); if (el) el.textContent = val; };
    set("sRpcIn", s.totalInRpc);
    set("sRpcOut", s.totalOutRpc);
    set("sPktIn", s.totalInPkt);
    set("sPktOut", s.totalOutPkt);
    renderTop(root, "topRpc", s.topRpc);
    renderTop(root, "topPkt", s.topPkt);
}

function renderTop(root, id, items) {
    const ul = root.querySelector(`#${id}`);
    if (!ul) return;
    if (!items || !items.length) {
        ul.innerHTML = '<li class="muted">-</li>';
        return;
    }
    ul.innerHTML = items.slice(0, 10).map(i => `<li><span>${esc(i.name) || "ID:" + i.id}</span><span class="muted">${i.count}</span></li>`).join("");
}

function updateInfo() {
    const el = document.getElementById("entryInfo");
    if (el) el.textContent = `${domRowCount} shown (${store.ringCount} cached, ${store.totalReceived} total)`;
}

export const PacketsPage = { render, mount, onShow, onHide };
