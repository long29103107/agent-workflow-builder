FROM node:25-alpine
WORKDIR /app
COPY src/agent-workflow-ui/package*.json ./
RUN npm install
COPY src/agent-workflow-ui/ ./
EXPOSE 5173
CMD ["npm", "run", "dev"]
