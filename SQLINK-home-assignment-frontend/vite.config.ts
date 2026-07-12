import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  test: {
    environment: "jsdom",
    setupFiles: "./src/test/setup.ts",
    globals: true,
    clearMocks: true,
    coverage: {
      provider: "v8",
      reporter: ["text", "html", "cobertura"],
      include: ["src/**/*.{ts,tsx}"],
      thresholds: {
        lines: 80,
        functions: 80,
        statements: 80,
        branches: 70,
      },
      exclude: [
        "**/*.test.ts",
        "**/*.test.tsx",
        "dist/**",
        "vite.config.ts",
        "src/main.tsx",
        "src/App.tsx",
        "src/hooks/useTransactionStream.ts",
        "src/types/**",
        "src/test/**",
        "**/*.d.ts",
      ],
    },
  },
});
