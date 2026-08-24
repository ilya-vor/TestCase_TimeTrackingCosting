import { useState } from "react";
import { ApiError, Employee, EntryInput, Project, TimeEntryRow } from "../api";

function toDateInputValue(iso: string): string {
  return iso.slice(0, 10);
}

export function EntryForm({
  initial,
  employees,
  projects,
  submitLabel,
  onSave
}: {
  initial: TimeEntryRow | null;
  employees: Employee[];
  projects: Project[];
  submitLabel: string;
  onSave: (input: EntryInput) => Promise<void>;
}) {
  const [employeeId, setEmployeeId] = useState(initial?.employeeId ?? "");
  const [projectId, setProjectId] = useState(initial?.projectId ?? "");
  const [date, setDate] = useState(initial ? toDateInputValue(initial.date) : "");
  const [hours, setHours] = useState(initial ? String(initial.hours) : "");
  const [comment, setComment] = useState(initial?.comment ?? "");

  const [clientError, setClientError] = useState<string | null>(null);
  const [serverError, setServerError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const validate = (): string | null => {
    if (!employeeId) return "Выберите сотрудника.";
    if (!projectId) return "Выберите проект.";
    if (!date) return "Укажите дату.";

    const h = Number(hours);
    if (!Number.isFinite(h) || h <= 0) return "Часы должны быть положительным числом.";
    if (h > 24) return "Часы одной записи не могут превышать 24.";
    if (Math.abs(h % 0.5) > 1e-9) return "Часы должны быть кратны 0,5.";
    return null;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const error = validate();
    setClientError(error);
    if (error) return;

    setSaving(true);
    setServerError(null);
    try {
      await onSave({
        employeeId,
        projectId,
        date,
        hours: Number(hours),
        comment
      });
    } catch (err) {
      setServerError(err instanceof ApiError ? err.message : "Не удалось сохранить запись.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <form className="entry-form" onSubmit={handleSubmit}>
      <label>
        Сотрудник
        <select value={employeeId} onChange={(e) => setEmployeeId(e.target.value)}>
          <option value="">— выберите —</option>
          {employees.map((emp) => (
            <option key={emp.id} value={emp.id}>
              {emp.name}
            </option>
          ))}
        </select>
      </label>

      <label>
        Проект
        <select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
          <option value="">— выберите —</option>
          {projects.map((p) => (
            <option key={p.id} value={p.id}>
              {p.code} · {p.name}
            </option>
          ))}
        </select>
      </label>

      <label>
        Дата
        <input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
      </label>

      <label>
        Часы (кратно 0,5, до 24)
        <input
          type="number"
          inputMode="decimal"
          step="0.5"
          min="0.5"
          max="24"
          value={hours}
          onChange={(e) => setHours(e.target.value)}
        />
      </label>

      <label>
        Комментарий
        <input type="text" value={comment} onChange={(e) => setComment(e.target.value)} />
      </label>

      {clientError && <div className="form-error">{clientError}</div>}
      {serverError && <div className="form-error">{serverError}</div>}

      <div className="form-actions">
        <button type="submit" className="primary" disabled={saving}>
          {saving ? "Сохранение..." : submitLabel}
        </button>
      </div>
    </form>
  );
}
