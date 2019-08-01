#!/bin/sh

echo Starting container and launch application...
#docker run -d -p 8080:8080 --name sage_accounting_csharp_sample --volume="`pwd`/app:/application/app" sage_accounting_csharp_sample /bin/bash -c "dotnet publish -c Release -o out && dotnet out/app.dll"
docker run -d -p 8080:8080 --name sage_accounting_csharp_sample --volume="`pwd`/app:/application/app" sage_accounting_csharp_sample /bin/bash -c "dotnet clean && dotnet restore && dotnet build && dotnet out/app.dll"
docker logs -f sage_accounting_csharp_sample
