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
expected_script=$(pwd)"/expected_download.sh"
vm_name=""
vm_ip=""
vm_snapshot=""
vm_code=""

mkdir -p "$outputFolder"

function waitingForKeyPress(){
	echo -e "$Red"
	read -p "Press any key to continue... " -n1 -s
	echo -e "$Color_Off"
}

#
# TESTING SERVER
#
vm_name="WikiRef MediaWiki"
vm_snapshot="fresh"
echo -e  "$Cyan#> ===================================================================== $Color_Off"
echo -e  "$Cyan#>                             VM $vm_name                               $Color_Off"
echo -e  "$Cyan#> ===================================================================== $Color_Off"
echo -e  "$Cyan#> Apply snapshot $vm_snapshot for $vm_name $Color_Off" 
VBoxManage.exe snapshot "$vm_name" restore "$vm_snapshot"

echo -e  "$Cyan#> Start $vm_name $Color_Off"
VBoxManage.exe startvm "$vm_name"

#
# UBUNTU PORTABLE
#
function Test() {
	vm_name=$1
	vm_ip=$2
	vm_snapshot=$3
	vm_code=$4
	
	echo -e  "$Cyan#> ===================================================================== $Color_Off"
	echo -e  "$Cyan#>                             VM $vm_name                               $Color_Off"
	echo -e  "$Cyan#> ===================================================================== $Color_Off"
	
	echo -e  "$Cyan#> start test $Color_Off" 
	waitingForKeyPress

	echo -e  "$Cyan#> apply snapshot $vm_snapshot for $vm_name $Color_Off" 
	vboxmanage.exe snapshot "$vm_name" restore "$vm_snapshot"

	echo -e  "$Cyan#> start $vm_name $Color_Off"
	vboxmanage.exe startvm "$vm_name"

	waitingForKeyPress

	echo -e  "$Cyan#> recreate distant wikiref dir $Color_Off"
	sshpass -p 1234 ssh eis@$vm_ip 'rm -rf wikiref && mkdir wikiref'

	echo -e  "$Cyan#> upload wikiref file $Color_Off"
	zip_filename="wikiref_""$version""_linux-x64_portable.zip"
	zip_location="..\\output\\zip\\""$zip_filename"
	sshpass -p 1234 scp $zip_location eis@$vm_ip:/home/eis/wikiref/wikiref.zip

	echo -e  "$Cyan#> unzip wikiref $Color_Off"
	sshpass -p 1234 ssh eis@$vm_ip 'cd wikiref && unzip wikiref.zip && chmod +x wikiref'

	echo -e  "$Cyan#> analyse $Color_Off"
	sshpass -p 1234 ssh eis@$vm_ip "cd wikiref && ./wikiref analyse -w $targetwiki -c \"test pages\" --json output.json -b -l"
	
	echo -e  "$Cyan#> build fake video $Color_Off"
	sshpass -p 1234 ssh eis@$vm_ip "cd wikiref && mkdir -p ./video/Psyhodelik"
	sshpass -p 1234 ssh eis@$vm_ip "cd wikiref && touch ./video/Psyhodelik/LE_PREMIER_PRETRE_NON_BINAIRE_GRACE_A_ADAM_ET_EVE__[E0gzvUc797I].mp4"
	sshpass -p 1234 ssh eis@$vm_ip "cd wikiref && touch ./video/Psyhodelik/#RDF_Psyhodelik_clash__La_gauchiasse!__[gcMm2zzUP6s].mp4"
	
	echo -e  "$Cyan#> generate download script $Color_Off"
	sshpass -p 1234 ssh eis@$vm_ip "cd wikiref && ./wikiref script -i output.json --tool /usr/local/bin/yt-dlp -d ./video --output-script download_script.sh"
		
	echo -e  "$Cyan#> download json $Color_Off"
	output_json=$(pwd)"/$outputFolder/output_$vm_code.json"
	sshpass -p 1234 scp eis@$vm_ip:/home/eis/wikiref/output.json _output.json
	mv _output.json "$output_json"
	
	echo -e  "$Cyan#> download download script $Color_Off"
	output_script=$(pwd)"/$outputFolder/download_$vm_code.sh"
	echo $output_script
	sshpass -p 1234 scp eis@$vm_ip:/home/eis/wikiref/download_script.sh _download.json
	mv _download.json "$output_script"

	echo -e  "$Cyan#> convert eol $Color_Off"
	unix2dos "$output_json"
	unix2dos "$output_script"
	
	echo $output_script
	echo $expected_script
	echo -e  "$Cyan#> Diff $output_script and $expected_script $Color_Off"
	if [ "$(wc -l < "$output_script")" -eq "$(wc -l < $expected_script)" ]; 
	then 
		echo -e "$Green#> Download file contains the same number of downloads$Color_Off"; 
	else 
		echo -e "$Red#> Differenre number of lines$Color_Off"; 
		echo -e "$Red#> Expected: $(wc -l < $expected_script)$Color_Off"; 
		echo -e "$Red#> Provided: $(wc -l < "$output_script")$Color_Off"; 
	fi

	echo -e  "$Cyan#> Diff $outputJson and $expected_json $Color_Off"
	diff -q "$output_json" "$expected_json"
	if [[ $? == "0" ]]
	then
	  echo -e  "$Green#> JSON Files are identical $Color_Off"
	else
	  echo -e  "$Red#> JSON Files are different $Color_Off"
	fi
	
	echo -e  "$Cyan#> Shutdown $vm_name $Color_Off"
	vboxmanage.exe controlvm "$vm_name" acpipowerbutton
	
	echo -e  "$Cyan#> $Color_Off"
	echo -e  "$Cyan#> --------------------------------------------------------------------- $Color_Off"
	echo -e  "$Cyan#> $Color_Off"
}

Test "Ubuntu Server 18.04" "192.168.2.103" "fresh" "ubuntu_1804"