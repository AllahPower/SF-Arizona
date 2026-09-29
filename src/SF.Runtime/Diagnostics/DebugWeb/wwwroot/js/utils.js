export function esc(s) {
    return s ? s.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;") : "";
}

export function fmtVec(v) {
    if (!v) return "-";
    return `(${fmtCompact(v.x)}, ${fmtCompact(v.y)}, ${fmtCompact(v.z)})`;
}

export function fmtCompact(v) {
    return Number(v ?? 0).toFixed(2).replace(/\.00$/, "");
}

export function fmtNumber(v) {
    return v === null || v === undefined ? "-" : fmtCompact(v);
}

export function fmtOpt(v) {
    return v === null || v === undefined ? "-" : `${v}`;
}

export function fmtColor(value) {
    const num = Number(value >>> 0);
    return `0x${num.toString(16).toUpperCase().padStart(8, "0")}`;
}

export function toRgba(value, alpha) {
    const num = Number(value >>> 0);
    const a = (num >>> 24) & 0xFF;
    const b = (num >>> 16) & 0xFF;
    const g = (num >>> 8) & 0xFF;
    const r = num & 0xFF;
    const effectiveAlpha = alpha ?? (a / 255);
    return `rgba(${r}, ${g}, ${b}, ${effectiveAlpha})`;
}

export function toRgbCss(value) {
    const num = Number(value >>> 0);
    const b = (num >>> 16) & 0xFF;
    const g = (num >>> 8) & 0xFF;
    const r = num & 0xFF;
    return `rgb(${r}, ${g}, ${b})`;
}

export function fmtAttachment(vehicleId, objectId) {
    if (vehicleId !== null && vehicleId !== undefined) return `vehicle:${vehicleId}`;
    if (objectId !== null && objectId !== undefined) return `object:${objectId}`;
    return "-";
}

export function fmtLabelAttachment(label) {
    if (label.attachedToPlayer !== null && label.attachedToPlayer !== undefined) {
        return `player:${label.attachedToPlayer}`;
    }
    if (label.attachedToVehicle !== null && label.attachedToVehicle !== undefined) {
        return `vehicle:${label.attachedToVehicle}`;
    }
    return "-";
}

export function trimText(value, maxLength) {
    if (!value) return value;
    return value.length > maxLength ? `${value.slice(0, maxLength - 3)}...` : value;
}

export function $(sel, root) {
    return (root || document).querySelector(sel);
}

export function $$(sel, root) {
    return (root || document).querySelectorAll(sel);
}
