#!/bin/sh

echo Stopping container ...
docker stop sage_accounting_csharp_sample
docker rm sage_accounting_csharp_sample

rm -r app/bin
rm -r app/obj
