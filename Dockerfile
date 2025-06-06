# Global Arguments
ARG DCR_URL=mcr.microsoft.com
ARG BUILD_IMAGE=${DCR_URL}/dotnet/sdk:8.0-alpine
ARG RUNTIME_IMAGE=${DCR_URL}/dotnet/aspnet:8.0-alpine

# Build Container
FROM ${BUILD_IMAGE} AS builder
WORKDIR /app

COPY . .

RUN dotnet publish ./OCBC.HeadlessCMS/OCBC.HeadlessCMS.csproj -c Release -o /app/src/out

# Runtime Container
FROM ${RUNTIME_IMAGE}

# Install cultures
RUN apk add --no-cache \
   icu-data-full \
   icu-libs

WORKDIR /app

COPY --from=builder /app/src/out .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "OCBC.HeadlessCMS.dll"]
