import { useMemo, useState } from "react";
import { StatusPill } from "../components/StatusPill";
import type { Transaction } from "../types/transaction";
import type { UseTransactionStreamResult } from "../hooks/useTransactionStream";

type StatusFilter = "All" | Transaction["status"];

export function MonitorPage({
  transactions,
  connectionState,
  reconnect,
  // clear,
}: UseTransactionStreamResult) {
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("All");

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

        {/* <button type="button" className="secondary-btn" onClick={clear}>
          Clear list
        </button> */}
      </div>

      <div
        className="table-shell"
        role="region"
        aria-label="Transaction stream table"
      >
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Amount</th>
              <th>Currency</th>
              <th>Status</th>
              <th>Time</th>
            </tr>
          </thead>
          <tbody>
            {visibleTransactions.map((transaction: Transaction) => (
              <tr
                key={`${transaction.id}-${transaction.timestamp}-${transaction.amount}`}
              >
                <td className="mono-cell">{transaction.id}</td>
                <td>{transaction.amount.toFixed(2)}</td>
                <td>{transaction.currency}</td>
                <td>
                  <StatusPill status={transaction.status} />
                </td>
                <td>{new Date(transaction.timestamp).toLocaleTimeString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <p className="list-count">
        Visible rows: {visibleTransactions.length} / {transactions.length}
      </p>
    </section>
  );
}
