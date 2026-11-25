import { PageSkeleton } from "@/components/page-skeleton";

// Root fallback while a first-level page loads its data.
export default function Loading() {
  return (
    <>
      <div aria-hidden className="mb-6 h-8 w-1/2 rounded-md bg-border" />
      <PageSkeleton />
    </>
  );
}
