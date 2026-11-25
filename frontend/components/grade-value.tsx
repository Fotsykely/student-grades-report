import { formatGrade } from "@/lib/format";

// Below this mark a grade is flagged as insufficient.
export const PASS_MARK = 10;

// A grade as text. A failing one also gets a warning icon (visible) and "Note insuffisante" (screen readers),
// so the information never rests on the red colour alone.
export function GradeValue({ value }: { value: number }) {
  if (value >= PASS_MARK) return <>{formatGrade(value)}</>;

  return (
    <span
      className="inline-flex items-center justify-end gap-1 font-semibold text-destructive"
      title={`Note insuffisante (inférieure à ${PASS_MARK})`}
    >
      <svg aria-hidden viewBox="0 0 20 20" className="h-4 w-4 shrink-0" fill="currentColor">
        <path d="M10 2 1 18h18L10 2Zm-1 6h2v5H9V8Zm0 6h2v2H9v-2Z" />
      </svg>
      <span className="sr-only">Note insuffisante : </span>
      {formatGrade(value)}
    </span>
  );
}
