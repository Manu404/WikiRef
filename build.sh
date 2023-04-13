#!/bin/bash

#rm-rf ./wikiref/obj/
target="one"
projectVersion="1.0.0.0"
projetFile="./wikiref/WikiRef.csproj"
projectName="wikiref"
buildScript="multiplateform_build.sh"
embed=false

#rm -f $buildScript
#curl https://raw.githubusercontent.com/Manu404/MultiplateformDotNetCoreBuildScript/main/multiplateform_build.sh --output $buildScript
chmod 755 $buildScript

#
# PARSE COMMAND PARAMETER
#
VALID_ARGS=$(getopt -o as -l all:single: -- "$@")
if [[ $? -ne 0 ]]; then
    exit 1;
fi

eval set -- "$VALID_ARGS"
while [ : ]; do
  case "$1" in
    -a | --all)
		target="all"
        echo "Build all plateform"
        break 
        ;;
	-s | --single)
		target="one"
        echo "Build sepecitific plateform"
        break 
        ;;
    --) shift; 
        break 
        ;;
  esac
done

#
# BUILD ALL SUPPORTED TARGET PORTABLE AND SINGLE FILE
#
if [ $target = "all" ]
then
	array=("win-x64" "win-x86" "win-arm64" "linux-x64" "linux-arm" "linux-arm64")
	echo "List of plateforme: ${array[*]}"
	for i in ${array[@]}
	do
		echo "Target platforme: '$i'"	
		./$buildScript -p $projetFile -n $projectName -v $projectVersion -t "$i"
		./$buildScript -p $projetFile -n $projectName -v $projectVersion -t "$i" -e
	done
	exit
fi

#
# REQUEST FOR PORTABLE OR MULTIFILE
#
if [ $target = "one" ]
then
	echo "Portable or multifile ?"
	PS3=Your choice: 
	kind=("portable" "multi")
	select p in "${kind[@]}"; do
		case $p in
			"portable")
				embed=true;
				break;
				;;
			"multi")
				embed=false;
				break;
				;;
			*)
		esac
	done
	
	#
	# REQUEST COMPILATION
	#
	if $embeded
	then
		./$buildScript -p $projetFile -n $projectName -v $projectVersion -e
	else
		./$buildScript -p $projetFile -n $projectName -v $projectVersion
	fi	
fi

			
