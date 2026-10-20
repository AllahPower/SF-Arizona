import { defineConfig } from "vite";
import preact from "@preact/preset-vite";

export default defineConfig({
  plugins: [preact()],
  build: { outDir: "../../../artifacts/frontend/DebugWeb", emptyOutDir: true },
  server: {
    host: "127.0.0.1",
    proxy: {
      "/api": {
        target: "http://localhost:7777",
        changeOrigin: true,
        headers: { Origin: "http://localhost:7777" },
      },
      "/ws": {
        target: "ws://localhost:7777",
        ws: true,
        changeOrigin: true,
        headers: { Origin: "http://localhost:7777" },
      },
    },
  },
});
