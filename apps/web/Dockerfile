FROM oven/bun:1.4.4-alpine AS build
WORKDIR /repo
COPY package.json bun.lock ./
COPY apps/auth/package.json apps/auth/
COPY apps/web/package.json apps/web/
COPY apps/api/package.json apps/api/
COPY packages/api-client/package.json packages/api-client/
COPY packages/tsconfig/package.json packages/tsconfig/
COPY packages/env/package.json packages/env/
RUN bun install --frozen-lockfile --filter @casarme/web
COPY packages/tsconfig/ packages/tsconfig/
COPY packages/api-client/ packages/api-client/
COPY packages/env/ packages/env/
COPY apps/web/ apps/web/
RUN cd apps/web && bun run build

FROM oven/bun:1.4.4-alpine
WORKDIR /app
COPY --from=build /repo/apps/web/.output .output
ENV PORT=3000
EXPOSE 3000
CMD ["bun", ".output/server/index.mjs"]
