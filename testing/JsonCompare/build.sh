#!/bin/bash

projectVersion="1.0.0.0"
projetFile="./JsonCompare.csproj"
projectName="jsoncompare"
buildScript="multiplateform_build.sh"

chmod 755 $buildScript
./MultiplateformDotNetCoreBuildScript/$buildScript -p $projetFile -n $projectName -v $projectVersion	
