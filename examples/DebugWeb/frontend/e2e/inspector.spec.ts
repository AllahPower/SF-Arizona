import { expect, test } from "@playwright/test";

const settings = {
  capture: false,
  incoming: true,
  outgoing: true,
  rpc: true,
  packets: true,
  port: 7777,
  maxEntries: 10000,
  rawBudgetMiB: 32,
  packetIds: [],
  rpcIds: [],
  subIds: [],
};
const linked = {
  seq: 1,
  direction: "out",
  kind: "packet",
  id: 220,
  effectiveId: 220,
  subId: 210,
  name: "Custom220:LinkedChatSend",
  group: null,
  category: null,
  dataBytes: 12,
  timestampMs: 1000,
};
const scalar = (name: string, type: string, kind: string, value: string) => ({
  name,
  type,
  kind,
  value,
  children: [],
});
const parsed = {
  name: "Parsed",
  type: "OutgoingSubPacket",
  kind: "object",
  value: null,
  children: [
    {
      name: "Payload",
      type: "ExampleLinkedChatSend",
      kind: "object",
      value: null,
      children: [
        scalar("Reserved", "Byte", "number", "0"),
        {
          name: "Segments",
          type: "ExampleLinkedChatSegment[]",
          kind: "array",
          value: "2",
          children: [
            {
              name: "[0]",
              type: "ExampleLinkedChatTextSegment",
              kind: "object",
              value: null,
              children: [scalar("Text", "String", "string", "hi")],
            },
            {
              name: "[1]",
              type: "ExampleLinkedChatItemSegment",
              kind: "object",
              value: null,
              children: [scalar("ItemId", "UInt16", "number", "1110")],
            },
          ],
        },
      ],
    },
  ],
};

test.beforeEach(async ({ page }) => {
  await page.addInitScript(() =>
    localStorage.setItem("sf.debug.scroll", "false"),
  );
  let current = { ...settings };
  const entries = [
    linked,
    ...["InjectCode", "SimpleCreate", "Send", "BrowserClick"].map(
      (group, index) => ({
        ...linked,
        seq: index + 2,
        direction: group === "Send" ? "out" : "in",
        subId: [17, 10, 18, 23][index],
        group: group,
        category: "TOOLS",
        name: `Custom220:${group}`,
      }),
    ),
  ];
  await page.route("**/api/**", async (route) => {
    const path = new URL(route.request().url()).pathname;
    if (path === "/api/config") {
      if (route.request().method() === "POST")
        current = route.request().postDataJSON();
      await route.fulfill({ json: current });
    } else if (path === "/api/stats")
      await route.fulfill({
        json: {
          session: "test-session",
          captured: 5,
          retained: 5,
          rawBytes: 60,
          evicted: 0,
          droppedBroadcastEntries: 0,
          listeningPort: 7777,
          oldestSeq: 1,
        },
      });
    else if (path === "/api/entries/1")
      await route.fulfill({
        json: {
          session: "test-session",
          summary: linked,
          parsed,
          parser: "LinkedChatSend",
          error: null,
          dataBitLength: 96,
          payloadBitOffset: 16,
          payloadBitLength: 80,
          raw: Buffer.from([
            220, 210, 0, 2, 0, 2, 0, 104, 105, 2, 86, 4,
          ]).toString("base64"),
        },
      });
    else if (path === "/api/entries")
      await route.fulfill({ json: { session: "test-session", entries } });
    else
      await route.fulfill({
        status: 410,
        json: { error: "Entry was evicted or cleared." },
      });
  });
  await page.routeWebSocket("**/ws", (socket) =>
    socket.send(JSON.stringify({ session: "test-session", entries })),
  );
});

test("expands polymorphic chat segments and shows original hex offsets", async ({
  page,
}) => {
  await page.goto("/");
  await page.getByRole("button", { name: /Custom220:LinkedChatSend/ }).click();
  await page
    .getByRole("button", {
      name: "Toggle Parsed.Payload.Segments",
      exact: true,
    })
    .click();
  await page
    .getByRole("button", {
      name: "Toggle Parsed.Payload.Segments.[0]",
      exact: true,
    })
    .click();
  await expect(page.getByText("hi", { exact: true })).toBeVisible();
  await expect(
    page.getByText("ExampleLinkedChatItemSegment", { exact: true }),
  ).toBeVisible();
  await page.getByRole("button", { name: "JSON", exact: true }).click();
  await expect(page.locator(".json-pane pre")).toContainText('"Segments": [');
  await page.getByRole("button", { name: "RAW", exact: true }).click();
  await expect(page.getByText("00000000", { exact: true })).toBeVisible();
  await expect(page.getByText("DC", { exact: true })).toBeVisible();
  await expect(page.getByText("D2", { exact: true })).toBeVisible();
  await page.getByLabel("Raw text encoding").selectOption("windows-1251");
  await page.screenshot({
    path: "../../../artifacts/tests/DebugWeb/inspector-desktop.png",
  });
});

