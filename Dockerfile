# ── Stage 1: Base Runtime ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# ── Stage 2: Build & Publish ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies first (for layer caching)
COPY ["myMotionApi.csproj", "./"]
RUN dotnet restore "./myMotionApi.csproj"

# Copy the rest of the source code
COPY . .
RUN dotnet build "myMotionApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "myMotionApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ── Stage 3: Final Production Image ───────────────────────────────────────
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# File .env can be passed via Docker environment variables or mounted volume
ENTRYPOINT ["dotnet", "myMotionApi.dll"]
