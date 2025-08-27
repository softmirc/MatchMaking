# Build and Publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only csproj for cached restore
COPY MatchMaking.Service.csproj ./
RUN dotnet restore

# Copy project
COPY . ./

# Publish the project to a minimal directory
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Minimal final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy only the published application
COPY --from=build /app/publish ./

# Forward the necessary ports
EXPOSE 8080
EXPOSE 8081

# Entry point
ENTRYPOINT ["dotnet", "MatchMaking.Service.dll"]