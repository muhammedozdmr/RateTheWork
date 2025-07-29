FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln .
COPY RateTheWork.API/*.csproj ./RateTheWork.API/
COPY RateTheWork.Application/*.csproj ./RateTheWork.Application/
COPY RateTheWork.Domain/*.csproj ./RateTheWork.Domain/
COPY RateTheWork.Infrastructure/*.csproj ./RateTheWork.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build and publish
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Railway uses PORT environment variable
ENV ASPNETCORE_URLS=http://+:${PORT}

ENTRYPOINT ["dotnet", "RateTheWork.API.dll"]