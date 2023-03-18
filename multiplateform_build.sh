#!/bin/bash

echo "Choose your plateforme"
PS3='Your choice: '
plateform=("linux-x64" "win-x64")
select p in "${plateform[@]}"; do
    case $p in
        "linux-x64")
			architecture='linux-x64'
			break;
            ;;
        "win-x64")
			architecture='win-x64'
			break;
			;;
		*)
    esac
done

outputFolder='./output/'
architecture='linux-x64'
buildFolder="$outputFolder/build/"
zipOutput="$outputFolder/zip/"
outputFolder="$buildFolder/$architecture"
zipDestination="$zipOutput/$architecture.zip"

dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=True --self-contained True -o $outputFolder

echo $zipOutput
mkdir -p $zipOutput

zip -r $zipDestination $outputFolder