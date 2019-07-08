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


FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /application
COPY --from=build /application/app/out ./
ENTRYPOINT ["dotnet", "app.dll"]


#RUN dotnet /dist/app.dll


#COPY library/library.csproj library/
#RUN dotnet restore "app/app.csproj"
#COPY . .
#WORKDIR "/app"
#RUN dotnet build "app/app.csproj" -c Release -o /app

#FROM build AS publish
#RUN dotnet publish "app/app.csproj" -c Release -o /app

#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app .
#ENTRYPOINT ["dotnet", "app.dll"]