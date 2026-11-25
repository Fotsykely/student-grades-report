import type { Mention } from "./api-client/types";

// Texts shown to users stay in French; identifiers and comments stay in English.
export const MENTION_LABELS: Record<Mention, string> = {
  Ajourne: "Ajourné",
  Passable: "Passable",
  AssezBien: "Assez bien",
  Bien: "Bien",
  TresBien: "Très bien",
};

// A grade as typed: up to 2 decimals, none added ("12,5", "14").
const gradeFormatter = new Intl.NumberFormat("fr-FR", { maximumFractionDigits: 2 });

// An average always shows 2 decimals so that columns line up ("12,50").
const averageFormatter = new Intl.NumberFormat("fr-FR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

export function formatGrade(value: number): string {
  return gradeFormatter.format(value);
}

export function formatAverage(value: number): string {
  return averageFormatter.format(value);
}

// "1er", "2e", "2e ex æquo" (plain "e" rather than a superscript, which screen readers read badly).
export function formatRank(rank: number, tied: boolean): string {
  const ordinal = rank === 1 ? "1er" : `${rank}e`;
  return tied ? `${ordinal} ex æquo` : ordinal;
}

export function formatGradeCount(count: number): string {
  return count === 1 ? "1 note" : `${count} notes`;
}
