import type { Metadata } from "next";
import { Suspense } from "react";
import { GradeForm } from "@/components/grade-form";
import { PageSkeleton } from "@/components/page-skeleton";
import { listStudents } from "@/lib/api-client/students";
import { listSubjects } from "@/lib/api-client/subjects";
import { createGradeAction } from "../actions";

export const metadata: Metadata = { title: "Saisir une note" };

// The heading renders instantly; the lists for the selects are read in a child under its own <Suspense>.
export default function NewGradePage() {
  return (
    <>
      <h1 className="mb-6 text-2xl font-semibold">Saisir une note</h1>
      <Suspense fallback={<PageSkeleton />}>
        <NewGradeForm />
      </Suspense>
    </>
  );
}

async function NewGradeForm() {
  const [students, subjects] = await Promise.all([listStudents(), listSubjects()]);

  return (
    <GradeForm
      action={createGradeAction}
      students={students}
      subjects={subjects}
      initialValues={{ studentId: "", subjectId: "", value: "" }}
      cancelHref="/grades"
    />
  );
}
