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
input[type=text]{background:#0d1117;color:#c9d1d9;border:1px solid #30363d;padding:5px 10px;border-radius:6px;font-size:12px;width:180px}
input[type=text]:focus{outline:none;border-color:#1f6feb}
select{background:#21262d;color:#c9d1d9;border:1px solid #30363d;padding:5px 8px;border-radius:6px;font-size:12px;cursor:pointer}
.content{display:flex;height:calc(100vh - 88px)}
.main{flex:1;overflow:auto;padding:0}
.sidebar{width:260px;background:#161b22;border-left:1px solid #30363d;padding:12px;overflow:auto}
.sidebar h3{color:#8b949e;font-size:10px;text-transform:uppercase;letter-spacing:.5px;margin:12px 0 6px}
.sidebar h3:first-child{margin-top:0}
.stat-row{display:flex;gap:6px;margin-bottom:6px}
.stat-card{flex:1;background:#0d1117;border:1px solid #21262d;border-radius:6px;padding:8px}
.stat-card .label{color:#6e7681;font-size:10px}
.stat-card .value{color:#f0f6fc;font-size:18px;font-weight:600}
table{width:100%;border-collapse:collapse}
thead{position:sticky;top:0;z-index:1}
th{background:#161b22;color:#8b949e;font-size:10px;text-transform:uppercase;letter-spacing:.4px;padding:6px 8px;text-align:left;border-bottom:1px solid #30363d}
td{padding:4px 8px;border-bottom:1px solid #1b2028;font-size:12px;white-space:nowrap}
tr:hover td{background:#161b22}
tr.selected td{background:#1c2333}
.dir-in{color:#58a6ff}.dir-out{color:#d29922}
.kind-rpc{color:#bc8cff}.kind-pkt{color:#f0883e}
.id-col{color:#f0f6fc;font-weight:500}
.name-col{color:#8b949e;max-width:200px;overflow:hidden;text-overflow:ellipsis}
.detail-col{color:#6e7681;max-width:320px;overflow:hidden;text-overflow:ellipsis;font-family:'Cascadia Code',Consolas,monospace;font-size:11px}
.parsed-col{color:#7ee787;max-width:400px;overflow:hidden;text-overflow:ellipsis;font-family:'Cascadia Code',Consolas,monospace;font-size:11px}
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
    <input type="text" id="searchBox" placeholder="Filter by name/id..." oninput="applyView()">
    <button id="btnPause" onclick="togglePause()">Pause</button>
    <button class="danger" onclick="clearAll()">Clear</button>
    <button onclick="toggleAutoScroll()" id="btnScroll" class="active">Auto-scroll</button>
</div>

<div class="content">
    <div class="main" id="mainPanel">
        <table>
            <thead>
                <tr>
                    <th style="width:36px">#</th>
                    <th style="width:36px">Dir</th>
                    <th style="width:36px">Type</th>
                    <th style="width:50px">ID</th>
                    <th style="width:160px">Name</th>
                    <th>Parsed</th>
                    <th>Detail</th>
                    <th style="width:50px;text-align:right">Size</th>
                    <th style="width:70px">Time</th>
                </tr>
            </thead>
            <tbody id="tbody"></tbody>
        </table>
        <div class="empty" id="emptyMsg">Enable capture to start collecting traffic.</div>
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
const MAX_CLIENT_ENTRIES = 5000;
let entries = [];
let totalReceived = 0;
let config = {capture:false,incoming:true,outgoing:true,rpc:true,packets:true};
let paused = false;
let autoScroll = true;
let ws = null;
let selectedSeq = null;

// ID filter: null=show, 'excluded'=hide, 'included'=solo
let idFilters = {};
let knownIds = new Map(); // id -> {name, kind, count}

function connectWs() {
    const url = `ws://${location.host}/ws`;
    ws = new WebSocket(url);
    ws.onopen = () => { document.getElementById('wsBadge').textContent='WS: OK'; document.getElementById('wsBadge').className='badge ws-ok'; };
    ws.onclose = () => { document.getElementById('wsBadge').textContent='WS: OFF'; document.getElementById('wsBadge').className='badge ws-err'; setTimeout(connectWs, 2000); };
    ws.onerror = () => ws.close();
    ws.onmessage = (e) => {
        const msg = JSON.parse(e.data);
        if (msg.type === 'entry') onEntry(msg.data);
        else if (msg.type === 'batch') msg.data.forEach(d => onEntry(d));
        else if (msg.type === 'config') { config = msg.data; updateConfigUI(); }
        else if (msg.type === 'stats') updateStats(msg.data);
        else if (msg.type === 'clear') { entries=[]; totalReceived=0; knownIds.clear(); idFilters={}; applyView(); }
    };
}

function onEntry(e) {
    entries.push(e);
    totalReceived++;
    if (entries.length > MAX_CLIENT_ENTRIES) entries.splice(0, entries.length - MAX_CLIENT_ENTRIES);

    const key = (e.kind===0?'r':'p') + e.id;
    const prev = knownIds.get(key);
    if (prev) { prev.count++; }
    else { knownIds.set(key, {id:e.id, name:e.name, kind:e.kind, count:1}); rebuildFilterTags(); }

    if (!paused) appendRow(e);
}

function appendRow(e) {
    if (!passesFilter(e)) return;
    const tbody = document.getElementById('tbody');
    const empty = document.getElementById('emptyMsg');
    empty.style.display = 'none';

    const tr = document.createElement('tr');
    tr.dataset.seq = e.seq;
    tr.onclick = () => selectEntry(e);
    const dirCls = e.direction===0?'dir-in':'dir-out';
    const dirTxt = e.direction===0?'IN':'OUT';
    const kindCls = e.kind===0?'kind-rpc':'kind-pkt';
    const kindTxt = e.kind===0?'RPC':'PKT';
    const age = entries.length>0 ? entries[entries.length-1].timestampMs - e.timestampMs : 0;
    tr.innerHTML = `<td class="time-col">${e.seq}</td><td class="${dirCls}">${dirTxt}</td><td class="${kindCls}">${kindTxt}</td><td class="id-col">${e.id}</td><td class="name-col" title="${esc(e.name)}">${esc(e.name)||'?'}</td><td class="parsed-col" title="${esc(e.parsed)}">${esc(e.parsed)||''}</td><td class="detail-col" title="${esc(e.detail)}">${esc(e.detail)||''}</td><td class="size-col">${e.dataBytes}B</td><td class="time-col">${fmtTime(age)}</td>`;
    tbody.appendChild(tr);

    // Trim DOM
    while (tbody.children.length > 1000) tbody.removeChild(tbody.firstChild);

    if (autoScroll) {
        const main = document.getElementById('mainPanel');
        main.scrollTop = main.scrollHeight;
    }
    updateInfo();
}

function passesFilter(e) {
    const search = document.getElementById('searchBox').value.toLowerCase();
    if (search && !(e.name||'').toLowerCase().includes(search) && !String(e.id).includes(search)) return false;

    const key = (e.kind===0?'r':'p') + e.id;
    const hasIncluded = Object.values(idFilters).includes('included');
    if (hasIncluded) return idFilters[key] === 'included';
    return idFilters[key] !== 'excluded';
}

function applyView() {
    const tbody = document.getElementById('tbody');
    tbody.innerHTML = '';
    const visible = entries.filter(e => passesFilter(e));
    const show = visible.slice(-1000);
    show.forEach(e => appendRow(e));
    document.getElementById('emptyMsg').style.display = show.length ? 'none' : '';
    updateInfo();
}

function selectEntry(e) {
    selectedSeq = e.seq;
    document.querySelectorAll('tr.selected').forEach(r => r.classList.remove('selected'));
    const row = document.querySelector(`tr[data-seq="${e.seq}"]`);
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
function rebuildFilterTags() {
    const container = document.getElementById('filterTags');
    container.innerHTML = '';
    const sorted = [...knownIds.entries()].sort((a,b) => b[1].count - a[1].count);
    for (const [key, info] of sorted) {
        const tag = document.createElement('span');
        tag.className = 'ftag' + (idFilters[key]==='excluded'?' excluded':'') + (idFilters[key]==='included'?' included':'');
        tag.textContent = `${info.kind===0?'R':'P'}:${info.id}`;
        tag.title = `${info.name||'?'} (${info.count}x) — click=exclude, dblclick=solo`;
        tag.onclick = (ev) => { ev.stopPropagation(); toggleIdFilter(key); };
        tag.ondblclick = (ev) => { ev.stopPropagation(); soloIdFilter(key); };
        container.appendChild(tag);
    }
}

function toggleIdFilter(key) {
    if (idFilters[key] === 'excluded') delete idFilters[key];
    else if (idFilters[key] === 'included') delete idFilters[key];
    else idFilters[key] = 'excluded';
    rebuildFilterTags();
    applyView();
}

function soloIdFilter(key) {
    idFilters = {};
    idFilters[key] = 'included';
    rebuildFilterTags();
    applyView();
}

function resetIdFilters() {
    idFilters = {};
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
async function clearAll() { await fetch('/api/clear',{method:'POST'}); entries=[]; totalReceived=0; knownIds.clear(); idFilters={}; applyView(); rebuildFilterTags(); }

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
    const visible = document.getElementById('tbody').children.length;
    document.getElementById('entryInfo').textContent = `${visible} shown (${entries.length} cached, ${totalReceived} total)`;
}

function fmtTime(ms) {
    if (ms<=0) return 'now';
    if (ms<1000) return ms+'ms';
    if (ms<60000) return (ms/1000).toFixed(1)+'s';
    return (ms/60000).toFixed(0)+'m';
}
function esc(s){return s?s.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;'):'';}

// Stats polling (every 3s since entries come via WS)
setInterval(async()=>{
    try{const r=await fetch('/api/stats');const s=await r.json();updateStats(s);}catch{}
}, 3000);

connectWs();
</script>
</body>
</html>
""";
}
