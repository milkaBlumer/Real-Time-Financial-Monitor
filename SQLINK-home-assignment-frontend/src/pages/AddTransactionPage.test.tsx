import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { AddTransactionPage } from "./AddTransactionPage";

const postTransactionMock = vi.fn();

vi.mock("../api/transactions", () => ({
  postTransaction: (...args: unknown[]) => postTransactionMock(...args),
}));

function createMockTransaction(status: "Pending" | "Completed" | "Failed") {
  return {
    id: crypto.randomUUID(),
    amount: 10,
    currency: "USD",
    timestamp: new Date().toISOString(),
    status,
  };
}

describe("AddTransactionPage", () => {
  it("submits a generated transaction and shows success feedback", async () => {
    const user = userEvent.setup();
    postTransactionMock.mockResolvedValueOnce({
      accepted: true,
      id: "tx-success",
      message: "ok",
    });

    render(
      <AddTransactionPage createMockTransaction={createMockTransaction} />,
    );

    await user.clear(screen.getByLabelText("Amount"));
    await user.type(screen.getByLabelText("Amount"), "33.5");
    await user.clear(screen.getByLabelText("Currency"));
    await user.type(screen.getByLabelText("Currency"), "eur");
    await user.selectOptions(screen.getByLabelText("Status"), "Completed");
    await user.click(screen.getByRole("button", { name: "Generate & Send" }));

    await waitFor(() => expect(postTransactionMock).toHaveBeenCalledTimes(1));

    const call = postTransactionMock.mock.calls[0][0] as {
      amount: number;
      currency: string;
      status: string;
    };

    expect(call.amount).toBe(33.5);
    expect(call.currency).toBe("EUR");
    expect(call.status).toBe("Completed");
    expect(screen.getByText("ok")).toBeInTheDocument();
  });

  it("shows error message when submit fails", async () => {
    const user = userEvent.setup();
    postTransactionMock.mockRejectedValueOnce(new Error("network down"));

    render(
      <AddTransactionPage createMockTransaction={createMockTransaction} />,
    );

    await user.click(screen.getByRole("button", { name: "Generate & Send" }));

    expect(await screen.findByText("network down")).toBeInTheDocument();
  });
});
