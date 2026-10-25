import { index, snakeCase } from "drizzle-orm/pg-core";

export const authSchema = snakeCase.schema("auth");

export const users = authSchema.table("users", (t) => ({
  id: t.text().primaryKey(),
  name: t.text().notNull(),
  email: t.text().unique().notNull(),
  emailVerified: t.boolean().default(false).notNull(),
  image: t.text(),
  createdAt: t.timestamp({ withTimezone: true }).notNull(),
  updatedAt: t
    .timestamp({ withTimezone: true })
    .$onUpdate(() => new Date())
    .notNull(),
}));

export const jwks = authSchema.table("jwks", (t) => ({
  id: t.text().primaryKey(),
  publicKey: t.text().notNull(),
  privateKey: t.text().notNull(),
  alg: t.text(),
  crv: t.text(),
  createdAt: t.timestamp({ precision: 6, withTimezone: true }).notNull(),
  expiresAt: t.timestamp({ precision: 6, withTimezone: true }),
}));

export const sessions = authSchema.table(
  "sessions",
  (t) => ({
    id: t.text().primaryKey(),
    userId: t
      .text()
      .references(() => users.id, { onDelete: "cascade" })
      .notNull(),
    expiresAt: t.timestamp({ withTimezone: true }).notNull(),
    token: t.text().unique().notNull(),
    ipAddress: t.text(),
    userAgent: t.text(),
    createdAt: t.timestamp({ withTimezone: true }).notNull(),
    updatedAt: t
      .timestamp({ withTimezone: true })
      .$onUpdate(() => new Date())
      .notNull(),
  }),
  (table) => [index("session_userId_idx").on(table.userId)],
);

export const accounts = authSchema.table(
  "accounts",
  (t) => ({
    id: t.text().primaryKey(),
    userId: t
      .text()
      .notNull()
      .references(() => users.id, { onDelete: "cascade" }),
    accountId: t.text().notNull(),
    providerId: t.text().notNull(),
    accessToken: t.text(),
    refreshToken: t.text(),
    idToken: t.text(),
    accessTokenExpiresAt: t.timestamp({ withTimezone: true }),
    refreshTokenExpiresAt: t.timestamp({ withTimezone: true }),
    scope: t.text(),
    password: t.text(),
    createdAt: t.timestamp({ withTimezone: true }).notNull(),
    updatedAt: t
      .timestamp({ withTimezone: true })
      .$onUpdate(() => new Date())
      .notNull(),
  }),
  (table) => [index("account_userId_idx").on(table.userId)],
);

export const verifications = authSchema.table(
  "verifications",
  (t) => ({
    id: t.text().primaryKey(),
    identifier: t.text().notNull(),
    value: t.text().notNull(),
    expiresAt: t.timestamp({ withTimezone: true }).notNull(),
    createdAt: t.timestamp({ withTimezone: true }).notNull(),
    updatedAt: t
      .timestamp({ withTimezone: true })
      .$onUpdate(() => new Date())
      .notNull(),
  }),
  (table) => [index("verification_identifier_idx").on(table.identifier)],
);
