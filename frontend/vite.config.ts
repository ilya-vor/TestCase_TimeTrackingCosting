import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// В dev-режиме Vite проксирует /api на backend (http://localhost:5080).
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: "http://localhost:5080",
        changeOrigin: true
      }
    }
  }
});
