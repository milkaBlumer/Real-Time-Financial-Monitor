import { useMemo, useState } from "react";
import type { FormEvent } from "react";
import { postTransaction } from "../api/transactions";
import type { Transaction, TransactionStatus } from "../types/transaction";

interface AddTransactionPageProps {
  createMockTransaction: (status: TransactionStatus) => Transaction;
}

export function AddTransactionPage({
  createMockTransaction,
}: AddTransactionPageProps) {
  const [amount, setAmount] = useState(120);
  const [currency, setCurrency] = useState("USD");
  const [selectedStatus, setSelectedStatus] =
    useState<TransactionStatus>("Pending");
  const [loading, setLoading] = useState(false);
  const [resultMessage, setResultMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [previewSeed, setPreviewSeed] = useState(0);

  const transactionPreview = useMemo(
    () => createMockTransaction(selectedStatus),
    [createMockTransaction, selectedStatus, previewSeed],
  );

  const submitTransaction = async (event: FormEvent) => {
    event.preventDefault();
    setLoading(true);
    setResultMessage(null);
    setErrorMessage(null);

    // Create a fresh transaction for each submit to avoid duplicate IDs.
    const transaction: Transaction = {
      ...createMockTransaction(selectedStatus),
      amount,
      currency,
      status: selectedStatus,
    };

    try {
      const response = await postTransaction(transaction);
      setResultMessage(
        response.message ??
          `Transaction ${response.id ?? transaction.id} sent successfully.`,
      );
      setPreviewSeed((value) => value + 1);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Failed to submit transaction.";
      setErrorMessage(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="page-card">
      <h2>Transaction Simulator</h2>
      <p className="subtitle">
        Generate synthetic transactions and post them to the ingestion API.
      </p>

      <form className="simulator-form" onSubmit={submitTransaction}>
        <label>
          Amount
          <input
            type="number"
            min={1}
            step={0.01}
            value={amount}
            onChange={(event) => setAmount(Number(event.target.value))}
            required
          />
        </label>

        <label>
          Currency
          <input
            type="text"
            value={currency}
            onChange={(event) => setCurrency(event.target.value.toUpperCase())}
            placeholder="USD"
            maxLength={3}
            required
          />
        </label>

        <label>
          Status
          <select
            value={selectedStatus}
            onChange={(event) =>
              setSelectedStatus(event.target.value as TransactionStatus)
            }
          >
            <option value="Pending">Pending</option>
            <option value="Completed">Completed</option>
            <option value="Failed">Failed</option>
          </select>
        </label>

        <button type="submit" className="primary-btn" disabled={loading}>
          {loading ? "Sending..." : "Generate & Send"}
        </button>
      </form>

      <div className="preview-box">
        <h3>Payload preview</h3>
        <pre>{JSON.stringify(transactionPreview, null, 2)}</pre>
      </div>

      {resultMessage && (
        <p className="feedback success-text">{resultMessage}</p>
      )}
      {errorMessage && <p className="feedback error-text">{errorMessage}</p>}
    </section>
  );
}
