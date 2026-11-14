import { defineConfig } from "@hey-api/openapi-ts";

export default defineConfig({
  input: "../../apps/api/openapi.json",
  output: "src/generated",
  plugins: [
    "@hey-api/typescript",
    // The .NET api serializes DateTimeOffset with an offset (e.g. +00:00).
    { name: "zod", dates: { offset: true } },
    { name: "@hey-api/sdk", validator: true },
    "@hey-api/client-ky",
    "@tanstack/react-query",
  ],
});
