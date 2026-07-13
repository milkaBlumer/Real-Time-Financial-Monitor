import { useEffect, useMemo, useRef, useState } from "react";
import {
  HubConnectionBuilder,
  HttpTransportType,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { buildMonitorSocketUrl, getTransactions } from "../api/transactions";
import type { Transaction } from "../types/transaction";

type ConnectionState = "connecting" | "open" | "closed" | "error";

export interface UseTransactionStreamResult {
  transactions: Transaction[];
  connectionState: ConnectionState;
  receivedCount: number;
  reconnect: () => void;
}

const MAX_TRANSACTIONS = 1000;
const TRANSACTION_EVENT_NAME = "transaction";
const STREAM_LOG_PREFIX = "[monitor-stream]";

let bootstrapTransactionsCache: Transaction[] | null = null;
let bootstrapTransactionsRequest: Promise<Transaction[]> | null = null;

async function loadBootstrapTransactions(): Promise<Transaction[]> {
  if (bootstrapTransactionsCache !== null) {
    console.info(STREAM_LOG_PREFIX, {
      source: "bootstrap-get",
      event: "cache-hit",
      count: bootstrapTransactionsCache.length,
    });
    return bootstrapTransactionsCache;
  }

  if (bootstrapTransactionsRequest !== null) {
    return bootstrapTransactionsRequest;
  }

  bootstrapTransactionsRequest = (async () => {
    console.info(STREAM_LOG_PREFIX, {
      source: "bootstrap-get",
      event: "network-fetch-start",
    });
    const data = await getTransactions();
    console.info(STREAM_LOG_PREFIX, {
      source: "bootstrap-get",
      event: "network-fetch-success",
      count: data.length,
    });
    bootstrapTransactionsCache = data;
    return data;
  })();

  try {
    return await bootstrapTransactionsRequest;
  } finally {
    bootstrapTransactionsRequest = null;
  }
}

function logSource(source: "bootstrap-get" | "signalr", tx: Transaction): void {
  console.info(STREAM_LOG_PREFIX, {
    source,
    id: tx.id,
    status: tx.status,
    timestamp: tx.timestamp,
  });
}

function toTransaction(candidate: unknown): Transaction | null {
  if (typeof candidate === "string") {
    try {
      return toTransaction(JSON.parse(candidate));
    } catch {
      return null;
    }
  }

  if (!candidate || typeof candidate !== "object") {
    return null;
  }

  const record = candidate as Record<string, unknown>;
  const nestedCandidate =
    record.transaction ?? record.Transaction ?? record.tx ?? record.Tx;
  if (nestedCandidate) {
    return toTransaction(nestedCandidate);
  }

  const id = (record.id ?? record.Id) as string | undefined;
  const amountValue = record.amount ?? record.Amount;
  const currency = (record.currency ?? record.Currency) as string | undefined;
  const timestampRaw = (record.timestamp ?? record.Timestamp) as
    | string
    | number
    | Date
    | undefined;
  const status = (record.status ?? record.Status) as
    | Transaction["status"]
    | undefined;

  const amount =
    typeof amountValue === "number"
      ? amountValue
      : Number(amountValue ?? Number.NaN);

  if (
    typeof id !== "string" ||
    !Number.isFinite(amount) ||
    typeof currency !== "string" ||
    typeof status !== "string"
  ) {
    return null;
  }

  const normalizedStatusInput = status.trim().toLowerCase();
  const normalizedStatus: Transaction["status"] =
    normalizedStatusInput === "completed" || normalizedStatusInput === "success"
      ? "Completed"
      : normalizedStatusInput === "failed" || normalizedStatusInput === "error"
        ? "Failed"
        : "Pending";

  const timestampValue = timestampRaw ?? new Date().toISOString();
  const timestamp =
    typeof timestampValue === "string"
      ? timestampValue
      : new Date(timestampValue).toISOString();

  if (!timestamp || Number.isNaN(Date.parse(timestamp))) {
    return null;
  }

  return {
    id,
    amount,
    currency: currency.toUpperCase(),
    timestamp,
    status: normalizedStatus,
  };
}

function normalizePayload(payload: unknown): Transaction[] {
  if (Array.isArray(payload)) {
    return payload
      .map((entry) => toTransaction(entry))
      .filter((entry): entry is Transaction => entry !== null);
  }

  const single = toTransaction(payload);
  return single ? [single] : [];
}

export function useTransactionStream(): UseTransactionStreamResult {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [connectionState, setConnectionState] =
    useState<ConnectionState>("connecting");
  const [receivedCount, setReceivedCount] = useState(0);
  const [socketVersion, setSocketVersion] = useState(0);

  const queueRef = useRef<Transaction[]>([]);
  const flushHandleRef = useRef<number | null>(null);
  const reconnectTimerRef = useRef<number | null>(null);
  const connectionRef = useRef<ReturnType<
    HubConnectionBuilder["build"]
  > | null>(null);

  const flushQueue = () => {
    if (queueRef.current.length === 0) {
      return;
    }

    const batch = queueRef.current;
    queueRef.current = [];

    setTransactions((current) => {
      const byId = new Map(current.map((entry) => [entry.id, entry]));

      for (const entry of batch) {
        byId.set(entry.id, entry);
      }

      return Array.from(byId.values())
        .sort(
          (a, b) =>
            new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime(),
        )
        .slice(0, MAX_TRANSACTIONS);
    });
  };

  const scheduleFlush = () => {
    if (flushHandleRef.current !== null) {
      return;
    }

    flushHandleRef.current = window.requestAnimationFrame(() => {
      flushHandleRef.current = null;
      flushQueue();
    });
  };

  useEffect(() => {
    let isDisposed = false;

    void (async () => {
      try {
        const initialTransactions = await loadBootstrapTransactions();
        console.info(STREAM_LOG_PREFIX, {
          source: "bootstrap-get",
          event: "initial-load",
          count: initialTransactions.length,
        });

        for (const tx of initialTransactions) {
          logSource("bootstrap-get", tx);
        }

        if (isDisposed) {
          return;
        }

        setTransactions((current) => {
          const byId = new Map(current.map((entry) => [entry.id, entry]));

          for (const entry of initialTransactions) {
            byId.set(entry.id, entry);
          }

          return Array.from(byId.values())
            .sort(
              (a, b) =>
                new Date(b.timestamp).getTime() -
                new Date(a.timestamp).getTime(),
            )
            .slice(0, MAX_TRANSACTIONS);
        });

        setReceivedCount((count) =>
          Math.max(count, initialTransactions.length),
        );
      } catch {
        console.warn(STREAM_LOG_PREFIX, {
          source: "bootstrap-get",
          event: "initial-load-failed",
        });
        // Ignore bootstrap load failures and keep realtime stream active.
      }
    })();

    const connection = new HubConnectionBuilder()
      .withUrl(buildMonitorSocketUrl(), {
        withCredentials: false,
        transport:
          HttpTransportType.WebSockets |
          HttpTransportType.ServerSentEvents |
          HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    setConnectionState("connecting");

    connection.onreconnecting(() => {
      if (isDisposed) {
        return;
      }
      setConnectionState("connecting");
    });

    connection.onreconnected(() => {
      if (isDisposed) {
        return;
      }
      setConnectionState("open");
    });

    connection.onclose(() => {
      if (isDisposed) {
        return;
      }
      setConnectionState("closed");
    });

    const enqueueTransactionEvent = (payload: unknown) => {
      const normalized = normalizePayload(payload);
      if (normalized.length === 0) {
        return;
      }

      console.info(STREAM_LOG_PREFIX, {
        source: "signalr",
        event: "message",
        count: normalized.length,
      });

      for (const tx of normalized) {
        logSource("signalr", tx);
      }

      queueRef.current.push(...normalized);
      setReceivedCount((count) => count + normalized.length);
      scheduleFlush();
    };

    connection.on(TRANSACTION_EVENT_NAME, enqueueTransactionEvent);

    const startConnection = async () => {
      let attempts = 0;

      while (!isDisposed && connectionRef.current === connection) {
        if (connection.state === HubConnectionState.Connected) {
          setConnectionState("open");
          return;
        }

        if (connection.state !== HubConnectionState.Disconnected) {
          return;
        }

        try {
          await connection.start();
          if (!isDisposed && connectionRef.current === connection) {
            setConnectionState("open");
          }
          return;
        } catch {
          if (isDisposed || connectionRef.current !== connection) {
            return;
          }

          attempts += 1;
          setConnectionState("error");

          // Short bounded retry for startup race/transient network issues.
          if (attempts >= 3) {
            return;
          }

          await new Promise<void>((resolve) => {
            reconnectTimerRef.current = window.setTimeout(() => {
              reconnectTimerRef.current = null;
              resolve();
            }, 500 * attempts);
          });
        }
      }
    };

    const startTask = startConnection();

    return () => {
      isDisposed = true;

      connection.off(TRANSACTION_EVENT_NAME, enqueueTransactionEvent);

      if (connectionRef.current === connection) {
        connectionRef.current = null;
      }

      if (reconnectTimerRef.current !== null) {
        window.clearTimeout(reconnectTimerRef.current);
        reconnectTimerRef.current = null;
      }

      void (async () => {
        try {
          await startTask;
        } catch {
          // Ignore start failures during teardown.
        }

        if (connection.state !== HubConnectionState.Disconnected) {
          try {
            await connection.stop();
          } catch {
            // Ignore stop failures during teardown.
          }
        }
      })();

      if (flushHandleRef.current !== null) {
        window.cancelAnimationFrame(flushHandleRef.current);
        flushHandleRef.current = null;
      }
      flushQueue();
    };
  }, [socketVersion]);

  const reconnect = useMemo(
    () => () => setSocketVersion((version) => version + 1),
    [],
  );

  return {
    transactions,
    connectionState,
    receivedCount,
    reconnect,
  };
}
