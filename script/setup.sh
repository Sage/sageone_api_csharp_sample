#!/bin/sh

echo Building docker image ...
docker build --rm -t sageone_api_csharp_dotnet_core_sample -f Dockerfile .

echo Setup complete.
