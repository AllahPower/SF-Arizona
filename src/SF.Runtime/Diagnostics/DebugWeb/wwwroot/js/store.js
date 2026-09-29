const MAX_ENTRIES = 50000;

const store = {
    serverSettings: { capture: false, incoming: true, outgoing: true, rpc: true, packets: true },

    paused: false,
    autoScroll: true,
    selectedSeq: null,
    idFilters: {},
    hasIncludedFilter: false,
    knownIds: new Map(),

    ring: new Array(MAX_ENTRIES),
    ringHead: 0,
    ringCount: 0,
    totalReceived: 0,

    worldViewState: { section: "overview", search: "", streamZone: false },
    worldSelectionKey: null,
    latestWorldSnapshot: null,

    _listeners: {},

    on(event, fn) {
        (this._listeners[event] ??= []).push(fn);
    },

    off(event, fn) {
        const arr = this._listeners[event];
        if (arr) this._listeners[event] = arr.filter(f => f !== fn);
    },

    emit(event, data) {
        for (const fn of this._listeners[event] || []) fn(data);
    },

    ringPush(entry) {
        this.ring[this.ringHead] = entry;
        this.ringHead = (this.ringHead + 1) % MAX_ENTRIES;
        if (this.ringCount < MAX_ENTRIES) this.ringCount++;
    },

    ringGet(i) {
        return this.ring[(this.ringHead - this.ringCount + i + MAX_ENTRIES) % MAX_ENTRIES];
    },

    recalcHasIncluded() {
        this.hasIncludedFilter = false;
        for (const k in this.idFilters) {
            if (this.idFilters[k] === "included") {
                this.hasIncludedFilter = true;
                return;
            }
        }
    },

    resetPackets() {
        this.ringHead = 0;
        this.ringCount = 0;
        this.totalReceived = 0;
    },

    resetAll() {
        this.resetPackets();
        this.knownIds.clear();
        this.idFilters = {};
        this.hasIncludedFilter = false;
    }
};

export { store, MAX_ENTRIES };
