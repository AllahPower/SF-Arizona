import { store } from "../store.js";
import { esc } from "../utils.js";

const CEF_GROUPS = {
    injectCode: {
        label: "InjectCode",
        css: "cef-inject",
        dir: "in",
        names: ["Arizona220:InjectCode"],
    },
    simpleCreate: {
        label: "SimpleCreate",
        css: "cef-create",
        dir: "in",
        names: ["Arizona220:SimpleCreate"],
    },
    send: {
        label: "Send",
        css: "cef-send",
        dir: "out",
        names: ["Arizona220:Send", "Arizona220:SendMessage"],
    },
    browserClick: {
        label: "BrowserClick",
        css: "cef-click",
        dir: "in",
        names: ["Arizona220:BrowserClick"],
    },
};

const CEF_NAME_MAP = new Map(
    Object.entries(CEF_GROUPS).flatMap(([key, def]) =>
        def.names.map(name => [name, { ...def, key }])
    )
);

const CEF_GROUP_KEYS = Object.keys(CEF_GROUPS);

const COPY_SVG = '<svg viewBox="0 0 16 16" width="14" height="14" fill="currentColor"><path d="M0 6.75C0 5.784.784 5 1.75 5h1.5a.75.75 0 0 1 0 1.5h-1.5a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-1.5a.75.75 0 0 1 1.5 0v1.5A1.75 1.75 0 0 1 9.25 16h-7.5A1.75 1.75 0 0 1 0 14.25Z"/><path d="M5 1.75C5 .784 5.784 0 6.75 0h7.5C15.216 0 16 .784 16 1.75v7.5A1.75 1.75 0 0 1 14.25 11h-7.5A1.75 1.75 0 0 1 5 9.25Zm1.75-.25a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-7.5a.25.25 0 0 0-.25-.25Z"/></svg>';

const MAX_CEF_ENTRIES = 5000;

let entries = [];
let _root = null;
let paused = false;
let searchText = "";
let activeFilters = new Set(CEF_GROUP_KEYS);
let expandedSeqs = new Set();
let pendingFlush = false;
let _searchTimer = 0;

function render() {
    const filterBtns = Object.entries(CEF_GROUPS).map(([key, def]) =>
        `<button class="cef-filter-btn active" data-cef-type="${key}"><span class="cef-dot ${def.css}"></span>${def.label}</button>`
    ).join("");

    return `
<div class="cef-toolbar">
    <div class="cef-filters">
        <button class="cef-filter-btn cef-filter-all active" id="cefBtnAll">All</button>
        ${filterBtns}
    </div>
    <div class="sep"></div>
    <input type="text" id="cefSearch" placeholder="Filter by event name, code, url...">
    <div class="sep"></div>
    <button id="cefBtnPause">Pause</button>
    <button class="warn" id="cefBtnClear">Clear</button>
    <span class="cef-counter" id="cefCounter">0 packets</span>
</div>
<div class="cef-scroll">
    <div class="cef-empty" id="cefEmpty">Waiting for CEF packets... Enable capture in the Packets tab.</div>
    <div class="cef-list" id="cefList"></div>
</div>`;
}

