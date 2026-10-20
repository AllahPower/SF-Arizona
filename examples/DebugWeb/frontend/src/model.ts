export interface Settings {
  capture: boolean;
  incoming: boolean;
  outgoing: boolean;
  rpc: boolean;
  packets: boolean;
  port: number;
  maxEntries: number;
  rawBudgetMiB: number;
  packetIds: number[];
  rpcIds: number[];
  subIds: number[];
}
export interface Entry {
  seq: number;
  direction: "in" | "out";
  kind: "packet" | "rpc";
  id: number;
  effectiveId: number;
  subId: number | null;
  name: string;
  group: string | null;
  category?: string | null;
  source?: string | null;
  dataBytes: number;
  timestampMs: number;
}
export interface Node {
  name: string;
  type: string;
  kind: string;
  value: string | null;
  children: Node[];
}
export interface Details {
  reconstructed?: boolean;
  session: string;
  summary: Entry;
  parsed: Node | null;
  parser: string;
  error: string | null;
  dataBitLength: number;
  payloadBitOffset: number;
  payloadBitLength: number;
  raw: string;
}
export interface Stats {
  session: string;
  captured: number;
  retained: number;
  rawBytes: number;
  evicted: number;
  droppedBroadcastEntries: number;
  listeningPort: number;
  oldestSeq: number;
}
export interface Snapshot {
  session: string;
  entries: Entry[];
}

export async function api<T>(
  path: string,
  body?: unknown,
  signal?: AbortSignal,
): Promise<T> {
  const response = await fetch(path, {
    signal,
    ...(body === undefined
      ? {}
      : {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(body),
        }),
  });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.error || `HTTP ${response.status}`);
  }
  return response.status === 204 ||
    response.headers.get("content-length") === "0"
    ? (undefined as T)
    : response.json();
}

export function decodeRaw(base64: string): Uint8Array {
  return Uint8Array.from(atob(base64), (c) => c.charCodeAt(0));
}
export function hexOffset(offset: number): string {
  return offset.toString(16).toUpperCase().padStart(8, "0");
}
export function hexByte(value: number): string {
  return value.toString(16).toUpperCase().padStart(2, "0");
}
export function rawTextCells(bytes: Uint8Array, encoding: string): string[] {
  if (encoding === "windows-1251")
    return [...new TextDecoder(encoding).decode(bytes)].map((char) =>
      /[\x00-\x1f\x7f]/.test(char) ? "." : char,
    );
  const cells = Array<string>(bytes.length).fill(".");
  const decoder = new TextDecoder("utf-8", { fatal: true });
  for (let index = 0; index < bytes.length; index++) {
    const lead = bytes[index];
    if (lead >= 32 && lead < 127) {
      cells[index] = String.fromCharCode(lead);
      continue;
    }
    if (encoding !== "utf-8" || lead < 0xc2) continue;
    const width = lead <= 0xdf ? 2 : lead <= 0xef ? 3 : lead <= 0xf4 ? 4 : 0;
    if (!width || index + width > bytes.length) continue;
    try {
      cells[index] = decoder.decode(bytes.subarray(index, index + width));
      index += width - 1;
    } catch {
      /* Invalid sequences remain dots beside the unchanged hex bytes. */
    }
  }
  return cells;
}

export function rawRowText(
  bytes: Uint8Array,
  offset: number,
  encoding: string,
): string {
  const start = Math.max(0, offset - 3);
  return rawTextCells(bytes.subarray(start, offset + 19), encoding)
    .slice(offset - start, offset - start + 16)
    .join("");
}

export function payloadBitsInByte(
  index: number,
  offset: number,
  length: number,
): number {
  return Math.max(
    0,
    Math.min((index + 1) * 8, offset + length) - Math.max(index * 8, offset),
  );
}
export function toPlain(node: Node): unknown {
  if (node.kind === "null") return null;
  if (node.kind === "boolean") return node.value === "true";
  if (node.kind === "number") {
    const value = Number(node.value);
    return Number.isFinite(value) ? value : node.value;
  }
  if (node.kind === "array") return node.children.map(toPlain);
  if (node.kind === "object")
    return {
      $type: node.type,
      ...Object.fromEntries(
        node.children.map((child) => [child.name, toPlain(child)]),
      ),
    };
  return node.value;
}
export function matches(entry: Entry, search: string): boolean {
  return `${entry.id} ${entry.effectiveId} ${entry.subId ?? ""} ${entry.name} ${entry.direction} ${entry.kind}`
    .toLowerCase()
    .includes(search.toLowerCase());
}
export interface TrafficRule {
  exclude: boolean;
  kind: Entry["kind"];
  id: number;
  subId: number | null;
}

export function parseTrafficFilter(expression: string): TrafficRule[] {
  if (!expression.trim()) return [];
  return expression.split(",").map((token) => {
    const match = /^\s*(!)?(?:(packet|rpc):)?(\d+)(?:\/(\d+))?\s*$/.exec(token);
    if (
      !match ||
      Number(match[3]) > 255 ||
      (match[4] !== undefined &&
        (!Number.isSafeInteger(Number(match[4])) || match[2] === "rpc"))
    )
      throw new Error(
        "Use packet IDs 0-255, 220/17 or rpc:93; prefix ! to exclude.",
      );
    return {
      exclude: Boolean(match[1]),
      kind: match[2] === "rpc" ? "rpc" : "packet",
      id: Number(match[3]),
      subId: match[4] === undefined ? null : Number(match[4]),
    };
  });
}

export function matchesTrafficFilter(
  entry: Entry,
  rules: TrafficRule[],
): boolean {
  const matching = (rule: TrafficRule) =>
    rule.kind === entry.kind &&
    rule.id === entry.effectiveId &&
    (rule.subId === null || rule.subId === entry.subId);
  return (
    !rules.some((rule) => rule.exclude && matching(rule)) &&
    (!rules.some((rule) => !rule.exclude) ||
      rules.some((rule) => !rule.exclude && matching(rule)))
  );
}

export function trafficRoute(entry: Entry): string {
  return `${entry.kind === "rpc" ? "rpc:" : ""}${entry.effectiveId}${entry.kind === "packet" && entry.subId !== null ? `/${entry.subId}` : ""}`;
}
export function mergeEntries(
  existing: Entry[],
  added: Entry[],
  limit: number,
  oldest = 0,
): Entry[] {
  // Live batches normally append in sequence order; do not rebuild and sort the retained history.
  const ordered = added.every(
    (entry, index) => index === 0 || entry.seq > added[index - 1].seq,
  );
  if (
    ordered &&
    (!existing.length ||
      !added.length ||
      added[0].seq > existing[existing.length - 1].seq)
  ) {
    const combined = existing.concat(added);
    let start = Math.max(0, combined.length - limit);
    while (start < combined.length && combined[start].seq < oldest) start++;
    return combined.slice(start);
  }
  const map = new Map(
    existing.filter((e) => e.seq >= oldest).map((e) => [e.seq, e]),
  );
  for (const entry of added) if (entry.seq >= oldest) map.set(entry.seq, entry);
  return [...map.values()].sort((a, b) => a.seq - b.seq).slice(-limit);
}
