import { describe, expect, it } from "vitest";
import {
  decodeRaw,
  hexOffset,
  matches,
  matchesTrafficFilter,
  parseTrafficFilter,
  trafficRoute,
  mergeEntries,
  payloadBitsInByte,
  rawTextCells,
  rawRowText,
  toPlain,
} from "./model";
import type { Entry, Node } from "./model";

describe("packet inspector data", () => {
  it("decodes a UTF-8 character across hex rows without replacement characters", () => {
    const text = "x".repeat(15) + String.fromCodePoint(0x41f);
    const cells = rawTextCells(new TextEncoder().encode(text), "utf-8");
    expect(cells[15]).toBe(String.fromCodePoint(0x41f));
    expect(cells[16]).toBe(".");
    expect(cells).toHaveLength(17);
    expect(rawRowText(new TextEncoder().encode(text), 16, "utf-8")).toBe(".");
    expect(
      rawTextCells(new Uint8Array([0xcf, 0xf0]), "windows-1251").join(""),
    ).toBe(String.fromCodePoint(0x41f, 0x440));
  });
  const entry = (seq: number): Entry => ({
    seq,
    direction: "out",
    kind: "packet",
    id: 220,
    effectiveId: 220,
    subId: 210,
    name: "LinkedChatSend",
    group: null,
    dataBytes: 3,
    timestampMs: 0,
  });
  it("deduplicates replay, retains ordering and removes evicted entries", () => {
    expect(
      mergeEntries(
        [entry(1), entry(2)],
        [entry(2), entry(3), entry(4)],
        2,
        2,
      ).map((e) => e.seq),
    ).toEqual([3, 4]);
  });
  it("appends live batches with retention limits without changing the inputs", () => {
    const existing = [entry(1), entry(2), entry(3)];
    const added = [entry(4), entry(5)];
    expect(mergeEntries(existing, added, 3).map((item) => item.seq)).toEqual([
      3, 4, 5,
    ]);
    expect(
      mergeEntries(existing, added, 10, 4).map((item) => item.seq),
    ).toEqual([4, 5]);
    expect(mergeEntries(existing, [], 10, 9)).toEqual([]);
    expect(mergeEntries([], added, 1).map((item) => item.seq)).toEqual([5]);
    expect(existing.map((item) => item.seq)).toEqual([1, 2, 3]);
    expect(added.map((item) => item.seq)).toEqual([4, 5]);
  });
  it("orders late batches and replaces replayed entries", () => {
    const replacement = { ...entry(3), name: "updated" };
    const merged = mergeEntries(
      [entry(1), entry(3)],
      [entry(4), entry(2), replacement],
      10,
    );
    expect(merged.map((item) => item.seq)).toEqual([1, 2, 3, 4]);
    expect(merged[2].name).toBe("updated");
  });
  it("preserves all raw bytes and offsets", () => {
    expect([...decodeRaw("/6uA")]).toEqual([255, 171, 128]);
    expect(hexOffset(16)).toBe("00000010");
  });
  it("highlights partial payload bytes rather than rounding payload offsets", () => {
    expect(payloadBitsInByte(0, 11, 10)).toBe(0);
    expect(payloadBitsInByte(1, 11, 10)).toBe(5);
    expect(payloadBitsInByte(2, 11, 10)).toBe(5);
    expect(payloadBitsInByte(3, 11, 10)).toBe(0);
  });
  it("turns typed arrays into actual JSON arrays", () => {
    const node: Node = {
      name: "Segments",
      type: "Segment[]",
      kind: "array",
      value: "1",
      children: [
        {
          name: "[0]",
          type: "TextSegment",
          kind: "object",
          value: null,
          children: [
            {
              name: "Text",
              type: "String",
              kind: "string",
              value: "hello",
              children: [],
            },
          ],
        },
      ],
    };
    expect(toPlain(node)).toEqual([{ $type: "TextSegment", Text: "hello" }]);
  });
  it("searches numeric routes independently of displayed names", () => {
    expect(matches(entry(1), "210")).toBe(true);
    expect(matches(entry(1), "linkedchat")).toBe(true);
    expect(matches(entry(1), "missing")).toBe(false);
  });
  it("filters effective packet routes without confusing RPC IDs", () => {
    const packet = { ...entry(1), id: 40, effectiveId: 220, subId: 17 };
    const rpc = {
      ...entry(2),
      kind: "rpc" as const,
      effectiveId: 220,
      subId: null,
    };
    expect(
      matchesTrafficFilter(packet, parseTrafficFilter("220/17, rpc:93")),
    ).toBe(true);
    expect(matchesTrafficFilter(rpc, parseTrafficFilter("220"))).toBe(false);
    expect(
      matchesTrafficFilter(packet, parseTrafficFilter("220, !220/17")),
    ).toBe(false);
    expect(matchesTrafficFilter(entry(3), parseTrafficFilter("!220/17"))).toBe(
      true,
    );
    expect(matchesTrafficFilter(rpc, parseTrafficFilter("!220"))).toBe(true);
    expect(matchesTrafficFilter(rpc, [])).toBe(true);
    expect(trafficRoute(packet)).toBe("220/17");
    expect(trafficRoute(rpc)).toBe("rpc:220");
  });
  it("rejects malformed route expressions instead of silently broadening them", () => {
    for (const invalid of [
      "256",
      "-1",
      "220/",
      "rpc:93/1",
      "220,,207",
      "220/9007199254740992",
      "foo",
    ]) {
      expect(() => parseTrafficFilter(invalid)).toThrow();
    }
    expect(parseTrafficFilter(" !packet:220/17, rpc:93 ")).toHaveLength(2);
    expect(parseTrafficFilter("  ")).toEqual([]);
  });
});
