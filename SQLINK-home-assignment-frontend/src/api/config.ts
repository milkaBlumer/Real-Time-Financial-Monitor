const httpBase = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7276";
const wsBase = import.meta.env.VITE_WS_BASE_URL ?? httpBase;

export const apiConfig = {
  httpBase,
  wsBase,
  ingestPath: "/api/Ingest",
  monitorPath: "/hub/transactions",
};
