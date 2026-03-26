public partial class DebugModule
{
    private const string DashboardHtml = """
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>SF-Arizona Network Debug</title>
<style>
*{margin:0;padding:0;box-sizing:border-box}
body{background:#0d1117;color:#c9d1d9;font-family:'Segoe UI',system-ui,sans-serif;font-size:14px}
::-webkit-scrollbar{width:8px;height:8px}
::-webkit-scrollbar-track{background:#0d1117}
::-webkit-scrollbar-thumb{background:#30363d;border-radius:4px}
::-webkit-scrollbar-thumb:hover{background:#484f58}
::-webkit-scrollbar-corner{background:#0d1117}
*{scrollbar-width:thin;scrollbar-color:#30363d #0d1117}
.header{background:#161b22;border-bottom:1px solid #30363d;padding:10px 20px;display:flex;align-items:center;gap:12px}
.header h1{font-size:16px;color:#58a6ff;font-weight:600}
.badge{font-size:11px;padding:2px 8px;border-radius:10px;font-weight:500}
.badge.on{background:#238636;color:#fff}
.badge.off{background:#da3633;color:#fff}
.badge.ws-ok{background:#1f6feb;color:#fff}
.badge.ws-err{background:#6e7681;color:#fff}
.toolbar{background:#161b22;border-bottom:1px solid #30363d;padding:8px 20px;display:flex;gap:6px;flex-wrap:wrap;align-items:center}
.toolbar .sep{width:1px;height:20px;background:#30363d;margin:0 4px}
button{background:#21262d;color:#c9d1d9;border:1px solid #30363d;padding:5px 12px;border-radius:6px;cursor:pointer;font-size:12px;transition:all .12s}
button:hover{background:#30363d;border-color:#8b949e}
button.active{background:#1f6feb;border-color:#1f6feb;color:#fff}
button.danger{border-color:#da3633}
button.danger:hover{background:#da3633;color:#fff}
button.warn{border-color:#d29922}
button.warn:hover{background:#d29922;color:#fff}
input[type=text]{background:#0d1117;color:#c9d1d9;border:1px solid #30363d;padding:5px 10px;border-radius:6px;font-size:12px;width:180px}
input[type=text]:focus{outline:none;border-color:#1f6feb}
select{background:#21262d;color:#c9d1d9;border:1px solid #30363d;padding:5px 8px;border-radius:6px;font-size:12px;cursor:pointer}
.content{display:flex;height:calc(100vh - 88px)}
.main{flex:1;overflow:hidden;padding:0;display:flex;flex-direction:column}
.table-head{flex:0 0 auto;overflow:hidden}
.table-head table,.table-body table{width:100%;border-collapse:collapse;table-layout:fixed}
.table-body{flex:1;overflow-y:auto;overflow-x:hidden}
.sidebar{width:260px;background:#161b22;border-left:1px solid #30363d;padding:12px;overflow:auto}
.sidebar h3{color:#8b949e;font-size:10px;text-transform:uppercase;letter-spacing:.5px;margin:12px 0 6px}
.sidebar h3:first-child{margin-top:0}
.stat-row{display:flex;gap:6px;margin-bottom:6px}
.stat-card{flex:1;background:#0d1117;border:1px solid #21262d;border-radius:6px;padding:8px}
.stat-card .label{color:#6e7681;font-size:10px}
.stat-card .value{color:#f0f6fc;font-size:18px;font-weight:600}
th{background:#161b22;color:#8b949e;font-size:10px;text-transform:uppercase;letter-spacing:.4px;padding:6px 8px;text-align:left;border-bottom:1px solid #30363d}
td{padding:4px 8px;border-bottom:1px solid #1b2028;font-size:12px;white-space:nowrap}
tr:hover td{background:#161b22}
tr.selected td{background:#1c2333}
.dir-in{color:#58a6ff}.dir-out{color:#d29922}
.kind-rpc{color:#bc8cff}.kind-pkt{color:#f0883e}
.id-col{color:#f0f6fc;font-weight:500}
.name-col{color:#8b949e;overflow:hidden;text-overflow:ellipsis}
.detail-col{color:#6e7681;overflow:hidden;text-overflow:ellipsis;font-family:'Cascadia Code',Consolas,monospace;font-size:11px}
.parsed-col{color:#7ee787;overflow:hidden;text-overflow:ellipsis;font-family:'Cascadia Code',Consolas,monospace;font-size:11px}
.size-col{color:#c9d1d9;text-align:right}
.time-col{color:#484f58;font-size:11px}
.empty{text-align:center;padding:40px;color:#484f58}
.top-list{list-style:none}
.top-list li{display:flex;justify-content:space-between;padding:3px 0;border-bottom:1px solid #1b2028;font-size:11px}
.top-list li:last-child{border:none}
.filter-panel{background:#0d1117;border:1px solid #21262d;border-radius:6px;padding:8px;margin-bottom:8px}
.filter-panel label{display:block;font-size:11px;color:#8b949e;margin-bottom:4px}
.filter-tags{display:flex;flex-wrap:wrap;gap:3px;max-height:120px;overflow:auto}
.ftag{font-size:10px;padding:2px 6px;border-radius:4px;cursor:pointer;border:1px solid #30363d;background:#21262d;color:#8b949e;transition:all .1s}
.ftag:hover{border-color:#8b949e}
.ftag.excluded{background:#3d1f20;border-color:#da3633;color:#f85149;text-decoration:line-through}
.ftag.included{background:#1a3a2a;border-color:#238636;color:#7ee787}
.info{color:#484f58;font-size:11px;padding:8px 20px;text-align:right}
.detail-view{background:#0d1117;border:1px solid #30363d;border-radius:6px;padding:10px;margin-top:8px;font-family:'Cascadia Code',Consolas,monospace;font-size:11px;color:#c9d1d9;white-space:pre-wrap;word-break:break-all;max-height:200px;overflow:auto;display:none}
</style>
</head>
<body>

<div class="header">
    <h1>SF-Arizona Network Debug</h1>
    <span class="badge" id="capBadge">OFF</span>
    <span class="badge ws-err" id="wsBadge">WS: ...</span>
    <span style="margin-left:auto;color:#484f58;font-size:11px" id="entryInfo">0 entries (0 total)</span>
</div>

<div class="toolbar">
    <button id="btnCap" onclick="toggleCapture()">Capture</button>
    <button id="btnIn" onclick="toggleFilter('incoming')">Incoming</button>
    <button id="btnOut" onclick="toggleFilter('outgoing')">Outgoing</button>
    <button id="btnRpc" onclick="toggleFilter('rpc')">RPC</button>
    <button id="btnPkt" onclick="toggleFilter('packets')">Packets</button>
    <div class="sep"></div>
    <input type="text" id="searchBox" placeholder="Filter by name/id..." oninput="scheduleApplyView()">
    <button id="btnPause" onclick="togglePause()">Pause</button>
    <button class="warn" onclick="clearLogs()">Clear Logs</button>
    <button class="danger" onclick="clearAll()">Clear All</button>
    <button onclick="toggleAutoScroll()" id="btnScroll" class="active">Auto-scroll</button>
</div>

<div class="content">
    <div class="main" id="mainPanel">
        <div class="table-head">
            <table>
                <colgroup><col style="width:50px"><col style="width:36px"><col style="width:40px"><col style="width:36px"><col style="width:180px"><col><col><col style="width:46px"><col style="width:40px"></colgroup>
                <thead>
                    <tr><th>#</th><th>Dir</th><th>Type</th><th>ID</th><th>Name</th><th>Parsed</th><th>Detail</th><th style="text-align:right">Size</th><th>Time</th></tr>
                </thead>
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
            <div class="filter-tags" id="filterTags" oncontextmenu="resetIdFilters();return false;"></div>
        </div>

        <h3>Selected Entry</h3>
        <div class="detail-view" id="detailView">Select a row to inspect.</div>

        <h3>Top RPC</h3>
        <ul class="top-list" id="topRpc"><li style="color:#484f58">—</li></ul>

        <h3>Top Packets</h3>
        <ul class="top-list" id="topPkt"><li style="color:#484f58">—</li></ul>
    </div>
</div>

<script>
const MAX_ENTRIES = 50000;
const MAX_DOM_ROWS = 1000;

// --- Ring buffer for entries (avoids splice/shift on large arrays) ---
let ring = new Array(MAX_ENTRIES);
let ringHead = 0;   // write position (next slot)
let ringCount = 0;  // entries currently in ring
let totalReceived = 0;

function ringPush(e) {
    ring[ringHead] = e;
    ringHead = (ringHead + 1) % MAX_ENTRIES;
    if (ringCount < MAX_ENTRIES) ringCount++;
}
function ringGet(i) {
    // i=0 is oldest, i=ringCount-1 is newest
    return ring[(ringHead - ringCount + i + MAX_ENTRIES) % MAX_ENTRIES];
}

let config = {capture:false,incoming:true,outgoing:true,rpc:true,packets:true};
let paused = false;
let autoScroll = true;
let ws = null;
let selectedSeq = null;

// ID filter
let idFilters = {};
let hasIncludedFilter = false;
let knownIds = new Map();

// --- rAF batch queue: incoming entries accumulate here, flushed once per frame ---
let pendingEntries = [];
let rafId = 0;
let domRowCount = 0; // track without querying DOM

// cached DOM refs (resolved once)
let _tbody, _tableBody, _emptyMsg, _entryInfo, _searchBox;
function initRefs() {
    _tbody = document.getElementById('tbody');
    _tableBody = document.getElementById('tableBody');
    _emptyMsg = document.getElementById('emptyMsg');
    _entryInfo = document.getElementById('entryInfo');
    _searchBox = document.getElementById('searchBox');
}

function connectWs() {
    const url = `ws://${location.host}/ws`;
    ws = new WebSocket(url);
    ws.onopen = () => { document.getElementById('wsBadge').textContent='WS: OK'; document.getElementById('wsBadge').className='badge ws-ok'; };
    ws.onclose = () => { document.getElementById('wsBadge').textContent='WS: OFF'; document.getElementById('wsBadge').className='badge ws-err'; setTimeout(connectWs, 2000); };
    ws.onerror = () => ws.close();
    ws.onmessage = (ev) => {
        const msg = JSON.parse(ev.data);
        if (msg.type === 'entry') onEntry(msg.data);
        else if (msg.type === 'batch') { for (const d of msg.data) onEntry(d); }
        else if (msg.type === 'config') { config = msg.data; updateConfigUI(); }
        else if (msg.type === 'stats') updateStats(msg.data);
        else if (msg.type === 'clear') { ringHead=0; ringCount=0; totalReceived=0; knownIds.clear(); idFilters={}; hasIncludedFilter=false; pendingEntries.length=0; applyView(); rebuildFilterTags(); }
    };
}

function onEntry(e) {
    ringPush(e);
    totalReceived++;

    const key = (e.kind===0?'r':'p') + e.id;
    const prev = knownIds.get(key);
    if (prev) prev.count++;
    else { knownIds.set(key, {id:e.id, name:e.name, kind:e.kind, count:1}); rebuildFilterTags(); }

    if (!paused) {
        pendingEntries.push(e);
        scheduleFlush();
    }
}

// --- rAF flush: one DOM write per frame ---
function scheduleFlush() {
    if (!rafId) rafId = requestAnimationFrame(flushPending);
}

function flushPending() {
    rafId = 0;
    if (pendingEntries.length === 0) return;

    const batch = pendingEntries;
    pendingEntries = [];

    // filter batch
    _cachedSearch = _searchBox.value.toLowerCase();
    const filtered = [];
    for (let i = 0; i < batch.length; i++) {
        if (passesFilter(batch[i])) filtered.push(batch[i]);
    }
    if (filtered.length === 0) return;

    _emptyMsg.style.display = 'none';

    // build HTML for all new rows at once
    const html = filtered.map(buildRowHtml).join('');
    _tbody.insertAdjacentHTML('beforeend', html);

    // assign _entry refs for click delegation
    const children = _tbody.children;
    const newCount = children.length;
    for (let i = newCount - filtered.length; i < newCount; i++) {
        children[i]._entry = filtered[i - (newCount - filtered.length)];
    }

    domRowCount = newCount;

    // trim excess rows — remove from front
    if (domRowCount > MAX_DOM_ROWS) {
        const excess = domRowCount - MAX_DOM_ROWS;
        for (let i = 0; i < excess; i++) _tbody.removeChild(_tbody.firstChild);
        domRowCount = MAX_DOM_ROWS;
    }

    // single scroll + info update after all DOM work
    if (autoScroll) _tableBody.scrollTop = _tableBody.scrollHeight;
    _entryInfo.textContent = `${domRowCount} shown (${ringCount} cached, ${totalReceived} total)`;
}

function buildRowHtml(e) {
    const dirCls = e.direction===0?'dir-in':'dir-out';
    const dirTxt = e.direction===0?'IN':'OUT';
    const kindCls = e.kind===0?'kind-rpc':'kind-pkt';
    const kindTxt = e.kind===0?'RPC':'PKT';
    return `<tr data-seq="${e.seq}"><td class="time-col">${e.seq}</td><td class="${dirCls}">${dirTxt}</td><td class="${kindCls}">${kindTxt}</td><td class="id-col">${e.id}</td><td class="name-col" title="${esc(e.name)}">${esc(e.name)||'?'}</td><td class="parsed-col" title="${esc(e.parsed)}">${esc(e.parsed)||''}</td><td class="detail-col" title="${esc(e.detail)}">${esc(e.detail)||''}</td><td class="size-col">${e.dataBytes}B</td><td class="time-col">now</td></tr>`;
}

function passesFilter(e) {
    const key = (e.kind===0?'r':'p') + e.id;
    if (hasIncludedFilter) { if (idFilters[key] !== 'included') return false; }
    else if (idFilters[key] === 'excluded') return false;

    const search = _cachedSearch;
    if (search && !(e.name||'').toLowerCase().includes(search) && !String(e.id).includes(search)) return false;
    return true;
}

let _cachedSearch = '';
let _applyViewTimer = 0;
function scheduleApplyView() {
    clearTimeout(_applyViewTimer);
    _applyViewTimer = setTimeout(applyView, 150);
}

function applyView() {
    _cachedSearch = _searchBox.value.toLowerCase();

    // collect last MAX_DOM_ROWS matching entries from ring buffer (newest first)
    const rows = [];
    for (let i = ringCount - 1; i >= 0 && rows.length < MAX_DOM_ROWS; i--) {
        const e = ringGet(i);
        if (passesFilter(e)) rows.push(e);
    }
    rows.reverse();

    if (rows.length === 0) {
        _tbody.innerHTML = '';
        domRowCount = 0;
        _emptyMsg.style.display = '';
        updateInfo();
        return;
    }

    _emptyMsg.style.display = 'none';
    _tbody.innerHTML = rows.map(buildRowHtml).join('');

    // assign _entry refs
    const trs = _tbody.children;
    for (let i = 0; i < trs.length; i++) trs[i]._entry = rows[i];
    domRowCount = trs.length;

    if (selectedSeq !== null) {
        const sel = _tbody.querySelector(`tr[data-seq="${selectedSeq}"]`);
        if (sel) sel.classList.add('selected');
    }

    if (autoScroll) _tableBody.scrollTop = _tableBody.scrollHeight;
    updateInfo();
}

// event delegation for row clicks
document.addEventListener('click', (ev) => {
    const tr = ev.target.closest('#tbody tr');
    if (!tr || !tr._entry) return;
    selectEntry(tr._entry);
});

function selectEntry(e) {
    selectedSeq = e.seq;
    const prev = _tbody.querySelector('tr.selected');
    if (prev) prev.classList.remove('selected');
    const row = _tbody.querySelector(`tr[data-seq="${e.seq}"]`);
    if (row) row.classList.add('selected');
    const dv = document.getElementById('detailView');
    dv.style.display = 'block';
    const dir = e.direction===0?'Incoming':'Outgoing';
    const kind = e.kind===0?'RPC':'Packet';
    let text = `${dir} ${kind} #${e.seq}\nID: ${e.id} (${e.name||'Unknown'})\nSize: ${e.dataBytes} bytes\n`;
    if (e.parsed) text += `\nParsed:\n${e.parsed}`;
    if (e.detail) text += `\nDetail:\n${e.detail}`;
    dv.textContent = text;
}

// --- ID Filters ---
function recalcHasIncluded() {
    hasIncludedFilter = false;
    for (const k in idFilters) { if (idFilters[k] === 'included') { hasIncludedFilter = true; return; } }
}

function rebuildFilterTags() {
    const container = document.getElementById('filterTags');
    const sorted = [...knownIds.entries()].sort((a,b) => b[1].count - a[1].count);
    const parts = [];
    for (const [key, info] of sorted) {
        const cls = idFilters[key]==='excluded'?' excluded':idFilters[key]==='included'?' included':'';
        const label = (info.kind===0?'R':'P')+':'+info.id;
        const title = (info.name||'?')+' ('+info.count+'x)';
        parts.push(`<span class="ftag${cls}" data-fkey="${key}" title="${esc(title)}">${label}</span>`);
    }
    container.innerHTML = parts.join('');
}

document.getElementById('filterTags').addEventListener('click', (ev) => {
    const tag = ev.target.closest('.ftag');
    if (!tag) return;
    ev.stopPropagation();
    toggleIdFilter(tag.dataset.fkey);
});
document.getElementById('filterTags').addEventListener('dblclick', (ev) => {
    const tag = ev.target.closest('.ftag');
    if (!tag) return;
    ev.stopPropagation();
    soloIdFilter(tag.dataset.fkey);
});

function toggleIdFilter(key) {
    if (idFilters[key] === 'excluded') delete idFilters[key];
    else if (idFilters[key] === 'included') delete idFilters[key];
    else idFilters[key] = 'excluded';
    recalcHasIncluded();
    rebuildFilterTags();
    applyView();
}

function soloIdFilter(key) {
    idFilters = {};
    idFilters[key] = 'included';
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

// --- Config ---
async function postConfig() {
    await fetch('/api/config', {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(config)});
}

function toggleCapture() { config.capture=!config.capture; postConfig(); updateConfigUI(); }
function toggleFilter(k) { config[k]=!config[k]; postConfig(); updateConfigUI(); }
function togglePause() {
    paused=!paused;
    document.getElementById('btnPause').classList.toggle('active',paused);
    document.getElementById('btnPause').textContent=paused?'Resume':'Pause';
    if (!paused) applyView();
}
function toggleAutoScroll() {
    autoScroll=!autoScroll;
    document.getElementById('btnScroll').classList.toggle('active',autoScroll);
}

function clearLogs() {
    ringHead = 0;
    ringCount = 0;
    totalReceived = 0;
    pendingEntries.length = 0;
    _tbody.innerHTML = '';
    domRowCount = 0;
    _emptyMsg.style.display = '';
    updateInfo();
}

async function clearAll() {
    await fetch('/api/clear',{method:'POST'});
    ringHead = 0;
    ringCount = 0;
    totalReceived = 0;
    pendingEntries.length = 0;
    knownIds.clear();
    idFilters = {};
    hasIncludedFilter = false;
    _tbody.innerHTML = '';
    domRowCount = 0;
    _emptyMsg.style.display = '';
    rebuildFilterTags();
    updateInfo();
}

function updateConfigUI() {
    const b = document.getElementById('capBadge');
    b.textContent = config.capture?'ON':'OFF';
    b.className = 'badge '+(config.capture?'on':'off');
    tog('btnCap', config.capture);
    tog('btnIn', config.incoming);
    tog('btnOut', config.outgoing);
    tog('btnRpc', config.rpc);
    tog('btnPkt', config.packets);
}
function tog(id,on){document.getElementById(id).classList.toggle('active',on)}

function updateStats(s) {
    document.getElementById('sRpcIn').textContent=s.totalInRpc;
    document.getElementById('sRpcOut').textContent=s.totalOutRpc;
    document.getElementById('sPktIn').textContent=s.totalInPkt;
    document.getElementById('sPktOut').textContent=s.totalOutPkt;
    renderTop('topRpc', s.topRpc);
    renderTop('topPkt', s.topPkt);
}
function renderTop(id, items) {
    const ul=document.getElementById(id);
    if (!items||!items.length){ul.innerHTML='<li style="color:#484f58">—</li>';return;}
    ul.innerHTML=items.slice(0,10).map(i=>`<li><span style="color:#c9d1d9">${esc(i.name)||'ID:'+i.id}</span><span style="color:#6e7681">${i.count}</span></li>`).join('');
}

function updateInfo() {
    _entryInfo.textContent = `${domRowCount} shown (${ringCount} cached, ${totalReceived} total)`;
}

function esc(s){return s?s.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;'):'';}

// Stats polling
setInterval(async()=>{
    try{const r=await fetch('/api/stats');const s=await r.json();updateStats(s);}catch{}
}, 3000);

initRefs();
connectWs();
</script>
</body>
</html>
""";
}
