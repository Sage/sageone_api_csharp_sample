#/bin/bash

./script/stop.sh
./script/setup.sh
./script/start.sh
docker logs -f sage_accounting_csharp_sample