test("routes extension categories by metadata and saves settings", async ({
  page,
}) => {
  await page.goto("/");
  await page.getByRole("button", { name: "TOOLS", exact: true }).click();
  await expect(page.locator(".traffic-row")).toHaveCount(4);
  await page.getByRole("button", { name: "SimpleCreate", exact: true }).click();
  await expect(page.locator(".traffic-row")).toHaveCount(3);
  await page.getByRole("button", { name: "SETTINGS", exact: true }).click();
  await page.getByLabel("Maximum entries").fill("1500");
  await page
    .getByRole("button", { name: "Save configuration", exact: true })
    .click();
  await expect(page.getByRole("status")).toContainText("Saved.");
  await expect(page.getByLabel("Maximum entries")).toHaveValue("1500");
});

test("shows distinct branch controls and indentation in the structure", async ({
  page,
}) => {
  await page.goto("/");
  await page.getByRole("button", { name: /Custom220:LinkedChatSend/ }).click();
  const root = page.getByRole("button", { name: "Toggle Parsed", exact: true });
  const payload = page.getByRole("button", {
    name: "Toggle Parsed.Payload",
    exact: true,
  });
  const segments = page.getByRole("button", {
    name: "Toggle Parsed.Payload.Segments",
    exact: true,
  });
  await expect(root).toHaveAttribute("aria-expanded", "true");
  await expect(segments).toHaveAttribute("aria-expanded", "false");
  await expect(segments.locator("svg")).toBeVisible();
  const rootBox = await root.boundingBox();
  const payloadBox = await payload.boundingBox();
  const segmentsBox = await segments.boundingBox();
  expect(rootBox!.width).toBe(20);
  expect(payloadBox!.x - rootBox!.x).toBe(18);
  expect(segmentsBox!.x - payloadBox!.x).toBe(18);
  await segments.click();
  await expect(segments).toHaveAttribute("aria-expanded", "true");
  await expect(
    page.getByRole("button", {
      name: "Toggle Parsed.Payload.Segments.[0]",
      exact: true,
    }),
  ).toBeVisible();
  await page.screenshot({
    path: "../../../artifacts/tests/DebugWeb/structure-tree.png",
  });
  await payload.click();
  await expect(segments).toHaveCount(0);
  await expect(payload).toHaveAttribute("aria-expanded", "false");
});

test("keeps a large traffic buffer virtualized", async ({ page }) => {
  await page.routeWebSocket("**/ws", (socket) =>
    socket.send(
      JSON.stringify({
        session: "test-session",
        entries: Array.from({ length: 10000 }, (_, index) => ({
          ...linked,
          seq: index + 1,
        })),
      }),
    ),
  );
  await page.goto("/");
  await expect(page.locator(".traffic-row").first()).toBeVisible();
  expect(await page.locator(".traffic-row").count()).toBeLessThanOrEqual(100);
  const list = page.locator(".traffic-panel .virtual");
  await expect(list.locator(":scope > div")).toHaveCSS("height", "260000px");
  await list.evaluate((element) => {
    element.scrollTop = 5000 * 26;
  });
  await expect(list.locator(".seq").first()).toHaveText("4993");
  await expect(list.locator(":scope > div > div")).toHaveCSS("top", "129792px");
  await list.evaluate((element) => {
    element.scrollTop = element.scrollHeight;
  });
  await expect(list.locator(".seq").last()).toHaveText("10000");
  await list.evaluate((element) => {
    element.scrollTop = 0;
  });
  await expect(list.locator(".seq").first()).toHaveText("1");
});

