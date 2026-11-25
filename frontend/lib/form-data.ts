// FormData values are a string or a File; our forms only have text fields.
export function readText(formData: FormData, field: string): string {
  const value = formData.get(field);
  return typeof value === "string" ? value : "";
}

// Reads a decimal typed by a French teacher: "12,5" and "12.5" are both fine.
// Returns null for an empty field, NaN for text that is not a number ("abc", "12,5,3").
export function parseDecimal(text: string): number | null {
  const trimmed = text.trim();
  if (trimmed === "") return null;
  if (!/^-?\d+(?:[.,]\d+)?$/.test(trimmed)) return Number.NaN;
  return Number(trimmed.replace(",", "."));
}

// A select value: "" means nothing chosen.
export function parseId(text: string): number | null {
  return /^\d+$/.test(text) ? Number(text) : null;
}
