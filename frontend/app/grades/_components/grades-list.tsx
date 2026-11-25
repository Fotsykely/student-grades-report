import Link from "next/link";
import { Pencil } from "lucide-react";
import { Button, ButtonLink } from "@/components/button";
import { ConfirmDelete } from "@/components/confirm-delete";
import { listGrades } from "@/lib/api-client/grades";
import { listSubjects } from "@/lib/api-client/subjects";
import { formatGrade } from "@/lib/format";
import { deleteGradeAction } from "../actions";
import { GradeValue } from "@/components/grade-value";

// "?subjectId=2" filters the list. Anything else (absent, empty, not a number) means "all subjects".
function parseSubjectFilter(raw: string | string[] | undefined): number | undefined {
  return typeof raw === "string" && /^\d+$/.test(raw) ? Number(raw) : undefined;
}

type GradesListProps = {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
};

export async function GradesList({ searchParams }: GradesListProps) {
  const { subjectId: rawSubjectId } = await searchParams;
  const subjectId = parseSubjectFilter(rawSubjectId);

  const [subjects, grades] = await Promise.all([listSubjects(), listGrades(subjectId)]);

  return (
    <>
      {/* A plain GET form: the filter works without JavaScript and the URL can be shared. */}
      <form method="get" action="/grades" className="mb-6 flex flex-wrap items-end gap-3">
        <div className="flex flex-col gap-1.5">
          <label htmlFor="subjectId" className="text-sm font-semibold">
            Matière
          </label>
          <select
            id="subjectId"
            name="subjectId"
            defaultValue={subjectId ?? ""}
            className="min-h-11 rounded-md border border-border-input bg-surface px-3 py-2 text-base"
          >
            <option value="">Toutes les matières</option>
            {subjects.map((subject) => (
              <option key={subject.id} value={subject.id}>
                {subject.name}
              </option>
            ))}
          </select>
        </div>
        <Button type="submit" variant="secondary">
          Filtrer
        </Button>
        {subjectId !== undefined && (
          <Link href="/grades" className="inline-flex min-h-11 items-center text-sm font-semibold underline">
            Réinitialiser
          </Link>
        )}
      </form>

      {grades.length === 0 ? (
        <p className="rounded-lg border border-dashed border-border-input p-6 text-center text-muted-foreground">
          {subjectId === undefined
            ? "Aucune note pour le moment. Saisissez la première avec le bouton « Saisir une note »."
            : "Aucune note dans cette matière pour le moment."}
        </p>
      ) : (
        <div className="overflow-x-auto rounded-lg border border-border bg-surface shadow-sm">
          <table className="w-full text-left">
            <caption className="sr-only">Liste des notes</caption>
            <thead className="border-b border-border bg-background text-sm">
              <tr>
                <th scope="col" className="px-4 py-3 font-semibold">Étudiant</th>
                <th scope="col" className="px-4 py-3 font-semibold">Matière</th>
                <th scope="col" className="px-4 py-3 text-right font-semibold">Note /20</th>
                <th scope="col" className="px-4 py-3 text-right font-semibold">Actions</th>
              </tr>
            </thead>
            <tbody>
              {grades.map((grade) => (
                <tr key={grade.id} className="border-b border-border last:border-b-0">
                  <th scope="row" className="px-4 py-2 font-medium">{grade.studentName}</th>
                  <td className="px-4 py-2">{grade.subjectName}</td>
                  <td className="px-4 py-2 text-right font-semibold">
                    <GradeValue value={grade.value} />
                  </td>
                  <td className="px-4 py-2">
                    <div className="flex justify-end gap-2">
                      <ButtonLink
                        href={`/grades/${grade.id}/edit`}
                        variant="secondary"
                        aria-label={`Modifier la note de ${grade.studentName} en ${grade.subjectName}`}
                      >
                        <Pencil aria-hidden className="size-4" />
                      </ButtonLink>
                      <ConfirmDelete
                        iconOnly
                        triggerLabel={`Supprimer la note de ${grade.studentName} en ${grade.subjectName}`}
                        title="Supprimer cette note ?"
                        description={`La note de ${grade.studentName} en ${grade.subjectName} (${formatGrade(grade.value)}/20) sera supprimée définitivement.`}
                        confirmLabel="Supprimer"
                        action={deleteGradeAction.bind(null, grade.id)}
                      />
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
