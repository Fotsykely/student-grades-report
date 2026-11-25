import { formatAverage, formatGradeCount, formatRank, MENTION_LABELS } from "@/lib/format";
import type { Report, ReportRow } from "@/lib/api-client/types";
import { GradeValue, PASS_MARK } from "@/components/grade-value";

const CELL = "px-4 py-3";
const NUMBER_CELL = `${CELL} text-right`;

// The report card: one row per student, one column per subject, then average, mention and rank.
export function ReportTable({ report }: { report: Report }) {
  const classAverageBySubject = new Map(report.classAverages.map((a) => [a.subjectId, a]));

  // A rank shared by several students is displayed "ex æquo".
  const studentsPerRank = new Map<number, number>();
  for (const row of report.rows) {
    if (row.rank !== null) studentsPerRank.set(row.rank, (studentsPerRank.get(row.rank) ?? 0) + 1);
  }

  return (
    <div className="space-y-2">
      <div className="overflow-x-auto rounded-lg border border-border bg-surface shadow-sm">
        <table className="w-full text-left">
          <caption className="sr-only">
            Bulletin de la classe : notes par matière, moyenne pondérée, mention et rang de chaque étudiant
          </caption>
          <thead className="border-b border-border bg-background text-sm">
            <tr>
              <th scope="col" className={`${CELL} font-semibold`}>Étudiant</th>
              {report.subjects.map((subject) => (
                <th key={subject.id} scope="col" className={`${NUMBER_CELL} font-semibold`}>
                  {subject.name}
                  <span className="block font-normal text-muted-foreground">coef. {subject.coefficient}</span>
                </th>
              ))}
              <th scope="col" className={`${NUMBER_CELL} font-semibold`}>Moyenne</th>
              <th scope="col" className={`${CELL} font-semibold`}>Mention</th>
              <th scope="col" className={`${NUMBER_CELL} font-semibold`}>Rang</th>
            </tr>
          </thead>

          <tbody>
            {report.rows.map((row) => (
              <StudentRow key={row.studentId} row={row} tied={row.rank !== null && (studentsPerRank.get(row.rank) ?? 0) > 1} />
            ))}
          </tbody>

          <tfoot className="border-t border-border bg-primary-subtle">
            <tr>
              <th scope="row" className={`${CELL} font-semibold`}>Moyenne de la classe</th>
              {report.subjects.map((subject) => {
                const classAverage = classAverageBySubject.get(subject.id);
                return (
                  <td key={subject.id} className={NUMBER_CELL}>
                    {classAverage?.average == null ? (
                      <Missing label="Aucune note" />
                    ) : (
                      <>
                        <span className="font-semibold">{formatAverage(classAverage.average)}</span>
                        <span className="block text-sm text-muted-foreground">
                          {formatGradeCount(classAverage.gradeCount)}
                        </span>
                      </>
                    )}
                  </td>
                );
              })}
              <td colSpan={3} />
            </tr>
            <tr>
              <th scope="row" className={`${CELL} font-semibold`}>Médiane de la classe</th>
              {report.subjects.map((subject) => {
                const median = classAverageBySubject.get(subject.id)?.median;
                return (
                  <td key={subject.id} className={NUMBER_CELL}>
                    {median == null ? <Missing label="Aucune note" /> : <span className="font-semibold">{formatAverage(median)}</span>}
                  </td>
                );
              })}
              <td colSpan={3} />
            </tr>
          </tfoot>
        </table>
      </div>
      <p className="text-sm text-muted-foreground">
        <span aria-hidden className="font-semibold text-destructive">▲</span> Note inférieure à {PASS_MARK} (insuffisante)
      </p>
    </div>
  );
}

function StudentRow({ row, tied }: { row: ReportRow; tied: boolean }) {
  return (
    <tr className="border-b border-border">
      <th scope="row" className={`${CELL} font-medium`}>{row.studentName}</th>

      {row.grades.map((grade, index) => (
        <td key={index} className={NUMBER_CELL}>
          {grade === null ? <Missing label="Note manquante" /> : <GradeValue value={grade} />}
        </td>
      ))}

      {row.isComplete && row.average !== null && row.mention !== null && row.rank !== null ? (
        <>
          <td className={`${NUMBER_CELL} font-semibold`}>{formatAverage(row.average)}</td>
          <td className={CELL}>{MENTION_LABELS[row.mention]}</td>
          <td className={NUMBER_CELL}>{formatRank(row.rank, tied)}</td>
        </>
      ) : (
        <>
          {/* Written out, never colour alone: an incomplete student has no average and no rank. */}
          <td className={`${NUMBER_CELL} font-semibold text-muted-foreground`}>Incomplet</td>
          <td className={CELL}><Missing label="Pas de mention" /></td>
          <td className={NUMBER_CELL}><Missing label="Non classé" /></td>
        </>
      )}
    </tr>
  );
}

// A dash for the eye, a sentence for screen readers.
function Missing({ label }: { label: string }) {
  return (
    <>
      <span aria-hidden className="text-muted-foreground">—</span>
      <span className="sr-only">{label}</span>
    </>
  );
}
