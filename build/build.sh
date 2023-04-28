#!/bin/bash

projectVersion="1.0.0.0"
projetFile="./wikiref/WikiRef.csproj"
projectName="wikiref"
buildScript="multiplateform_build.sh"

chmod 755 $buildScript
cd ../
./build/MultiplateformDotNetCoreBuildScript/$buildScript -p $projetFile -n $projectName -v $projectVersion	