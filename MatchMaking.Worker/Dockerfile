# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the entire project contents
COPY . .

# Restore dependencies
RUN dotnet restore

# Publish the app
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Optional: expose port if your app listens on one
EXPOSE 80

# Entry point
ENTRYPOINT ["dotnet", "MatchMaking.Worker.dll"]
