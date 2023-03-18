#!/bin/bash

echo -----------------------------------
echo Settings up build
echo -----------------------------------

# Required variables
outputFolder='./output/'
selectedPlateform=""
buildFolder="$outputFolder/build/"
zipOutput="$outputFolder/zip/"
outputFolder="$buildFolder/$selectedPlateform"
zipDestination="$zipOutput/$selectedPlateform.zip"

#
# PARSE COMMAND PARAMETER
#
VALID_ARGS=$(getopt -o p: --long plateform: -- "$@")
if [[ $? -ne 0 ]]; then
    exit 1;
fi

eval set -- "$VALID_ARGS"
while [ : ]; do
  case "$1" in
    -p | --plateform)
		selectedPlateform="$2"
        echo "Provided plateform is '$2'"
        shift 2
        ;;
    --) shift; 
        break 
        ;;
  esac
done

#
# IF NO COMMAND PARAMETER GIVEN, ASK THE USE THE TARGET PLATEFORM IN THE OFFICIALLY SUPPORTED PLATEFORM
#
if [ -z "${selectedPlateform}" ]
then
	echo "Choose your plateform"
	PS3='Your choice: '
	plateform=("linux-x64" "win-x64")
	select p in "${plateform[@]}"; do
		case $p in
			"linux-x64")
				selectedPlateform='linux-x64'
				break;
				;;
			"win-x64")
				selectedPlateform='win-x64'
				break;
				;;
			*)
		esac
	done
fi

#
# BUILD OF THE PROJECT
#
echo -----------------------------------
echo Building project
echo -----------------------------------
dotnet publish -v n -c Release -r linux-x64 -p:PublishSingleFile=True --self-contained True -o $outputFolder


#
# PACKING OF THE BUILD OUTPUT
#
echo -----------------------------------
echo Packing project
echo -----------------------------------
echo $zipOutput
mkdir -p $zipOutput
zip -r $zipDestination $outputFolder



