import { apiConfig } from "./config";
import type { IngestResponse, Transaction } from "../types/transaction";

function buildUrl(path: string): string {
  return new URL(path, apiConfig.httpBase).toString();
}

function buildWsUrl(path: string): string {
  return new URL(path, apiConfig.wsBase).toString();
}

function toSignalRHttpUrl(url: string): string {
  if (url.startsWith("wss://")) {
    return `https://${url.slice("wss://".length)}`;
  }

  if (url.startsWith("ws://")) {
    return `http://${url.slice("ws://".length)}`;
  }

  return url;
}

export async function postTransaction(
  transaction: Transaction,
): Promise<IngestResponse> {
  const response = await fetch(buildUrl(apiConfig.ingestPath), {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(transaction),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(
      text || `Failed to ingest transaction. HTTP status ${response.status}`,
    );
  }

  return (await response.json()) as IngestResponse;
}

export async function getTransactions(): Promise<Transaction[]> {
  const response = await fetch(buildUrl(apiConfig.ingestPath), {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
    },
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(
      text || `Failed to load transactions. HTTP status ${response.status}`,
    );
  }

  return (await response.json()) as Transaction[];
}

export function buildMonitorSocketUrl(): string {
  return toSignalRHttpUrl(buildWsUrl(apiConfig.monitorPath));
}
