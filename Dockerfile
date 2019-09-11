FROM mcr.microsoft.com/dotnet/core/sdk:2.2.401 AS build
WORKDIR /application

# copy csproj and restore as distinct layers
COPY *.sln .
COPY app/*.csproj ./app/
WORKDIR /application/app
# RUN dotnet clean
RUN dotnet restore
