import { Navigate, NavLink, Route, Routes } from "react-router-dom";
import { AddTransactionPage } from "./pages/AddTransactionPage";
import { MonitorPage } from "./pages/MonitorPage";
import { useTransactionStream } from "./hooks/useTransactionStream";
import type { Transaction, TransactionStatus } from "./types/transaction";
import "./App.css";

function createMockTransaction(status: TransactionStatus): Transaction {
  return {
    id: crypto.randomUUID(),
    amount: Number((10 + Math.random() * 5000).toFixed(2)),
    currency: "USD",
    timestamp: new Date().toISOString(),
    status,
  };
}

function App() {
  const stream = useTransactionStream();

  return (
    <div className="app-shell">
      <header className="topbar">
        <div>
          <h1>Transaction Control Room</h1>
          <p>Simulator + live monitor for the ingestion engine.</p>
        </div>

        <nav className="nav-links" aria-label="Main routes">
          <NavLink to="/add">Simulator</NavLink>
          <NavLink to="/monitor">Monitor</NavLink>
        </nav>
      </header>

      <main>
        <Routes>
          <Route
            path="/add"
            element={
              <AddTransactionPage
                createMockTransaction={createMockTransaction}
              />
            }
          />
          <Route path="/monitor" element={<MonitorPage {...stream} />} />
          <Route path="*" element={<Navigate to="/add" replace />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
