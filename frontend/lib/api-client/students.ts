import { apiFetch } from "./client";
import type { Student } from "./types";

export function listStudents(): Promise<Student[]> {
  return apiFetch("/api/students");
}
