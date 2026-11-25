import { Suspense } from "react";
import { PageSkeleton } from "@/components/page-skeleton";
import { getReport } from "@/lib/api-client/report";
import { ReportTable } from "./_components/report-table";

export default function ReportPage() {
  return (
    <>
      <h1 className="mb-2 text-2xl font-semibold">Bulletin</h1>
      <p className="mb-6 text-muted-foreground">
        Moyenne pondérée par les coefficients. Un étudiant à qui il manque une note est « Incomplet » : il n&apos;a ni
        moyenne ni rang.
      </p>
      <Suspense fallback={<PageSkeleton />}>
        <Report />
      </Suspense>
    </>
  );
}

async function Report() {
  return <ReportTable report={await getReport()} />;
}
