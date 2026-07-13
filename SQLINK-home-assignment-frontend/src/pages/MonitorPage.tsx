import { useEffect, useMemo, useRef, useState } from "react";
import { StatusPill } from "../components/StatusPill";
import type { Transaction } from "../types/transaction";
import type { UseTransactionStreamResult } from "../hooks/useTransactionStream";

type StatusFilter = "All" | Transaction["status"];
const NEW_BADGE_DURATION_MS = 60_000;

function getNewBadgeExpiry(transaction: Transaction): number | null {
  const createdAt = Date.parse(transaction.timestamp);
  if (Number.isNaN(createdAt)) {
    return null;
  }

  return createdAt + NEW_BADGE_DURATION_MS;
}

export function MonitorPage({
  transactions,
  connectionState,
  reconnect,
}: UseTransactionStreamResult) {
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("All");
  const [newBadgeClock, setNewBadgeClock] = useState(() => Date.now());
  const expiryTimerRef = useRef<number | null>(null);

  useEffect(() => {
    const now = Date.now();
    if (expiryTimerRef.current !== null) {
      window.clearTimeout(expiryTimerRef.current);
      expiryTimerRef.current = null;
    }

    let nextExpiry = Number.POSITIVE_INFINITY;
    for (const transaction of transactions) {
      const expiresAt = getNewBadgeExpiry(transaction);
      if (expiresAt === null) {
        continue;
      }

      if (expiresAt > now && expiresAt < nextExpiry) {
        nextExpiry = expiresAt;
      }
    }

    if (Number.isFinite(nextExpiry)) {
      expiryTimerRef.current = window.setTimeout(
        () => {
          setNewBadgeClock(Date.now());
        },
        Math.max(16, nextExpiry - now + 20),
      );
    }
  }, [transactions, newBadgeClock]);

  useEffect(() => {
    return () => {
      if (expiryTimerRef.current !== null) {
        window.clearTimeout(expiryTimerRef.current);
      }
    };
  }, []);

  const visibleTransactions = useMemo(() => {
    if (statusFilter === "All") {
      return transactions;
    }

    return transactions.filter(
      (transaction: Transaction) => transaction.status === statusFilter,
    );
  }, [transactions, statusFilter]);

  return (
    <section className="page-card">
      <div className="monitor-header">
        <div>
          <h2>Live Monitor</h2>
          <p className="subtitle">Streaming last financial transactions.</p>
        </div>

        <div className="connection-state" data-state={connectionState}>
          {connectionState.toUpperCase()}
        </div>
      </div>

      <div className="monitor-controls">
        <button type="button" className="secondary-btn" onClick={reconnect}>
          Reconnect
        </button>

        <label className="status-filter-control">
          Status
          <select
            value={statusFilter}
            onChange={(event) =>
              setStatusFilter(event.target.value as StatusFilter)
            }
            aria-label="Filter transactions by status"
          >
            <option value="All">All</option>
            <option value="Pending">Pending</option>
            <option value="Completed">Completed</option>
            <option value="Failed">Failed</option>
          </select>
        </label>
      </div>

      <div
        className="table-shell"
        role="region"
        aria-label="Transaction stream table"
      >
        <table>
          <thead>
            <tr>
              <th>Amount</th>
              <th>Currency</th>
              <th>Status</th>
              <th>Time</th>
              <th className="row-flag-col"></th>
            </tr>
          </thead>
          <tbody>
            {visibleTransactions.map((transaction: Transaction) => {
              const expiresAt = getNewBadgeExpiry(transaction);
              const isNew = expiresAt !== null && expiresAt > newBadgeClock;

              return (
                <tr key={transaction.id}>
                  <td>{transaction.amount.toFixed(2)}</td>
                  <td>{transaction.currency}</td>
                  <td>
                    <StatusPill status={transaction.status} />
                  </td>
                  <td>
                    {new Date(transaction.timestamp).toLocaleTimeString()}
                  </td>
                  <td className="row-flag-cell">
                    {isNew ? <span className="new-badge">New</span> : null}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      <p className="list-count">
        Visible rows: {visibleTransactions.length} / {transactions.length}
      </p>
    </section>
  );
}