function mount(root) {
    _root = root;

    root.querySelector("#cefBtnAll").addEventListener("click", () => {
        const allActive = activeFilters.size === CEF_GROUP_KEYS.length;
        if (allActive) activeFilters.clear();
        else activeFilters = new Set(CEF_GROUP_KEYS);
        syncFilterButtons();
        rebuildList();
    });

    for (const btn of root.querySelectorAll(".cef-filter-btn[data-cef-type]")) {
        btn.addEventListener("click", ev => {
            const type = btn.dataset.cefType;
            if (ev.shiftKey) {
                if (activeFilters.has(type)) activeFilters.delete(type);
                else activeFilters.add(type);
            } else {
                if (activeFilters.size === 1 && activeFilters.has(type))
                    activeFilters = new Set(CEF_GROUP_KEYS);
                else
                    activeFilters = new Set([type]);
            }
            syncFilterButtons();
            rebuildList();
        });
    }

    root.querySelector("#cefSearch").addEventListener("input", ev => {
        clearTimeout(_searchTimer);
        _searchTimer = setTimeout(() => {
            searchText = ev.target.value.toLowerCase();
            rebuildList();
        }, 150);
    });

    root.querySelector("#cefBtnPause").addEventListener("click", () => {
        paused = !paused;
        const btn = root.querySelector("#cefBtnPause");
        btn.classList.toggle("active", paused);
        btn.textContent = paused ? "Resume" : "Pause";
        if (!paused) rebuildList();
    });

    root.querySelector("#cefBtnClear").addEventListener("click", () => {
        entries = [];
        expandedSeqs.clear();
        rebuildList();
    });

    // Toggle expand — only on header click
    root.querySelector("#cefList").addEventListener("click", ev => {
        // Copy button
        const copyBtn = ev.target.closest(".cef-copy-btn");
        if (copyBtn) {
            ev.stopPropagation();
            const text = copyBtn.dataset.copy;
            if (text) {
                navigator.clipboard.writeText(text).then(() => {
                    copyBtn.classList.add("copied");
                    setTimeout(() => copyBtn.classList.remove("copied"), 1200);
                });
            }
            return;
        }

        // Only toggle on header click
        const header = ev.target.closest(".cef-card-header");
        if (!header) return;
        const card = header.closest(".cef-card");
        if (!card) return;

        const seq = Number(card.dataset.seq);
        if (expandedSeqs.has(seq)) expandedSeqs.delete(seq);
        else expandedSeqs.add(seq);

        const body = card.querySelector(".cef-card-body");
        const arrow = card.querySelector(".cef-card-expand");
        if (body) body.hidden = !expandedSeqs.has(seq);
        if (arrow) arrow.textContent = expandedSeqs.has(seq) ? "\u25BC" : "\u25B6";
        card.classList.toggle("expanded", expandedSeqs.has(seq));
    });

    store.on("traffic:entry", onEntry);
    store.on("traffic:clear", () => {
        entries = [];
        expandedSeqs.clear();
        rebuildList();
    });
}

function onShow() {}
function onHide() {}

function onEntry(e) {
    const def = getCefDefinition(e);
    if (!def || !matchesDirection(def, e)) return;

    entries.push(e);
    if (entries.length > MAX_CEF_ENTRIES) entries = entries.slice(-MAX_CEF_ENTRIES);

    if (!paused && !pendingFlush) {
        pendingFlush = true;
        requestAnimationFrame(() => {
            pendingFlush = false;
            appendNewEntries();
        });
    }
}

function passesFilter(e) {
    const def = getCefDefinition(e);
    if (!def || !activeFilters.has(def.key) || !matchesDirection(def, e)) return false;
    if (searchText) {
        const haystack = `${e.name} ${e.parsed || ""} ${e.detail || ""}`.toLowerCase();
        if (!haystack.includes(searchText)) return false;
    }
    return true;
}

function appendNewEntries() {
    if (!_root) return;
    const list = _root.querySelector("#cefList");
    const empty = _root.querySelector("#cefEmpty");
    const scroll = _root.querySelector(".cef-scroll");

    const visible = entries.filter(passesFilter);
    if (visible.length > 0 && empty) { empty.hidden = true; list.hidden = false; }

    const lastRenderedSeq = list.lastElementChild?.dataset?.seq;
    const startIdx = lastRenderedSeq
        ? visible.findIndex(e => e.seq > Number(lastRenderedSeq))
        : 0;
    if (startIdx < 0) return;

    const fragment = document.createDocumentFragment();
    for (let i = Math.max(0, startIdx); i < visible.length; i++) {
        fragment.appendChild(buildCard(visible[i]));
    }
    list.appendChild(fragment);

    while (list.children.length > 2000) list.removeChild(list.firstChild);

    updateCounter();
    if (scroll) scroll.scrollTop = scroll.scrollHeight;
}

function rebuildList() {
    if (!_root) return;
    const list = _root.querySelector("#cefList");
    const empty = _root.querySelector("#cefEmpty");
    const scroll = _root.querySelector(".cef-scroll");

    const visible = entries.filter(passesFilter);

    if (visible.length === 0) {
        list.innerHTML = "";
        list.hidden = true;
        if (empty) empty.hidden = false;
        updateCounter();
        return;
    }

    if (empty) empty.hidden = true;
    list.hidden = false;

    const tail = visible.slice(-2000);
    const fragment = document.createDocumentFragment();
    for (const e of tail) fragment.appendChild(buildCard(e));
    list.innerHTML = "";
    list.appendChild(fragment);
    updateCounter();
    if (scroll) scroll.scrollTop = scroll.scrollHeight;
}

