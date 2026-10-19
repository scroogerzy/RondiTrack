# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first so package restore can be cached.
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY API/API.csproj API/
RUN dotnet restore API/API.csproj

# Copy source and publish the API.
COPY Domain/ Domain/
COPY Infrastructure/ Infrastructure/
COPY API/ API/
RUN dotnet publish API/API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Use the non-root user supplied by the .NET runtime image.
USER $APP_UID

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "API.dll"]
