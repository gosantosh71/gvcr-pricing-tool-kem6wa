# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage layer caching
COPY ["VatFilingPricingTool.sln", "./"]
COPY ["VatFilingPricingTool.Common/*.csproj", "VatFilingPricingTool.Common/"]
COPY ["VatFilingPricingTool.Contracts/*.csproj", "VatFilingPricingTool.Contracts/"]
COPY ["VatFilingPricingTool.Domain/*.csproj", "VatFilingPricingTool.Domain/"]
COPY ["VatFilingPricingTool.Data/*.csproj", "VatFilingPricingTool.Data/"]
COPY ["VatFilingPricingTool.Service/*.csproj", "VatFilingPricingTool.Service/"]
COPY ["VatFilingPricingTool.Infrastructure/*.csproj", "VatFilingPricingTool.Infrastructure/"]
COPY ["VatFilingPricingTool.Api/*.csproj", "VatFilingPricingTool.Api/"]

# Restore packages
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build and publish the application
RUN dotnet build -c Release --no-restore
RUN dotnet publish "VatFilingPricingTool.Api/VatFilingPricingTool.Api.csproj" -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS runtime
WORKDIR /app

# Create a non-root user
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -D appuser

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:80 \
    TZ=UTC

# Create directory for logs
RUN mkdir -p /app/logs && \
    chown -R appuser:appgroup /app

# Copy the published application
COPY --from=build --chown=appuser:appgroup /app/publish .

# Expose the HTTP port
EXPOSE 80

# Set the user to run the application
USER appuser

# Configure health check
HEALTHCHECK --interval=30s --timeout=5s --retries=3 --start-period=15s \
    CMD curl -f http://localhost:80/health/live || exit 1

# Set labels
LABEL maintainer="VAT Filing Pricing Tool Team" \
      description="VAT Filing Pricing Tool Backend API" \
      version="1.0.0"

# Set the entry point
ENTRYPOINT ["dotnet", "VatFilingPricingTool.Api.dll"]