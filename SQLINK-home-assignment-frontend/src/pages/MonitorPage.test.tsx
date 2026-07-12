import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { MonitorPage } from "./MonitorPage";
import type { UseTransactionStreamResult } from "../hooks/useTransactionStream";

function buildStream(
  overrides?: Partial<UseTransactionStreamResult>,
): UseTransactionStreamResult {
  return {
    transactions: [
      {
        id: "tx-1",
        amount: 120.5,
        currency: "USD",
        status: "Pending",
        timestamp: "2026-01-01T10:00:00.000Z",
      },
      {
        id: "tx-2",
        amount: 99.9,
        currency: "EUR",
        status: "Failed",
        timestamp: "2026-01-01T11:00:00.000Z",
      },
      {
        id: "tx-3",
        amount: 50,
        currency: "USD",
        status: "Completed",
        timestamp: "2026-01-01T12:00:00.000Z",
      },
    ],
    connectionState: "open",
    receivedCount: 3,
    reconnect: vi.fn(),
    clear: vi.fn(),
    ...overrides,
  };
}

describe("MonitorPage", () => {
  it("shows all rows by default", () => {
    render(<MonitorPage {...buildStream()} />);

    expect(screen.getByText("Visible rows: 3 / 3")).toBeInTheDocument();
    expect(screen.getByText("tx-1")).toBeInTheDocument();
    expect(screen.getByText("tx-2")).toBeInTheDocument();
    expect(screen.getByText("tx-3")).toBeInTheDocument();
  });

  it("filters rows by selected status", async () => {
    const user = userEvent.setup();
    render(<MonitorPage {...buildStream()} />);

    await user.selectOptions(
      screen.getByLabelText("Filter transactions by status"),
      "Failed",
    );

    expect(screen.getByText("Visible rows: 1 / 3")).toBeInTheDocument();
    expect(screen.queryByText("tx-1")).not.toBeInTheDocument();
    expect(screen.getByText("tx-2")).toBeInTheDocument();
    expect(screen.queryByText("tx-3")).not.toBeInTheDocument();
  });

  it("invokes reconnect callback", async () => {
    const user = userEvent.setup();
    const reconnect = vi.fn();

    render(<MonitorPage {...buildStream({ reconnect })} />);

    await user.click(screen.getByRole("button", { name: "Reconnect" }));
    expect(reconnect).toHaveBeenCalledTimes(1);
  });
});
