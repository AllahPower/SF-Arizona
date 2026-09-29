const pages = {};
let activePage = null;
let activeKey = null;

function register(key, page) {
    pages[key] = page;
}

function start(defaultKey) {
    window.addEventListener("hashchange", () => resolve());
    resolve(defaultKey);
}

function resolve(fallback) {
    const key = location.hash.slice(1) || fallback || "packets";
    if (key === activeKey) return;
    navigate(key);
}

function navigate(key) {
    const page = pages[key];
    if (!page) return;

    if (activePage && activePage.onHide) activePage.onHide();

    const container = document.getElementById("page-content");
    let el = document.getElementById(`page-${key}`);
    if (!el) {
        el = document.createElement("div");
        el.id = `page-${key}`;
        el.className = "page";
        el.innerHTML = page.render();
        container.appendChild(el);
        if (page.mount) page.mount(el);
    }

    for (const p of container.children) {
        p.classList.toggle("active", p === el);
    }

    for (const btn of document.querySelectorAll(".tab-btn")) {
        btn.classList.toggle("active", btn.dataset.tab === key);
    }

    activeKey = key;
    activePage = page;
    if (page.onShow) page.onShow(el);

    if (location.hash.slice(1) !== key) {
        history.replaceState(null, "", `#${key}`);
    }
}

export const router = { register, start, navigate, get activeKey() { return activeKey; } };
