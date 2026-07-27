# syntax=docker/dockerfile:1

# ---------------------------------------------------------
# Stage 1: Build the React application
# ---------------------------------------------------------

FROM node:24-alpine AS build

WORKDIR /app

COPY package.json package-lock.json ./

RUN npm ci

COPY . ./

RUN npm run build

# ---------------------------------------------------------
# Stage 2: Serve the compiled application with Nginx
# ---------------------------------------------------------

FROM nginx:stable-alpine AS final

COPY nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]