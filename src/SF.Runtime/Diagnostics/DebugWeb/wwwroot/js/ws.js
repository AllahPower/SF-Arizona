import { store } from "./store.js";

let socket = null;

function connect() {
    const url = `ws://${location.host}/ws`;
    socket = new WebSocket(url);

    socket.onopen = () => {
        store.emit("ws:status", true);
        pushPacketViewState();
        pushWorldViewState();
    };

    socket.onclose = () => {
        store.emit("ws:status", false);
        setTimeout(connect, 2000);
    };

    socket.onerror = () => socket.close();

    socket.onmessage = ev => {
        const msg = JSON.parse(ev.data);
        switch (msg.type) {
            case "entry":
                store.emit("traffic:entry", msg.data);
                break;
            case "batch":
                for (const d of msg.data) store.emit("traffic:entry", d);
                break;
            case "server-settings":
                store.serverSettings = msg.data;
                store.emit("settings:update", msg.data);
                break;
            case "stats":
                store.emit("stats:update", msg.data);
                break;
            case "world":
                store.latestWorldSnapshot = msg.data;
                store.emit("world:snapshot", msg.data);
                break;
            case "clear":
                store.resetAll();
                store.emit("traffic:clear");
                break;
        }
    };
}

function send(type, data) {
    if (!socket || socket.readyState !== WebSocket.OPEN) return;
    socket.send(JSON.stringify({ type, data }));
}

function pushPacketViewState() {
    send("packets-view:update", {
        search: "",
        paused: store.paused,
        autoScroll: store.autoScroll,
        idFilters: Object.entries(store.idFilters).map(([key, mode]) => ({ key, mode }))
    });
}

function pushWorldViewState() {
    send("world-view:update", {
        section: store.worldViewState.section,
        search: store.worldViewState.search,
        streamZone: store.worldViewState.streamZone
    });
}

export const ws = { connect, send, pushPacketViewState, pushWorldViewState };
