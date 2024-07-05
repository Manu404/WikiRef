#!/bin/bash

projectVersion="1.0.0.3"
projetFile="./WikiRef/WikiRef.csproj"
projectName="wikiref"
buildScript="multiplateform_build.sh"

chmod 755 $buildScript
cd ../
./build/MultiplateformDotNetCoreBuildScript/$buildScript -p $projetFile -n $projectName -v $projectVersion	
