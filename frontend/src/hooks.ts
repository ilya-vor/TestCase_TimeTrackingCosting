import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api, EntryInput } from "./api";

export function useEmployees() {
  return useQuery({ queryKey: ["employees"], queryFn: api.employees });
}

export function useProjects() {
  return useQuery({ queryKey: ["projects"], queryFn: api.projects });
}

export function useTimeEntries(params: {
  year: number;
  month: number;
  employeeId?: string;
  projectId?: string;
  page: number;
}) {
  return useQuery({
    queryKey: ["entries", params.year, params.month, params.employeeId, params.projectId, params.page],
    queryFn: () => api.listEntries({ ...params, pageSize: 20 })
  });
}

export function useProjectReport(year: number, month: number) {
  return useQuery({
    queryKey: ["report", year, month],
    queryFn: () => api.projectReport(year, month)
  });
}

function invalidateEntries(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: ["entries"] });
  queryClient.invalidateQueries({ queryKey: ["report"] });
}

export function useCreateEntry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (input: EntryInput) => api.createEntry(input),
    onSuccess: () => invalidateEntries(qc)
  });
}

export function useUpdateEntry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (vars: { id: string; version: number; input: EntryInput }) =>
      api.updateEntry(vars.id, vars.version, vars.input),
    onSuccess: () => invalidateEntries(qc)
  });
}

export function useDeleteEntry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.deleteEntry(id),
    onSuccess: () => invalidateEntries(qc)
  });
}

export function useClosePeriod() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (vars: { year: number; month: number }) => api.closePeriod(vars.year, vars.month),
    onSuccess: () => invalidateEntries(qc)
  });
}

export function useOpenPeriod() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (vars: { year: number; month: number }) => api.openPeriod(vars.year, vars.month),
    onSuccess: () => invalidateEntries(qc)
  });
}
