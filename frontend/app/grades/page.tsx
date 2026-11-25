import type { Metadata } from "next";
import { Plus } from "lucide-react";
import { Suspense } from "react";
import { ButtonLink } from "@/components/button";
import { PageSkeleton } from "@/components/page-skeleton";
import { GradesList } from "./_components/grades-list";

export const metadata: Metadata = { title: "Notes" };

// `searchParams` is request data: it is only read inside the child, under <Suspense>.
export default function GradesPage(props: PageProps<"/grades">) {
  return (
    <>
      <header className="mb-6 flex items-center justify-between gap-4">
        <h1 className="text-2xl font-semibold">Notes</h1>
        <ButtonLink href="/grades/new">
          <Plus aria-hidden className="size-4" />
          Saisir une note
        </ButtonLink>
      </header>

      <Suspense fallback={<PageSkeleton />}>
        <GradesList searchParams={props.searchParams} />
      </Suspense>
    </>
  );
}