function buildCard(e) {
    const def = getCefDefinition(e);
    const expanded = expandedSeqs.has(e.seq);
    const parsed = parseCefEntry(e);

    const card = document.createElement("div");
    card.className = `cef-card ${def.css}${expanded ? " expanded" : ""}`;
    card.dataset.seq = e.seq;

    card.innerHTML =
`<div class="cef-card-header">
    <span class="cef-card-seq">#${e.seq}</span>
    <span class="cef-badge ${def.css}">${def.label}</span>
    <span class="cef-card-summary">${esc(parsed.summary)}</span>
    <span class="cef-card-size">${e.dataBytes}B</span>
    <span class="cef-card-expand">${expanded ? "\u25BC" : "\u25B6"}</span>
</div>
<div class="cef-card-body"${expanded ? "" : " hidden"}>
    ${parsed.bodyHtml}
</div>`;

    return card;
}

// --- Parsers ---

function parseCefEntry(e) {
    const raw = e.parsed || e.detail || "";
    const def = getCefDefinition(e);
    if (!def) return { summary: raw.slice(0, 120), bodyHtml: copyBlock("Raw", raw) };

    if (def.key === "injectCode") return parseInjectCode(raw, e);
    if (def.key === "simpleCreate") return parseSimpleCreate(raw, e);
    if (def.key === "send") return parseSendMessage(raw, e);
    if (def.key === "browserClick") return parseBrowserClick(raw, e);
    return { summary: raw.slice(0, 120), bodyHtml: copyBlock("Raw", raw) };
}

function parseInjectCode(raw, e) {
    const codeMatch = raw.match(/Code\s*=\s*(.+?)(?:,\s*RequestId|$)/s);
    const browserMatch = raw.match(/BrowserId\s*=\s*(\d+)/);
    const reqMatch = raw.match(/RequestId\s*=\s*(\d+)/);

    const code = codeMatch ? codeMatch[1].trim() : raw;
    const browserId = browserMatch ? browserMatch[1] : "?";
    const requestId = reqMatch ? reqMatch[1] : "?";

    const execMatch = code.match(/^window\.executeEvent\('([^']+)',\s*`(\[[\s\S]*\])`\);?$/);

    if (execMatch) {
        const eventName = execMatch[1];
        const argsRaw = execMatch[2];
        const summary = `Browser:${browserId}  ${eventName}`;

        let argsFormatted, argsCopyText;
        try {
            const parsed = JSON.parse(argsRaw);
            argsCopyText = JSON.stringify(parsed, null, 2);
            argsFormatted = syntaxHighlight(parsed);
        } catch {
            argsCopyText = argsRaw;
            argsFormatted = `<pre class="cef-code">${esc(argsRaw)}</pre>`;
        }

        return { summary, bodyHtml:
            detailGrid([
                ["Event", `<span class="cef-event-name">${esc(eventName)}</span>`],
                ["Browser", browserId],
                ["Request ID", requestId],
                ["Size", `${e.dataBytes} bytes`],
            ]) +
            copyBlock("Arguments", argsCopyText, argsFormatted) +
            copyBlock("Raw Code", code)
        };
    }

    const summary = `Browser:${browserId}  ${code.slice(0, 100)}`;
    return { summary, bodyHtml:
        detailGrid([
            ["Browser", browserId],
            ["Request ID", requestId],
            ["Size", `${e.dataBytes} bytes`],
        ]) +
        copyBlock("Code", code)
    };
}

function parseSimpleCreate(raw, e) {
    const w = extract(raw, "Width"), h = extract(raw, "Height");
    const x = extract(raw, "X"), y = extract(raw, "Y");
    const url = extract(raw, "PrimaryText");
    const secondary = extract(raw, "SecondaryText");

    return {
        summary: `${w}x${h} at (${x},${y})  ${url}`,
        bodyHtml:
            detailGrid([
                ["Size", `${w} x ${h}`],
                ["Position", `(${x}, ${y})`],
                ["URL", `<span class="cef-url">${esc(url)}</span>`],
                ["Token", secondary],
                ["ExtraInt", extract(raw, "ExtraInt")],
                ["ExtraFloat", extract(raw, "ExtraFloat")],
            ]) +
            copyBlock("Raw", raw)
    };
}

