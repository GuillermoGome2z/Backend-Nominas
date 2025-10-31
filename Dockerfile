# Use Ubuntu base image with .NET runtime pre-installed
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["ProyectoNomina.Backend/ProyectoNomina.Backend.csproj", "ProyectoNomina.Backend/"]
COPY ["ProyectoNomina.Shared/ProyectoNomina.Shared.csproj", "ProyectoNomina.Shared/"]

# Restore dependencies
RUN dotnet restore "ProyectoNomina.Backend/ProyectoNomina.Backend.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/ProyectoNomina.Backend"
RUN dotnet publish "ProyectoNomina.Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage - runtime
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Create uploads directory
RUN mkdir -p /app/Uploads/Expedientes

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ProyectoNomina.Backend.dll"]