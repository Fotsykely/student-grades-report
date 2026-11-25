import type { Metadata } from "next";
import { Suspense } from "react";
import { GradeForm } from "@/components/grade-form";
import { PageSkeleton } from "@/components/page-skeleton";
import { getGrade } from "@/lib/api-client/grades";
import { formatGrade } from "@/lib/format";
import { orNotFound, parseRouteId } from "@/lib/route-helpers";
import { updateGradeAction } from "../../actions";

export const metadata: Metadata = { title: "Modifier la note" };

type EditGradePageProps = PageProps<"/grades/[gradeId]/edit">;

// The page itself renders instantly (static heading). Reading `params` and calling the API happen in a
// child inside its own <Suspense>: with Cache Components, a boundary above the shared layout (the root
// loading.tsx) does not give an instant fallback on a client navigation to this route.
export default function EditGradePage(props: EditGradePageProps) {
  return (
    <>
      <h1 className="mb-6 text-2xl font-semibold">Modifier la note</h1>
      <Suspense fallback={<PageSkeleton />}>
        <EditGradeForm params={props.params} />
      </Suspense>
    </>
  );
}

async function EditGradeForm({ params }: { params: EditGradePageProps["params"] }) {
  const { gradeId } = await params;
  const id = parseRouteId(gradeId);
  const grade = await orNotFound(getGrade(id));

  return (
    <GradeForm
      action={updateGradeAction.bind(null, grade.id)}
      locked={{ studentName: grade.studentName, subjectName: grade.subjectName }}
      initialValues={{ studentId: "", subjectId: "", value: formatGrade(grade.value) }}
      cancelHref="/grades"
    />
  );
}
