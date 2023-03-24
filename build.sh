#!/bin/bash

buildScript="multiplateform_build.sh"

rm -f $buildScript
curl https://raw.githubusercontent.com/Manu404/MultiplateformDotNetCoreBuildScript/main/multiplateform_build.sh --output $buildScript
chmod 755 $buildScript
./$buildScript "$@"