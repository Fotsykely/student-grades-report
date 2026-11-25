import { apiFetch } from "./client";
import type { Subject } from "./types";

export function listSubjects(): Promise<Subject[]> {
  return apiFetch("/api/subjects");
}
