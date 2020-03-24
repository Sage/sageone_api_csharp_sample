#!/bin/sh

echo Starting container and build the application...

docker run -d -p 8080:8080 --name sage_accounting_csharp_sample --volume="`pwd`/app:/application/app" sage_accounting_csharp_sample /bin/bash -c "dotnet publish"
docker logs -f sage_accounting_csharp_sample
