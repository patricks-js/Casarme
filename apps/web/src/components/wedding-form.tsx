import { IconCircleCheck, IconLoader2 } from "@tabler/icons-react";
import { useForm } from "@tanstack/react-form";
import { useRef, useState } from "react";
import { toast } from "sonner";

import { Button } from "#components/ui/button.tsx";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "#components/ui/field.tsx";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
  InputGroupText,
} from "#components/ui/input-group.tsx";
import { Input } from "#components/ui/input.tsx";
import {
  checkSlugAvailability,
  type WeddingField,
  type WeddingMutationResult,
} from "#functions/wedding.functions.ts";
import {
  PUBLIC_HOST,
  slugSchema,
  suggestSlug,
  todayIso,
  weddingFormSchema,
  type WeddingInput,
} from "#lib/wedding.ts";

const SLUG_TAKEN = "Esse link já está em uso. Escolha outro.";

// shown when the API rejects a field the form already validated (e.g. timezone edge cases)
const API_FIELD_ERRORS: Record<WeddingField, string> = {
  partnerOneName: "Informe um nome válido",
  partnerTwoName: "Informe um nome válido",
  slug: "Use só letras minúsculas, números e hífens (3 a 60 caracteres)",
  primaryDate: "A data precisa ser no futuro",
};

type WeddingFormProps = {
  defaultValues?: WeddingInput;
  /** Keep the slug in sync with the names until the user edits it. */
  suggestSlugFromNames?: boolean;
  submitLabel: string;
  submittingLabel: string;
  onSubmit: (values: WeddingInput) => Promise<WeddingMutationResult>;
  onSuccess: (result: Extract<WeddingMutationResult, { ok: true }>) => unknown;
};

const PARTNER_FIELDS = [
  {
    name: "partnerOneName",
    label: "Nome da primeira pessoa",
    placeholder: "Ana",
  },
  {
    name: "partnerTwoName",
    label: "Nome da segunda pessoa",
    placeholder: "João",
  },
] as const;

const emptyValues: WeddingInput = {
  partnerOneName: "",
  partnerTwoName: "",
  slug: "",
  primaryDate: "",
};

