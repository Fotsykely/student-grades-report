import { revalidatePath } from "next/cache";

// A grade change touches several pages: the report card, the class averages and the grade list.
// Refreshing everything is simpler than tracking each path.
export function revalidateAllPages(): void {
  revalidatePath("/", "layout");
}
