FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /application

# copy csproj and restore as distinct layers
COPY *.sln .
COPY app/*.csproj ./app/
WORKDIR /application/app
RUN dotnet restore

# copy everything else and build app
COPY app/. /application/app/
WORKDIR /application/app/
RUN dotnet publish -c Release -o out

# run project
#FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
#WORKDIR /application
#COPY --from=build /application/app/out ./
#ENTRYPOINT ["dotnet", "app.dll"]
