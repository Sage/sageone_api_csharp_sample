#/bin/bash

./script/stop.sh
./script/setup.sh
./script/start.sh
clear
docker logs -f sage_accounting_csharp_sample