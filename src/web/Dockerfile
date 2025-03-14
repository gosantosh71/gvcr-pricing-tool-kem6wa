# Build stage - compiles the Blazor WebAssembly application
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src

# Install Node.js and npm for frontend asset building
RUN apk add --no-cache nodejs npm

# Copy csproj file first for layer caching of package restore
COPY VatFilingPricingTool.Web/*.csproj ./VatFilingPricingTool.Web/
WORKDIR /src/VatFilingPricingTool.Web
RUN dotnet restore

# Copy the rest of the source code
COPY VatFilingPricingTool.Web/. ./
COPY package.json package-lock.json ./
RUN npm ci

# Build and publish the application
ARG ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
RUN dotnet publish -c Release -o /app

# Final stage - serves the compiled application using Nginx
FROM nginx:alpine
COPY --from=build /app/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
ENTRYPOINT ["nginx", "-g", "daemon off;"]