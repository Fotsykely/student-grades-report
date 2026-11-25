import { apiFetch, apiFetchNoContent, jsonRequest } from "./client";
import type { Grade, GradeInput } from "./types";

// One small function per endpoint of GradesController. Pages and Server Actions call these
// and never deal with fetch or status codes.

// Without a subject id, every grade is listed.
export function listGrades(subjectId?: number): Promise<Grade[]> {
  return apiFetch(subjectId === undefined ? "/api/grades" : `/api/grades?subjectId=${subjectId}`);
}

export function getGrade(id: number): Promise<Grade> {
  return apiFetch(`/api/grades/${id}`);
}

export function createGrade(input: GradeInput): Promise<Grade> {
  return apiFetch("/api/grades", jsonRequest("POST", input));
}

// Only the value can change: the student and the subject identify the grade.
export function updateGradeValue(id: number, value: number | null): Promise<Grade> {
  return apiFetch(`/api/grades/${id}`, jsonRequest("PUT", { value }));
}

export function deleteGrade(id: number): Promise<void> {
  return apiFetchNoContent(`/api/grades/${id}`, { method: "DELETE" });
}
