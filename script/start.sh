#!/bin/sh

echo Starting container and launch application...
#docker run -d --name sageone_api_csharp_dotnet_core_sample -p 8080:80
#docker run -it --entrypoint /bin/bash -p 192.168.99.100:8080:80/tcp sageone_api_csharp_dotnet_core_sample

docker run -d -p 192.168.99.100:8080:80/tcp sageone_api_csharp_dotnet_core_sample dotnet "/dist/app.dll"