function parseSendMessage(raw, e) {
    const textMatch = raw.match(/Text\s*=\s*(.+?)(?:,\s*BrowserId|$)/s);
    const text = textMatch ? textMatch[1].trim() : extract(raw, "Text");
    const browserId = extract(raw, "BrowserId");
    const parts = text.split("|");
    const action = parts[0] || text;
    const payload = parts.slice(1).join("|");

    let payloadHtml;
    if (payload) {
        try {
            const parsed = JSON.parse(payload);
            payloadHtml = copyBlock("Payload", JSON.stringify(parsed, null, 2), syntaxHighlight(parsed));
        } catch {
            payloadHtml = copyBlock("Payload", payload);
        }
    } else {
        payloadHtml = "";
    }

    return {
        summary: `Browser:${browserId}  ${action}${payload ? "  |  " + payload.slice(0, 80) : ""}`,
        bodyHtml:
            detailGrid([
                ["Action", `<span class="cef-event-name">${esc(action)}</span>`],
                ["Browser", browserId],
                ["Size", `${e.dataBytes} bytes`],
            ]) +
            payloadHtml +
            copyBlock("Raw", text)
    };
}

function parseBrowserClick(raw) {
    const browser = extract(raw, "BrowserId");
    const x = extract(raw, "X"), y = extract(raw, "Y");
    const btn = extract(raw, "MouseButton");
    const btnName = btn === "0" ? "Left" : btn === "1" ? "Right" : btn === "2" ? "Middle" : `Button ${btn}`;

    return {
        summary: `Browser:${browser}  (${x}, ${y})  ${btnName}`,
        bodyHtml: detailGrid([
            ["Browser", browser],
            ["Position", `(${x}, ${y})`],
            ["Button", btnName],
        ])
    };
}

// --- Helpers ---

function extract(raw, field) {
    const m = raw.match(new RegExp(field + "\\s*=\\s*([^,}]+)"));
    return m ? m[1].trim() : "";
}

function detailGrid(rows) {
    const cells = rows.map(([label, value]) => {
        const isHtml = typeof value === "string" && value.includes("<");
        const val = isHtml ? value : esc(String(value));
        return `<div class="cef-detail-label">${esc(label)}</div><div class="cef-detail-value">${val}</div>`;
    }).join("");
    return `<div class="cef-detail-grid">${cells}</div>`;
}

function copyBlock(label, copyText, customHtml) {
    const id = Math.random().toString(36).slice(2, 8);
    const display = customHtml || `<pre class="cef-code">${esc(copyText)}</pre>`;
    return `
<div class="cef-section">
    <div class="cef-section-header">
        <span class="cef-section-label">${esc(label)}</span>
        <button class="cef-copy-btn" data-copy="${esc(copyText)}" title="Copy">${COPY_SVG}</button>
    </div>
    <div class="cef-section-content">${display}</div>
</div>`;
}

function syntaxHighlight(obj) {
    const json = JSON.stringify(obj, null, 2);
    const html = esc(json)
        .replace(/"([^"]+)":/g, '<span class="cef-json-key">"$1"</span>:')
        .replace(/: "([^"]*)"/g, ': <span class="cef-json-string">"$1"</span>')
        .replace(/: (\d+\.?\d*)/g, ': <span class="cef-json-number">$1</span>')
        .replace(/: (true|false)/g, ': <span class="cef-json-bool">$1</span>')
        .replace(/: (null)/g, ': <span class="cef-json-null">$1</span>');
    return `<pre class="cef-json">${html}</pre>`;
}

function syncFilterButtons() {
    if (!_root) return;
    const allActive = activeFilters.size === CEF_GROUP_KEYS.length;
    _root.querySelector("#cefBtnAll").classList.toggle("active", allActive);
    for (const btn of _root.querySelectorAll(".cef-filter-btn[data-cef-type]")) {
        btn.classList.toggle("active", activeFilters.has(btn.dataset.cefType));
    }
}

function updateCounter() {
    if (!_root) return;
    const el = _root.querySelector("#cefCounter");
    const visible = entries.filter(passesFilter).length;
    if (el) el.textContent = `${visible} / ${entries.length} packets`;
}

function getCefDefinition(entry) {
    return CEF_NAME_MAP.get(entry.name);
}

function matchesDirection(def, entry) {
    return def.dir === "out" ? entry.direction === 1 : entry.direction === 0;
}

export const CefPage = { render, mount, onShow, onHide };
