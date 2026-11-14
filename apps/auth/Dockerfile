FROM oven/bun:1.4.4-alpine
WORKDIR /repo
COPY package.json bun.lock ./
COPY apps/auth/package.json apps/auth/
COPY apps/web/package.json apps/web/
COPY apps/api/package.json apps/api/
COPY packages/api-client/package.json packages/api-client/
COPY packages/tsconfig/package.json packages/tsconfig/
COPY packages/env/package.json packages/env/
RUN bun install --frozen-lockfile --production --filter @casarme/auth
COPY packages/env/ packages/env/
COPY apps/auth/ apps/auth/
WORKDIR /repo/apps/auth
ENV PORT=3001
EXPOSE 3001
CMD ["bun", "src/server.ts"]
