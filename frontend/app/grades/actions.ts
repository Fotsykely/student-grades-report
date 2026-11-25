"use server";

import { redirect } from "next/navigation";
import { ApiNotFoundError } from "@/lib/api-client/client";
import { createGrade, deleteGrade, updateGradeValue } from "@/lib/api-client/grades";
import { parseDecimal, parseId, readText } from "@/lib/form-data";
import { toFormState } from "@/lib/form-errors";
import type { FormState, FormValues } from "@/lib/form-state";
import { revalidateAllPages } from "@/lib/revalidate";

// Form field names, used to keep only the API messages that belong to each form.
const CREATE_FIELDS = ["studentId", "subjectId", "value"] as const;
const UPDATE_FIELDS = ["value"] as const;

const NOT_A_NUMBER = "La note doit être un nombre, par exemple 12,5.";

// These actions stay thin: read the form, call the API client, then redirect or return the errors.
// redirect() works by throwing, so it is always called outside the try/catch blocks.

export async function createGradeAction(_previousState: FormState, formData: FormData): Promise<FormState> {
  const values: FormValues = {
    studentId: readText(formData, "studentId"),
    subjectId: readText(formData, "subjectId"),
    value: readText(formData, "value"),
  };
  const value = parseDecimal(values.value);
  if (Number.isNaN(value)) return notANumber(values);

  try {
    await createGrade({ studentId: parseId(values.studentId), subjectId: parseId(values.subjectId), value });
  } catch (error) {
    return toFormState(error, CREATE_FIELDS, values);
  }

  revalidateAllPages();
  redirect("/grades");
}

// Bound to the grade id by the edit page: updateGradeAction.bind(null, grade.id).
export async function updateGradeAction(
  gradeId: number,
  _previousState: FormState,
  formData: FormData,
): Promise<FormState> {
  const values: FormValues = { value: readText(formData, "value") };
  const value = parseDecimal(values.value);
  if (Number.isNaN(value)) return notANumber(values);

  try {
    await updateGradeValue(gradeId, value);
  } catch (error) {
    return toFormState(error, UPDATE_FIELDS, values);
  }

  revalidateAllPages();
  redirect("/grades");
}

export async function deleteGradeAction(gradeId: number): Promise<void> {
  try {
    await deleteGrade(gradeId);
  } catch (error) {
    // Already deleted (for example in another tab): the goal is reached, so carry on.
    if (!(error instanceof ApiNotFoundError)) throw error;
  }

  revalidateAllPages();
  redirect("/grades");
}

// Text that is not a number cannot be sent as JSON: answer here with a message under the field.
function notANumber(values: FormValues): FormState {
  return { status: "error", message: null, fieldErrors: { value: [NOT_A_NUMBER] }, values };
}
