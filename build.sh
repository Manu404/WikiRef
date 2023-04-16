#!/bin/bash

projectVersion="1.0.0.0"
projetFile="./wikiref/WikiRef.csproj"
projectName="wikiref"
buildScript="multiplateform_build.sh"

rm -f $buildScript
curl https://raw.githubusercontent.com/Manu404/MultiplateformDotNetCoreBuildScript/main/multiplateform_build.sh --output $buildScript
chmod 755 $buildScript
./$buildScript -p $projetFile -n $projectName -v $projectVersion	
