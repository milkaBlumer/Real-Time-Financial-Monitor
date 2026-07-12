import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { StatusPill } from "./StatusPill";

describe("StatusPill", () => {
  it("renders success style for Completed", () => {
    render(<StatusPill status="Completed" />);

    const badge = screen.getByText("Completed");
    expect(badge).toHaveClass("status-pill", "success");
  });

  it("renders failed style for Failed", () => {
    render(<StatusPill status="Failed" />);

    const badge = screen.getByText("Failed");
    expect(badge).toHaveClass("status-pill", "failed");
  });

  it("renders pending style for Pending", () => {
    render(<StatusPill status="Pending" />);

    const badge = screen.getByText("Pending");
    expect(badge).toHaveClass("status-pill", "pending");
  });
});
