import { useState } from "react";
import { useCreateEntry, useDeleteEntry, useEmployees, useProjects, useTimeEntries, useUpdateEntry } from "../hooks";
import { ApiError, EntryInput, TimeEntryRow } from "../api";
import { MonthPicker } from "../components/MonthPicker";
import { Modal } from "../components/Modal";
import { EntryForm } from "../components/EntryForm";

const fmtMoney = (n: number) => `${n.toLocaleString("ru-RU", { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ₽`;
const fmtDate = (iso: string) => new Date(iso).toLocaleDateString("ru-RU");
const fmtHours = (n: number) => n.toLocaleString("ru-RU", { maximumFractionDigits: 1 });

type ModalState = { mode: "create" } | { mode: "edit"; row: TimeEntryRow } | null;

export function TimeEntriesScreen() {
  const [month, setMonth] = useState({ year: 2026, month: 3 });
  const [employeeFilter, setEmployeeFilter] = useState("");
  const [projectFilter, setProjectFilter] = useState("");
  const [page, setPage] = useState(1);
  const [modal, setModal] = useState<ModalState>(null);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const employees = useEmployees();
  const projects = useProjects();
  const entries = useTimeEntries({
    year: month.year,
    month: month.month,
    employeeId: employeeFilter || undefined,
    projectId: projectFilter || undefined,
    page
  });

  const createEntry = useCreateEntry();
  const updateEntry = useUpdateEntry();
  const deleteEntry = useDeleteEntry();

  const totalPages = Math.max(1, Math.ceil((entries.data?.totalCount ?? 0) / 20));

  const handleSave = async (input: EntryInput) => {
    if (modal?.mode === "edit") {
      await updateEntry.mutateAsync({ id: modal.row.id, version: modal.row.version, input });
    } else {
      await createEntry.mutateAsync(input);
    }
    setModal(null);
  };

  const handleDelete = async (row: TimeEntryRow) => {
    if (!window.confirm(`Удалить запись от ${fmtDate(row.date)} (${row.employeeName})?`)) return;
    setDeleteError(null);
    try {
      await deleteEntry.mutateAsync(row.id);
    } catch (err) {
      setDeleteError(err instanceof ApiError ? err.message : "Не удалось удалить запись.");
    }
  };

  return (
    <section>
      <div className="toolbar">
        <MonthPicker value={month} onChange={(m) => { setMonth(m); setPage(1); }} />
        <select value={employeeFilter} onChange={(e) => { setEmployeeFilter(e.target.value); setPage(1); }}>
          <option value="">Все сотрудники</option>
          {(employees.data ?? []).map((emp) => (
            <option key={emp.id} value={emp.id}>
              {emp.name}
            </option>
          ))}
        </select>
        <select value={projectFilter} onChange={(e) => { setProjectFilter(e.target.value); setPage(1); }}>
          <option value="">Все проекты</option>
          {(projects.data ?? []).map((p) => (
            <option key={p.id} value={p.id}>
              {p.code}
            </option>
          ))}
        </select>
        <button className="primary" onClick={() => setModal({ mode: "create" })}>
          Добавить запись
        </button>
      </div>

      {entries.isLoading && <div className="hint">Загрузка...</div>}
      {entries.isError && (
        <div className="form-error">
          Не удалось загрузить записи: {entries.error instanceof Error ? entries.error.message : "ошибка"}
        </div>
      )}
      {deleteError && <div className="form-error">{deleteError}</div>}

      {entries.data && (
        <>
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Дата</th>
                  <th>Сотрудник</th>
                  <th>Проект</th>
                  <th className="num">Часы</th>
                  <th className="num">Ставка</th>
                  <th className="num">Стоимость</th>
                  <th>Комментарий</th>
                  <th>Переработка</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {entries.data.items.map((row) => (
                  <tr key={row.id} className={row.overtime ? "row-overtime" : undefined}>
                    <td>{fmtDate(row.date)}</td>
                    <td>{row.employeeName}</td>
                    <td>{row.projectCode}</td>
                    <td className="num">{fmtHours(row.hours)}</td>
                    <td className="num">{fmtMoney(row.rate)}</td>
                    <td className="num">{fmtMoney(row.amount)}</td>
                    <td className="comment">{row.comment || "—"}</td>
                    <td>{row.overtime && <span className="badge overtime">переработка</span>}</td>
                    <td className="actions">
                      <button onClick={() => setModal({ mode: "edit", row })}>Изменить</button>
                      <button className="danger" onClick={() => handleDelete(row)}>
                        Удалить
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
              <tfoot>
                <tr className="total-row">
                  <td colSpan={3}>Итого по выборке</td>
                  <td className="num">{fmtHours(entries.data.totalHours)}</td>
                  <td className="num"></td>
                  <td className="num">{fmtMoney(entries.data.totalAmount)}</td>
                  <td colSpan={3}></td>
                </tr>
              </tfoot>
            </table>
          </div>

          <div className="pagination">
            <button disabled={page <= 1} onClick={() => setPage(page - 1)}>
              ← Назад
            </button>
            <span>
              Стр. {page} из {totalPages} · записей: {entries.data.totalCount}
            </span>
            <button disabled={page >= totalPages} onClick={() => setPage(page + 1)}>
              Вперёд →
            </button>
          </div>
        </>
      )}

      {modal && (
        <Modal title={modal.mode === "edit" ? "Изменить запись" : "Новая запись"} onClose={() => setModal(null)}>
          <EntryForm
            initial={modal.mode === "edit" ? modal.row : null}
            employees={employees.data ?? []}
            projects={projects.data ?? []}
            submitLabel={modal.mode === "edit" ? "Сохранить" : "Добавить"}
            onSave={handleSave}
          />
        </Modal>
      )}
    </section>
  );
}
