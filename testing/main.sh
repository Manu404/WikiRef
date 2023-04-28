#!/bin/bash

# cygwin (wget required) : https://www.cygwin.com/install.html
# apt-cyg : https://github.com/transcode-open/apt-cyg
# sshpass: https://github.com/huan/sshpass.sh

#
# COLORS
#
source ./color.sh

export PATH=$PATH:"C:\\Program Files\\Oracle\\VirtualBox\\"

outputFolder=$(date)
version=1.0.0.0
targetwiki="http://192.168.2.112"
expected_json=$(pwd)"/expected_output.json"


mkdir -p "$outputFolder"

function waitingForKeyPress(){
	read -p "Press any key to continue... " -n1 -s
}

#
# TESTING SERVER
#

vm_name="WikiRef MediaWiki"
#restor mediawiki vm
VBoxManage.exe snapshot "$vm_name" restore "fresh"
#start vm
VBoxManage.exe startvm "$vm_name"

#
# UBUNTU PORTABLE
#
function Test() {
	vm_name=$1
	vm_ip=$2
	vm_snapshot=$3
	
	echo "$Blue#> Start test $Color_Off" 
	waitingForKeyPress

	echo "$Blue#> Apply snapshot $vm_snapshot for $vm_name$Color_Off" 
	VBoxManage.exe snapshot "$vm_name" restore "$vm_snapshot"

	echo "$Blue#> Start $vm_name$Color_Off"
	VBoxManage.exe startvm "$vm_name"

	waitingForKeyPress

	echo "$Blue#> Recreate distant wikiref dir$Color_Off"
	sshpass -p 1234 ssh eis@$vm_ip 'rm -rf wikiref && mkdir wikiref'

	echo "$Blue#> Upload wikiref file$Color_Off"
	zip_filename="wikiref_""$version""_linux-x64_portable.zip"
	zip_location="..\\output\\zip\\""$zip_filename"
	sshpass -p 1234 scp $zip_location eis@$vm_ip:/home/eis/wikiref/wikiref.zip

	echo "$Blue#> Unzip wikiref$Color_Off"
	sshpass -p 1234 ssh eis@$vm_ip 'cd wikiref && unzip wikiref.zip && chmod +x wikiref'

	echo "$Blue#> Analyse$Color_Off"
	sshpass -p 1234 ssh eis@$vm_ip "cd wikiref && ./wikiref analyse -w $targetwiki -c \"Test pages\" --json output.json -b -l"
	
	echo "$Blue#> Generate download script$Color_Off"
	sshpass -p 1234 ssh eis@$vm_ip "cd wikiref && ./wikiref analyse -w $targetwiki -c \"Test pages\" --json output.json -b -l"

	echo "$Blue#> Download json$Color_Off"
	outputJson=$(pwd)"/$outputFolder/output_ubuntu1804.json"
	sshpass -p 1234 scp eis@$vm_ip:/home/eis/wikiref/output.json _output.json
	mv _output.json "$outputJson"
	

	echo "$Blue#> Convert EOL$Color_Off"
	unix2dos "$outputJson"

	echo "$Blue#> Diff $outputJson and $expected_json$Color_Off"
	diff -q "$outputJson" "$expected_json"
	if [[ $? == "0" ]]
	then
	  echo "$GreenThe same$Color_Off"
	else
	  echo "$RedNot the same$Color_Off"
	fi
	
	echo "$Blue#> Shutdown $vm_name$Color_Off"
	sshpass -p 1234 ssh eis@$vm_ip 'shutdown -h now'
}

Test "Ubuntu Server 18.04" "192.168.2.103" "fresh"