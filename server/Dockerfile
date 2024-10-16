# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution file
COPY ApiBank.sln ./

# Copy all project files
COPY src/ApiBank.Core/ApiBank.Core.csproj ./src/ApiBank.Core/
COPY src/ApiBank.Shared/ApiBank.Shared.csproj ./src/ApiBank.Shared/
COPY src/ApiBank.Application/ApiBank.Application.csproj ./src/ApiBank.Application/
COPY src/ApiBank.Config/ApiBank.Config.csproj ./src/ApiBank.Config/
COPY src/ApiBank.Infrastructure/ApiBank.Infrastructure.csproj ./src/ApiBank.Infrastructure/
COPY src/ApiBank.Api/ApiBank.Api.csproj ./src/ApiBank.Api/

# Restore dependencies
RUN dotnet restore "src/ApiBank.Api/ApiBank.Api.csproj"

# Copy the entire source code
COPY src/ ./src/

# Build the project
WORKDIR /src/src/ApiBank.Api
RUN dotnet build "ApiBank.Api.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
RUN dotnet publish "ApiBank.Api.csproj" -c Release -o /app/publish

# Stage 3: Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose custom port (8083)
EXPOSE 8083

# Set the entry point
ENTRYPOINT ["dotnet", "ApiBank.Api.dll"]
