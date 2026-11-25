// Shown while a page loads its data. The shapes roughly match a table, so the page does not jump
// when the content arrives.
export function PageSkeleton() {
  return (
    <div aria-busy="true" aria-live="polite">
      <span className="sr-only">Chargement…</span>
      <div aria-hidden className="flex flex-col gap-3">
        <div className="h-11 rounded-md bg-border" />
        <div className="h-11 rounded-md bg-border" />
        <div className="h-11 rounded-md bg-border" />
        <div className="h-11 rounded-md bg-border" />
      </div>
    </div>
  );
}