test("filters exact routes locally, persists them and resets the view", async ({
  page,
}) => {
  let configWrites = 0;
  page.on("request", (request) => {
    if (request.url().endsWith("/api/config") && request.method() === "POST")
      configWrites++;
  });
  await page.goto("/");
  await expect(page.locator(".traffic-row")).toHaveCount(5);
  expect(
    await page
      .locator(".traffic-row")
      .first()
      .evaluate((row) => row.getBoundingClientRect().height),
  ).toBe(26);
  await page.getByLabel("Traffic route filter").fill("220/17, 220/18");
  await expect(page.locator(".traffic-row")).toHaveCount(2);
  await page
    .locator(".traffic-row")
    .filter({ hasText: "Custom220:Send" })
    .click();
  await page
    .getByRole("button", { name: "Hide selected route", exact: true })
    .click();
  await expect(page.locator(".traffic-row")).toHaveCount(1);
  await page.reload();
  await expect(page.locator(".traffic-row")).toHaveCount(1);
  await expect(page.getByLabel("Traffic route filter")).toHaveValue(
    "220/17, 220/18, !220/18",
  );
  await page.getByLabel("Traffic route filter").fill("220/nope");
  await expect(page.getByLabel("Traffic route filter")).toHaveAttribute(
    "aria-invalid",
    "true",
  );
  await expect(page.locator(".traffic-row")).toHaveCount(0);
  await page
    .getByRole("button", { name: "Reset filters", exact: true })
    .click();
  await expect(page.locator(".traffic-row")).toHaveCount(5);
  await page.getByRole("button", { name: /Custom220:LinkedChatSend/ }).click();
  await page
    .getByRole("button", { name: "Only selected route", exact: true })
    .click();
  await expect(page.locator(".traffic-row")).toHaveCount(1);
  expect(configWrites).toBe(0);
});

test("fits a mobile viewport without overflowing the page", async ({
  page,
}) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("/");
  await page.getByRole("button", { name: /Custom220:LinkedChatSend/ }).click();
  await page.getByRole("button", { name: "RAW", exact: true }).click();
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth),
  ).toBeLessThanOrEqual(390);
  await page.screenshot({
    path: "../../../artifacts/tests/DebugWeb/inspector-mobile.png",
    fullPage: true,
  });
});

test("keeps following a sustained packet stream and lets the user scroll back", async ({
  page,
}) => {
  await page.addInitScript(() =>
    localStorage.setItem("sf.debug.scroll", "true"),
  );
  let send: ((count: number) => void) | undefined;
  let sequence = 0;
  await page.routeWebSocket("**/ws", (socket) => {
    send = (count) =>
      socket.send(
        JSON.stringify({
          session: "test-session",
          entries: Array.from({ length: count }, () => ({
            ...linked,
            seq: ++sequence,
          })),
        }),
      );
    send(4000);
  });
  await page.goto("/");
  const list = page.locator(".traffic-panel .virtual");
  await expect(list.locator(".seq").last()).toHaveText("4000");
  let batches = 0;
  const stream = setInterval(() => {
    send?.(80);
    batches++;
  }, 10);
  try {
    await expect.poll(() => batches).toBeGreaterThan(100);
    await expect
      .poll(() =>
        list.evaluate(
          (element) =>
            element.scrollHeight - element.clientHeight - element.scrollTop,
        ),
      )
      .toBeLessThanOrEqual(1);
    await list.hover();
    await page.mouse.wheel(0, -600);
    await expect(
      page.getByRole("button", { name: "FOLLOW", exact: true }),
    ).toHaveAttribute("aria-pressed", "false");
    const position = await list.locator(".seq").first().textContent();
    await expect.poll(() => batches).toBeGreaterThan(130);
    await expect(list.locator(".seq").first()).toHaveText(position!);
    await page.getByRole("button", { name: "FOLLOW", exact: true }).click();
  } finally {
    clearInterval(stream);
  }
  await expect(list.locator(".seq").last()).toHaveText(String(sequence));
  await expect(page.locator(".traffic-panel .section-heading")).toContainText(
    "10000 visible",
  );
  await page.setViewportSize({ width: 1600, height: 1400 });
  await expect
    .poll(() =>
      list.evaluate(
        (element) =>
          element.scrollHeight - element.clientHeight - element.scrollTop,
      ),
    )
    .toBeLessThanOrEqual(1);
  await expect(list.locator(".seq").last()).toHaveText(String(sequence));
  expect(await list.locator(".traffic-row").count()).toBeLessThanOrEqual(100);
  await page.getByRole("button", { name: "FOLLOW", exact: true }).click();
  await list.evaluate((element) => {
    element.scrollTop = 0;
  });
  await expect(list.locator(".seq").first()).toHaveText(
    String(sequence - 9999),
  );
});
