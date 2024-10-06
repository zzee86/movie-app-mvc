# Step 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy the project files for Docker caching 
COPY ["movie-app-mvc.sln", "."]
COPY ["movie-app-mvc/movie-app-mvc.csproj", "movie-app-mvc/"]
COPY ["movie-app-data/MovieApp.Data.csproj", "movie-app-data/"]
COPY ["MovieApp.Services/MovieApp.Services.csproj", "MovieApp.Services/"]

RUN dotnet restore "movie-app-mvc.sln"

COPY . .

WORKDIR "/src/movie-app-mvc"
RUN dotnet build "movie-app-mvc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "movie-app-mvc.csproj" -c Release -o /app/publish

# Step 3: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the entry point for the MVC app
ENTRYPOINT ["dotnet", "movie-app-mvc.dll"]
