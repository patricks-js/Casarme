import { defineConfig } from "@hey-api/openapi-ts";

export default defineConfig({
  input: "../../apps/api/openapi.json",
  output: "src/generated",
  plugins: [
    "@hey-api/typescript",
    // Event times are local to the Casamento; operational timestamps carry offsets.
    { name: "zod", dates: { offset: true, local: true } },
    { name: "@hey-api/sdk", validator: true },
    "@hey-api/client-ky",
    "@tanstack/react-query",
  ],
});
