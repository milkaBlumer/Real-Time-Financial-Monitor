import { afterEach, describe, expect, it, vi } from "vitest";
import {
  buildMonitorSocketUrl,
  getTransactions,
  postTransaction,
} from "../../api/transactions";
import type { Transaction } from "../../types/transaction";

describe("transactions api", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("buildMonitorSocketUrl converts ws to http for SignalR negotiate", () => {
    expect(buildMonitorSocketUrl()).toContain("http");
    expect(buildMonitorSocketUrl()).toContain("/hub/transactions");
  });

  it("postTransaction sends payload and parses response", async () => {
    const tx: Transaction = {
      id: "tx-1",
      amount: 10,
      currency: "USD",
      timestamp: "2026-01-01T00:00:00.000Z",
      status: "Pending",
    };

    const fetchMock = vi.spyOn(globalThis, "fetch").mockResolvedValue(
      new Response(JSON.stringify({ accepted: true, id: "tx-1" }), {
        status: 202,
      }),
    );

    const result = await postTransaction(tx);

    expect(fetchMock).toHaveBeenCalledTimes(1);
    expect(result.accepted).toBe(true);
    expect(result.id).toBe("tx-1");
  });

  it("getTransactions throws on non-ok responses", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue(
      new Response("boom", { status: 500 }),
    );

    await expect(getTransactions()).rejects.toThrow("boom");
  });
});
