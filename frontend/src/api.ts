export interface ApiErrorBody {
  code: string;
  message: string;
}

export class ApiError extends Error {
  readonly code: string;
  readonly status: number;

  constructor(status: number, code: string, message: string) {
    super(message);
    this.name = "ApiError";
    this.code = code;
    this.status = status;
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(path, {
    headers: init?.body ? { "Content-Type": "application/json" } : undefined,
    ...init
  });

  if (!res.ok) {
    let body: ApiErrorBody | undefined;
    try {
      body = (await res.json()) as ApiErrorBody;
    } catch {
      // тело не JSON — оставим undefined
    }
    throw new ApiError(
      res.status,
      body?.code ?? "UNKNOWN",
      body?.message ?? `Ошибка сервера (HTTP ${res.status})`
    );
  }

  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}

// --- типы ---

export interface TimeEntryRow {
  id: string;
  employeeId: string;
  employeeName: string;
  projectId: string;
  projectCode: string;
  date: string;
  hours: number;
  rate: number;
  amount: number;
  comment: string;
  overtime: boolean;
  version: number;
}

export interface TimeEntryPageResult {
  items: TimeEntryRow[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalHours: number;
  totalAmount: number;
}

export interface Employee {
  id: string;
  name: string;
  department: string;
}

export interface Project {
  id: string;
  code: string;
  name: string;
  start: string;
  end: string | null;
}

export interface ProjectReportRow {
  projectId: string;
  code: string;
  name: string;
  hours: number;
  amount: number;
  budget: number;
  percent: number | null;
  overspent: boolean;
  atRisk: boolean;
}

export interface EntryInput {
  employeeId: string;
  projectId: string;
  date: string; // YYYY-MM-DD
  hours: number;
  comment: string;
}

// --- API ---

export const api = {
  listEntries: (params: {
    year: number;
    month: number;
    employeeId?: string;
    projectId?: string;
    page?: number;
    pageSize?: number;
  }): Promise<TimeEntryPageResult> => {
    const qs = new URLSearchParams({
      year: String(params.year),
      month: String(params.month),
      page: String(params.page ?? 1),
      pageSize: String(params.pageSize ?? 20)
    });
    if (params.employeeId) qs.set("employeeId", params.employeeId);
    if (params.projectId) qs.set("projectId", params.projectId);
    return request(`/api/time-entries?${qs.toString()}`);
  },

  createEntry: (input: EntryInput): Promise<TimeEntryRow> =>
    request("/api/time-entries", { method: "PUT", body: JSON.stringify(input) }),

  updateEntry: (id: string, expectedVersion: number, input: EntryInput): Promise<TimeEntryRow> =>
    request(`/api/time-entries/${id}`, {
      method: "POST",
      body: JSON.stringify({ ...input, expectedVersion })
    }),

  deleteEntry: (id: string): Promise<void> =>
    request(`/api/time-entries/${id}`, { method: "DELETE" }),

  employees: (): Promise<Employee[]> => request("/api/employees"),

  projects: (): Promise<Project[]> => request("/api/projects"),

  projectReport: (year: number, month: number): Promise<ProjectReportRow[]> =>
    request(`/api/reports/projects?year=${year}&month=${month}`),

  closePeriod: (year: number, month: number): Promise<void> =>
    request("/api/periods/close", { method: "POST", body: JSON.stringify({ year, month }) }),

  openPeriod: (year: number, month: number): Promise<void> =>
    request("/api/periods/open", { method: "POST", body: JSON.stringify({ year, month }) })
};