export function WeddingForm({
  defaultValues = emptyValues,
  suggestSlugFromNames = false,
  submitLabel,
  submittingLabel,
  onSubmit,
  onSuccess,
}: WeddingFormProps) {
  const slugEdited = useRef(!suggestSlugFromNames);
  const [apiErrors, setApiErrors] = useState<
    Partial<Record<WeddingField, string>>
  >({});

  const clearApiError = (field: WeddingField) =>
    setApiErrors(({ [field]: _, ...rest }) => rest);

  const form = useForm({
    defaultValues,
    validators: { onSubmit: weddingFormSchema },
    onSubmit: async ({ value }) => {
      const result = await onSubmit(weddingFormSchema.parse(value));

      if (result.ok) {
        await onSuccess(result);
        return;
      }

      switch (result.error) {
        case "slug-taken":
          setApiErrors({ slug: SLUG_TAKEN });
          toast.error(SLUG_TAKEN);
          break;
        case "already-has-wedding":
          toast.error("Você já tem um casamento cadastrado.");
          break;
        case "validation":
          if (result.field) {
            setApiErrors({ [result.field]: API_FIELD_ERRORS[result.field] });
          }
          toast.error("Confira os dados do formulário.");
          break;
        default:
          toast.error("Não foi possível salvar. Tente novamente.");
      }
    },
  });

  const syncSuggestedSlug = () => {
    if (slugEdited.current) {
      return;
    }
    form.setFieldValue(
      "slug",
      suggestSlug(
        form.getFieldValue("partnerOneName"),
        form.getFieldValue("partnerTwoName"),
      ),
    );
  };

  return (
    <form
      noValidate
      onSubmit={(e) => {
        e.preventDefault();
        form.handleSubmit();
      }}
    >
      <FieldGroup>
        {PARTNER_FIELDS.map(({ name, label, placeholder }) => (
          <form.Field
            key={name}
            name={name}
            listeners={{ onChange: syncSuggestedSlug }}
            children={(field) => {
              const apiError = apiErrors[name];
              const isInvalid =
                (field.state.meta.isTouched && !field.state.meta.isValid) ||
                !!apiError;
              return (
                <Field data-invalid={isInvalid}>
                  <FieldLabel htmlFor={field.name}>{label}</FieldLabel>
                  <Input
                    id={field.name}
                    name={field.name}
                    autoComplete="off"
                    placeholder={placeholder}
                    value={field.state.value}
                    onBlur={field.handleBlur}
                    onChange={(e) => {
                      clearApiError(name);
                      field.handleChange(e.target.value);
                    }}
                    aria-invalid={isInvalid}
                  />
                  {apiError ? (
                    <FieldError>{apiError}</FieldError>
                  ) : (
                    isInvalid && <FieldError errors={field.state.meta.errors} />
                  )}
                </Field>
              );
            }}
          />
        ))}

        <form.Field
          name="primaryDate"
          children={(field) => {
            const apiError = apiErrors.primaryDate;
            const isInvalid =
              (field.state.meta.isTouched && !field.state.meta.isValid) ||
              !!apiError;
            return (
              <Field data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Data do casamento</FieldLabel>
                <Input
                  id={field.name}
                  name={field.name}
                  type="date"
                  min={todayIso()}
                  value={field.state.value}
                  onBlur={field.handleBlur}
                  onChange={(e) => {
                    clearApiError("primaryDate");
                    field.handleChange(e.target.value);
                  }}
                  aria-invalid={isInvalid}
                />
                {apiError ? (
                  <FieldError>{apiError}</FieldError>
                ) : (
                  isInvalid && <FieldError errors={field.state.meta.errors} />
                )}
              </Field>
            );
          }}
        />

        <form.Field
          name="slug"
          validators={{
            onChange: slugSchema,
            onChangeAsyncDebounceMs: 400,
            onChangeAsync: async ({ value }) => {
              const available = await checkSlugAvailability({
                data: { slug: value },
              });
              return available ? undefined : { message: SLUG_TAKEN };
            },
          }}
          children={(field) => {
            const { value } = field.state;
            const { isTouched, isValid, isValidating, errors } =
              field.state.meta;
            const apiError = apiErrors.slug;
            // the slug may be filled from the names, so show its state without waiting for a touch
            const isInvalid =
              ((isTouched || !!value) && !isValid) || !!apiError;
            return (
              <Field data-invalid={isInvalid}>
                <FieldLabel htmlFor={field.name}>Link do casamento</FieldLabel>
                <InputGroup>
                  <InputGroupAddon>
                    <InputGroupText>{PUBLIC_HOST}/</InputGroupText>
                  </InputGroupAddon>
                  <InputGroupInput
                    id={field.name}
                    name={field.name}
                    autoComplete="off"
                    autoCapitalize="none"
                    spellCheck={false}
                    value={value}
                    onBlur={field.handleBlur}
                    onChange={(e) => {
                      slugEdited.current = true;
                      clearApiError("slug");
                      field.handleChange(e.target.value);
                    }}
                    aria-invalid={isInvalid}
                  />
                  {value && (
                    <InputGroupAddon align="inline-end">
                      {isValidating ? (
                        <IconLoader2 className="animate-spin" />
                      ) : (
                        isValid &&
                        !apiError && (
                          <IconCircleCheck className="text-emerald-500" />
                        )
                      )}
                    </InputGroupAddon>
                  )}
                </InputGroup>
                {apiError ? (
                  <FieldError>{apiError}</FieldError>
                ) : isInvalid ? (
                  <FieldError errors={errors} />
                ) : (
                  <FieldDescription>
                    {!value
                      ? "Sugerimos um link a partir dos nomes. Você pode editá-lo."
                      : isValidating
                        ? "Verificando disponibilidade..."
                        : `Disponível: ${PUBLIC_HOST}/${value}`}
                  </FieldDescription>
                )}
              </Field>
            );
          }}
        />

        <Field>
          <form.Subscribe
            selector={(state) => state.isSubmitting}
            children={(isSubmitting) => (
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? submittingLabel : submitLabel}
              </Button>
            )}
          />
        </Field>
      </FieldGroup>
    </form>
  );
}
