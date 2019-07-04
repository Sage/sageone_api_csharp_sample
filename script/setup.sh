#!/bin/sh

echo Building docker image ...
docker build --rm -t sage_accounting_csharp_sample -f Dockerfile .

echo Setup complete.
