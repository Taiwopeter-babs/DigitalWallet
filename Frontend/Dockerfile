# syntax=docker/dockerfile:1


ARG NODE_VERSION=24.22.1-slim


FROM node:${NODE_VERSION} AS dependencies

WORKDIR /app

COPY package.json package-lock.json* ./


RUN --mount=type=cache,target=/root/.npm \
    if [ -f package-lock.json ]; then \
    npm ci --no-audit --no-fund; \
    else \
    echo "No lockfile found" && exit 1; \
    fi



FROM node:${NODE_VERSION} AS builder

WORKDIR /app

COPY --from=dependencies /app/node_modules ./node_modules

COPY . .

RUN mkdir -p /app/public

ENV NODE_ENV=production

ARG NEXT_PUBLIC_API_URL

ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL

RUN if [ -f package-lock.json ]; then \
    npm run build; \
    else \
    echo "No lockfile found." && exit 1; \
    fi



FROM node:${NODE_VERSION} AS runner

WORKDIR /app

ENV NODE_ENV=production
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

# copy production assets
COPY --from=builder --chown=node:node /app/public ./public

RUN mkdir -p .next
RUN chown node:node .next

COPY --from=builder --chown=node:node /app/.next/standalone ./
COPY --from=builder --chown=node:node /app/.next/static ./.next/static


# Run the application as a non-root user.
USER node


# Expose the port that the application listens on.
EXPOSE 3000

# Run the application.
# CMD ["npm", "run", "start"]
CMD ["node", "server.js"]
