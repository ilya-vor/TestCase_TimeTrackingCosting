import { useState } from "react";
import { useProjectReport } from "../hooks";
import { MonthPicker } from "../components/MonthPicker";
import { ProjectReportRow } from "../api";

const fmtMoney = (n: number) => `${n.toLocaleString("ru-RU", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ₽`;
const fmtHours = (n: number) => n.toLocaleString("ru-RU", { maximumFractionDigits: 1 });

function percentText(row: ProjectReportRow): string {
  if (row.code === "ИТОГО") return "";
  return row.percent == null ? "—" : `${row.percent.toLocaleString("ru-RU", { maximumFractionDigits: 2 })} %`;
}

export function ReportsScreen() {
  const [month, setMonth] = useState({ year: 2026, month: 3 });
  const report = useProjectReport(month.year, month.month);

  return (
    <section>
      <div className="toolbar">
        <MonthPicker value={month} onChange={setMonth} />
      </div>

      {report.isLoading && <div className="hint">Загрузка...</div>}
      {report.isError && (
        <div className="form-error">
          Не удалось загрузить отчёт: {report.error instanceof Error ? report.error.message : "ошибка"}
        </div>
      )}

      {report.data && (
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Проект</th>
                <th className="num">Часы</th>
                <th className="num">Стоимость</th>
                <th className="num">Бюджет</th>
                <th className="num">Освоено</th>
                <th>Статус</th>
              </tr>
            </thead>
            <tbody>
              {report.data.map((row, i) => {
                const isTotal = row.code === "ИТОГО";
                return (
                  <tr
                    key={i}
                    className={
                      isTotal ? "total-row" : row.overspent ? "row-overspent" : row.atRisk ? "row-risk" : undefined
                    }
                  >
                    <td>
                      {isTotal ? row.name : `${row.code} · ${row.name}`}
                    </td>
                    <td className="num">{fmtHours(row.hours)}</td>
                    <td className="num">{fmtMoney(row.amount)}</td>
                    <td className="num">{isTotal ? "" : fmtMoney(row.budget)}</td>
                    <td className="num">{percentText(row)}</td>
                    <td>
                      {!isTotal && row.overspent && <span className="badge overspent">перерасход</span>}
                      {!isTotal && row.atRisk && !row.overspent && <span className="badge risk">риск</span>}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
