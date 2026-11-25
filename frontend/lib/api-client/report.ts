import { apiFetch } from "./client";
import type { Report } from "./types";

export function getReport(): Promise<Report> {
  return apiFetch("/api/report");
}
