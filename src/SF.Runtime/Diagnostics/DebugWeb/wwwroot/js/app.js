import { store } from "./store.js";
import { router } from "./router.js";
import { ws } from "./ws.js";
import { PacketsPage } from "./pages/packets.js";
import { WorldPage } from "./pages/world.js";
import { CefPage } from "./pages/cef.js";

function renderShell() {
    document.getElementById("app").innerHTML = `
<div class="header">
    <h1>SF-Arizona Debug</h1>
    <span class="badge" id="capBadge">OFF</span>
    <span class="badge ws-err" id="wsBadge">WS: ...</span>
    <span class="header-info" id="entryInfo">0 entries (0 total)</span>
</div>
<div class="tab-bar">
    <button class="tab-btn active" data-tab="packets">Packets</button>
    <button class="tab-btn" data-tab="cef">CEF</button>
    <button class="tab-btn" data-tab="world">World</button>
</div>
<div id="page-content"></div>`;

    for (const btn of document.querySelectorAll(".tab-btn")) {
        btn.addEventListener("click", () => router.navigate(btn.dataset.tab));
    }
}

store.on("ws:status", ok => {
    const badge = document.getElementById("wsBadge");
    if (!badge) return;
    badge.textContent = ok ? "WS: OK" : "WS: OFF";
    badge.className = ok ? "badge ws-ok" : "badge ws-err";
});

renderShell();

router.register("packets", PacketsPage);
router.register("cef", CefPage);
router.register("world", WorldPage);
router.start("packets");

ws.connect();
