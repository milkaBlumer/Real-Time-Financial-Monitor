import type { TransactionStatus } from "../types/transaction";

interface StatusPillProps {
  status: TransactionStatus;
}

export function StatusPill({ status }: StatusPillProps) {
  const className =
    status === "Failed"
      ? "status-pill failed"
      : status === "Completed"
        ? "status-pill success"
        : "status-pill pending";

  return <span className={className}>{status}</span>;
}
