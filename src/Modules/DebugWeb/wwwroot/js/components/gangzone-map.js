import { store } from "../store.js";
import { fmtCompact, fmtColor, toRgbCss } from "../utils.js";

let map = null;
let layers = new Map();
let selectionKey = null;
let viewInitialized = false;
const worldBounds = [[-3000, -3000], [3000, 3000]];

export function ensureMap(hostEl) {
    if (map) return map;
    if (!hostEl || typeof L === "undefined") return null;

    map = L.map(hostEl, {
        crs: L.CRS.Simple,
        attributionControl: false,
        zoomControl: true,
        minZoom: -2,
        maxZoom: 4,
        zoomSnap: 0.25,
        zoomDelta: 0.5,
        maxBounds: [[-3200, -3200], [3200, 3200]],
        maxBoundsViscosity: 1.0,
        renderer: L.canvas({ padding: 2.0 })
    });

    const bounds = L.latLngBounds(worldBounds);
    L.rectangle(bounds, {
        color: "rgba(255,255,255,0.08)",
        weight: 1,
        fillOpacity: 0,
        interactive: false
    }).addTo(map);

    map.fitBounds(bounds, { padding: [16, 16], animate: false });
    viewInitialized = true;

    return map;
}

export function renderZones(zones, tooltipEl, legendEl) {
    if (!map) return;
    const rows = zones || [];

    if (!rows.length) {
        clearLayers();
        if (legendEl) legendEl.textContent = "No gang zones loaded.";
        if (tooltipEl) tooltipEl.textContent = "Hover a zone to inspect it.";
        selectionKey = null;
        return;
    }

    if (legendEl) legendEl.textContent = `${rows.length} zones loaded. Primary color is the current owner marker.`;
    if (tooltipEl && (!store.worldSelectionKey || !store.worldSelectionKey.startsWith("gangzone:"))) {
        tooltipEl.textContent = buildTooltip(rows[0]);
    }

    const currentKeys = new Set();

    for (const zone of rows) {
        const key = `gangzone:${zone.id}`;
        currentKeys.add(key);
        const bounds = normalizeBounds(zone);
        const existing = layers.get(key);

        if (existing) {
            existing.zone = zone;
            const newColor = toRgbCss(zone.color);
            const isActive = store.worldSelectionKey === key;
            existing.layer.setBounds(bounds);
            existing.layer.setStyle({
                color: newColor,
                fillColor: newColor,
                weight: isActive ? 2.5 : 1,
                opacity: isActive ? 0.92 : 0.55,
                fillOpacity: zone.isFlashing
                    ? (isActive ? 0.4 : 0.28)
                    : (isActive ? 0.3 : 0.18)
            });
            if (existing.layer._path) {
                existing.layer._path.className.baseVal = zone.isFlashing
                    ? "gangzone-layer gangzone-layer-flashing" : "gangzone-layer";
            }
            continue;
        }

        const layer = L.rectangle(bounds, {
            color: toRgbCss(zone.color),
            weight: store.worldSelectionKey === key ? 2.5 : 1,
            opacity: store.worldSelectionKey === key ? 0.92 : 0.55,
            fillColor: toRgbCss(zone.color),
            fillOpacity: zone.isFlashing ? 0.28 : 0.18,
            interactive: true,
            className: zone.isFlashing ? "gangzone-layer gangzone-layer-flashing" : "gangzone-layer"
        });

        layer.on("mouseover", () => {
            const item = layers.get(key);
            if (!item) return;
            if (tooltipEl) tooltipEl.textContent = buildTooltip(item.zone);
            if (store.worldSelectionKey !== key) {
                layer.setStyle({ weight: 2, opacity: 0.85, fillOpacity: item.zone.isFlashing ? 0.34 : 0.24 });
            }
        });
        layer.on("mouseout", () => {
            const item = layers.get(key);
            if (!item) return;
            if (store.worldSelectionKey !== key) {
                layer.setStyle({ weight: 1, opacity: 0.55, fillOpacity: item.zone.isFlashing ? 0.28 : 0.18 });
            }
        });
        layer.on("click", () => {
            const item = layers.get(key);
            if (!item) return;
            store.worldSelectionKey = key;
            store.emit("world:select", { key, entry: item.zone });
            if (tooltipEl) tooltipEl.textContent = buildTooltip(item.zone);
            if (map) {
                map.fitBounds(layer.getBounds(), { padding: [24, 24], maxZoom: 3, animate: false });
            }
            syncSelection(tooltipEl);
        });

        layer.addTo(map);
        layers.set(key, { layer, zone });
    }

    for (const [key, item] of layers.entries()) {
        if (!currentKeys.has(key)) {
            item.layer.remove();
            layers.delete(key);
        }
    }

    if (!viewInitialized) {
        fitToRows(rows);
        viewInitialized = true;
    }

    syncSelection(tooltipEl);
}

export function syncSelection(tooltipEl) {
    if (!map) return;

    selectionKey = store.worldSelectionKey;
    for (const [key, item] of layers.entries()) {
        const active = key === store.worldSelectionKey;
        item.layer.setStyle({
            weight: active ? 2.5 : 1,
            opacity: active ? 0.92 : 0.55,
            fillOpacity: active
                ? (item.zone.isFlashing ? 0.4 : 0.3)
                : (item.zone.isFlashing ? 0.28 : 0.18)
        });
    }

    if (store.worldSelectionKey && layers.has(store.worldSelectionKey)) {
        const item = layers.get(store.worldSelectionKey);
        if (tooltipEl) tooltipEl.textContent = buildTooltip(item.zone);
    }
}

export function invalidateSize() {
    if (map) map.invalidateSize(false);
}

function clearLayers() {
    for (const item of layers.values()) item.layer.remove();
    layers.clear();
}

function fitToRows(rows) {
    if (!map || !rows.length) return;
    let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
    for (const z of rows) {
        minX = Math.min(minX, z.minX, z.maxX);
        minY = Math.min(minY, z.minY, z.maxY);
        maxX = Math.max(maxX, z.minX, z.maxX);
        maxY = Math.max(maxY, z.minY, z.maxY);
    }
    if (!Number.isFinite(minX) || !Number.isFinite(minY) || !Number.isFinite(maxX) || !Number.isFinite(maxY)) return;
    map.fitBounds([[minY, minX], [maxY, maxX]], { padding: [24, 24], animate: false, maxZoom: 1 });
    viewInitialized = true;
}

function normalizeBounds(zone) {
    const minX = Math.min(zone.minX, zone.maxX);
    const maxX = Math.max(zone.minX, zone.maxX);
    const minY = Math.min(zone.minY, zone.maxY);
    const maxY = Math.max(zone.minY, zone.maxY);
    return [[minY, minX], [maxY, maxX]];
}

function buildTooltip(zone) {
    return [
        `Zone #${zone.id}`,
        `Owner: ${fmtColor(zone.color)}`,
        zone.isFlashing ? `Flash: ${fmtColor(zone.altColor)}` : "Flash: off",
        `Rect: (${fmtCompact(zone.minX)}, ${fmtCompact(zone.minY)}) -> (${fmtCompact(zone.maxX)}, ${fmtCompact(zone.maxY)})`
    ].join("\n");
}
