// These types mirror the DTOs of the ASP.NET Core API (backend/Api/Dtos).
// When a DTO changes, update the matching type here.

export interface Student {
  id: number;
  name: string;
}

export interface Subject {
  id: number;
  name: string;
  coefficient: number;
}

export interface Grade {
  id: number;
  studentId: number;
  studentName: string;
  subjectId: number;
  subjectName: string;
  coefficient: number;
  value: number;
}

// Request body of POST /api/grades. A null is sent when a field is left empty: the API then answers
// with its own "required" message under the matching field.
export type GradeInput = {
  studentId: number | null;
  subjectId: number | null;
  value: number | null;
};

export const MENTIONS = ["Ajourne", "Passable", "AssezBien", "Bien", "TresBien", "Excellent"] as const;
export type Mention = (typeof MENTIONS)[number];

// `grades` follows the order of `Report.subjects`; null means the grade is missing.
// average, mention and rank are all null when the student is incomplete.
export interface ReportRow {
  studentId: number;
  studentName: string;
  grades: (number | null)[];
  average: number | null;
  mention: Mention | null;
  rank: number | null;
  isComplete: boolean;
}

export interface ClassAverage {
  subjectId: number;
  average: number | null;
  median: number | null;
  gradeCount: number;
}

export interface Report {
  subjects: Subject[];
  rows: ReportRow[];
  classAverages: ClassAverage[];
}

// Validation messages per form field, with camelCase field names: { value: ["La note est obligatoire."] }
export type FieldErrors = Record<string, string[]>;
