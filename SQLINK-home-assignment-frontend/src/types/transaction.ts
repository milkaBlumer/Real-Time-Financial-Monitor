export type TransactionStatus = "Pending" | "Completed" | "Failed";

export interface Transaction {
  id: string;
  amount: number;
  currency: string;
  timestamp: string;
  status: TransactionStatus;
}

export interface IngestResponse {
  accepted: boolean;
  id?: string;
  message?: string;
}

export interface TransactionFilter {
  showOnlyErrors: boolean;
}
