FROM oven/bun:1.3.12-alpine
WORKDIR /app
COPY src/agent-workflow-ui/package.json src/agent-workflow-ui/bun.lock* ./
RUN bun install --frozen-lockfile
COPY src/agent-workflow-ui/ ./
EXPOSE 5173
CMD ["bun", "run", "dev"]
