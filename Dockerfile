FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /
COPY app/ app/

RUN dotnet restore /app/app.csproj

RUN mkdir /dist 

RUN dotnet build "/app/app.csproj" -c Release -o /dist

FROM build AS publish
RUN dotnet publish "app/app.csproj" -c Release -o /dist

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