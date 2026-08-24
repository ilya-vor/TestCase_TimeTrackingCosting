import { useState } from "react";
import { TimeEntriesScreen } from "./screens/TimeEntriesScreen";
import { ReportsScreen } from "./screens/ReportsScreen";

type Tab = "entries" | "reports";

export function App() {
  const [tab, setTab] = useState<Tab>("entries");

  return (
    <div className="app">
      <header className="app-header">
        <h1>Учёт трудозатрат по проектам</h1>
        <nav>
          <button className={tab === "entries" ? "tab active" : "tab"} onClick={() => setTab("entries")}>
            Табель
          </button>
          <button className={tab === "reports" ? "tab active" : "tab"} onClick={() => setTab("reports")}>
            Отчёт по проектам
          </button>
        </nav>
      </header>
      <main>{tab === "entries" ? <TimeEntriesScreen /> : <ReportsScreen />}</main>
    </div>
  );
}
