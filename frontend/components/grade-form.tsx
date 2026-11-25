"use client";

import { useActionState } from "react";
import { ButtonLink } from "@/components/button";
import { FormError, InputField, SelectField } from "@/components/field";
import { SubmitButton } from "@/components/submit-button";
import type { Student, Subject } from "@/lib/api-client/types";
import { INITIAL_FORM_STATE, type FormState } from "@/lib/form-state";

type GradeFormValues = { studentId: string; subjectId: string; value: string };

type GradeFormProps = {
  action: (previousState: FormState, formData: FormData) => Promise<FormState>;
  initialValues: GradeFormValues;
  cancelHref: string;
} & (
  // New grade: the student and the subject are chosen in lists.
  | { students: Student[]; subjects: Subject[]; locked?: undefined }
  // Edit: they identify the grade and are only displayed.
  | { locked: { studentName: string; subjectName: string }; students?: undefined; subjects?: undefined }
);

// Shared by the "new grade" and "edit grade" pages. The API validates; this form only displays
// the messages it sends back under the matching field (noValidate turns the browser's own checks off).
export function GradeForm(props: GradeFormProps) {
  const { action, initialValues, cancelHref } = props;
  const [state, formAction] = useActionState(action, INITIAL_FORM_STATE);

  const values = state.status === "error" ? state.values : initialValues;
  const fieldErrors = state.status === "error" ? state.fieldErrors : {};

  return (
    <form action={formAction} noValidate className="flex flex-col gap-5">
      {state.status === "error" && state.message && <FormError>{state.message}</FormError>}

      {props.locked ? (
        <dl className="grid grid-cols-[auto_1fr] gap-x-4 gap-y-1 rounded-lg border border-border bg-surface p-4">
          <dt className="text-sm text-muted-foreground">Étudiant</dt>
          <dd className="font-semibold">{props.locked.studentName}</dd>
          <dt className="text-sm text-muted-foreground">Matière</dt>
          <dd className="font-semibold">{props.locked.subjectName}</dd>
        </dl>
      ) : (
        <>
          <SelectField
            label="Étudiant"
            name="studentId"
            defaultValue={values.studentId}
            errors={fieldErrors.studentId}
          >
            <option value="">Choisir un étudiant</option>
            {props.students.map((student) => (
              <option key={student.id} value={student.id}>
                {student.name}
              </option>
            ))}
          </SelectField>
          <SelectField
            label="Matière"
            name="subjectId"
            defaultValue={values.subjectId}
            errors={fieldErrors.subjectId}
          >
            <option value="">Choisir une matière</option>
            {props.subjects.map((subject) => (
              <option key={subject.id} value={subject.id}>
                {subject.name} (coef. {subject.coefficient})
              </option>
            ))}
          </SelectField>
        </>
      )}

      <InputField
        label="Note sur 20"
        name="value"
        inputMode="decimal"
        autoComplete="off"
        hint="Les décimales sont autorisées, par exemple 12,5."
        defaultValue={values.value}
        errors={fieldErrors.value}
      />

      <div className="flex flex-wrap gap-3">
        <SubmitButton>Enregistrer</SubmitButton>
        <ButtonLink href={cancelHref} variant="secondary">
          Annuler
        </ButtonLink>
      </div>
    </form>
  );
}
