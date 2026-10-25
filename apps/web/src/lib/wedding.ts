import { z } from "zod";

// Mirrors WeddingRules in the API (apps/backend/.../Entities/WeddingRules.cs)
export const NAME_MAX_LENGTH = 100;
export const SLUG_MIN_LENGTH = 3;
export const SLUG_MAX_LENGTH = 60;
const SLUG_PATTERN = /^[a-z0-9]+(-[a-z0-9]+)*$/;

export const PUBLIC_HOST = "casarme.app";

export type Wedding = {
  id: string;
  partnerOneName: string;
  partnerTwoName: string;
  slug: string;
  primaryDate: string; // YYYY-MM-DD
  createdAt: string;
  updatedAt: string;
};

export const slugSchema = z
  .string()
  .trim()
  .min(SLUG_MIN_LENGTH, `Use pelo menos ${SLUG_MIN_LENGTH} caracteres`)
  .max(SLUG_MAX_LENGTH, `Use no máximo ${SLUG_MAX_LENGTH} caracteres`)
  .regex(SLUG_PATTERN, "Use só letras minúsculas, números e hífens");

const nameSchema = z
  .string()
  .trim()
  .min(1, "Informe o nome")
  .max(NAME_MAX_LENGTH, `Use no máximo ${NAME_MAX_LENGTH} caracteres`);

/** Shape sent to the API. The "future date" rule is left to the form and the API. */
export const weddingInputSchema = z.object({
  partnerOneName: nameSchema,
  partnerTwoName: nameSchema,
  slug: slugSchema,
  primaryDate: z.iso.date("Informe a data do casamento"),
});

export type WeddingInput = z.infer<typeof weddingInputSchema>;

export const weddingFormSchema = weddingInputSchema.extend({
  primaryDate: z.iso
    .date("Informe a data do casamento")
    .refine((date) => date > todayIso(), "A data precisa ser no futuro"),
});

/** Today's date in the browser's timezone, as YYYY-MM-DD. */
export function todayIso() {
  const now = new Date();
  const month = String(now.getMonth() + 1).padStart(2, "0");
  const day = String(now.getDate()).padStart(2, "0");
  return `${now.getFullYear()}-${month}-${day}`;
}

function slugifyPart(value: string) {
  return value
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "");
}

/** "Ana Júlia" + "João" → "ana-julia-e-joao" */
export function suggestSlug(partnerOneName: string, partnerTwoName: string) {
  const parts = [partnerOneName, partnerTwoName]
    .map(slugifyPart)
    .filter(Boolean);
  return parts.join("-e-").slice(0, SLUG_MAX_LENGTH).replace(/-+$/, "");
}

/** "2027-05-20" → "20 de maio de 2027", without timezone shifts. */
export function formatWeddingDate(date: string) {
  const [year = 0, month = 1, day = 1] = date.split("-").map(Number);
  return new Date(year, month - 1, day).toLocaleDateString("pt-BR", {
    dateStyle: "long",
  });
}
