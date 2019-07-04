#!/bin/sh

echo Stopping container ...
docker stop sageone_api_csharp_dotnet_core_sample
docker rm sageone_api_csharp_dotnet_core_sample
