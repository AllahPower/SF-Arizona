import { store } from "./store.js";

export async function postConfig() {
    await fetch("/api/config", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(store.serverSettings)
    });
}

export async function clearServer() {
    await fetch("/api/clear", { method: "POST" });
}

export async function fetchStats() {
    const r = await fetch("/api/stats");
    return r.json();
}
