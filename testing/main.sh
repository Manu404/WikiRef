#!/bin/bash

# cygwin (wget required) : https://www.cygwin.com/install.html
# apt-cyg : https://github.com/transcode-open/apt-cyg
# sshpass: https://github.com/huan/sshpass.sh

#
# COLORS
#
source ./color.sh

export PATH=$PATH:"C:\\Program Files\\Oracle\\VirtualBox\\"

version=1.0.0.1

output_folder=$(date '+%Y-%m-%d@%H-%M-%S')
remote_output_json="output.json"
expected_json=$(pwd)"/expected_output.json"
expected_script=$(pwd)"/expected_download.sh"
compare_tool=$(pwd)"/JsonCompare/output/build/win-x64_portable/JsonCompare.exe"

vm_name=""
vm_ip=""
vm_snapshot=""
vm_code=""

wiki_api="http://192.168.2.112/api.php"
wiki_user="Manu404"
wiki_password="9uM6U4tCBRgkR4V"
wiki_report_page="Page de Rapport"

mkdir -p "$output_folder"

function waitingForKeyPress(){
	echo -e "$Red"
	read -p "Press any key to continue... " -n1 -s
	echo -e "$Color_Off"
}

#
# TESTING SERVER
#
vm_name="WikiRef"
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
function PortableTest() {
	vm_name=$1
	vm_ip=$2
	vm_snapshot=$3
	vm_code=$4
	ssh_user=$5
	ssh_password=$6
	
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
	sshpass -p $ssh_password ssh $ssh_user@$vm_ip 'rm -rf wikiref && mkdir wikiref'

	echo -e  "$Cyan#> upload wikiref file $Color_Off"
	zip_filename="wikiref_""$version""_linux-x64_portable.zip"
	zip_location="..\\output\\zip\\""$zip_filename"
	sshpass -p $ssh_password scp $zip_location eis@$vm_ip:/home/eis/wikiref/wikiref.zip | cat

	echo -e  "$Cyan#> unzip wikiref $Color_Off"
	sshpass -p $ssh_password ssh $ssh_user@$vm_ip 'cd wikiref && unzip wikiref.zip && chmod +x wikiref' 
	
	echo -e  "$Cyan#> analyse $Color_Off"
	if [[ $vm_name = "CentOS Server 7" ]] || [[ $vm_name = "Alpine Standard 3.17.3" ]]
	then
		echo -e  "$Cyan#> Set DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 $Color_Off"
		sshpass -p $ssh_password ssh $ssh_user@$vm_ip "export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 && cd wikiref && ./wikiref analyse -a $wiki_api -c \"test pages\" --json $remote_output_json -b -l" | cat
	else 
		sshpass -p $ssh_password ssh $ssh_user@$vm_ip "cd wikiref && ./wikiref analyse -a $wiki_api -c \"test pages\" --json $remote_output_json -b -l" | cat
	fi
	
	echo -e  "$Cyan#> build fake video $Color_Off"
	sshpass -p $ssh_password ssh $ssh_user@$vm_ip "cd wikiref && mkdir -p ./video/Psyhodelik"
	sshpass -p $ssh_password ssh $ssh_user@$vm_ip "cd wikiref && touch ./video/Psyhodelik/LE_PREMIER_PRETRE_NON_BINAIRE_GRACE_A_ADAM_ET_EVE__[E0gzvUc797I].mp4" 
	sshpass -p $ssh_password ssh $ssh_user@$vm_ip "cd wikiref && touch ./video/Psyhodelik/#RDF_Psyhodelik_clash__La_gauchiasse!__[gcMm2zzUP6s].mp4"
	
	echo -e  "$Cyan#> generate download script $Color_Off"
	if [[ $vm_name = "CentOS Server 7" ]] || [[ $vm_name = "Alpine Standard 3.17.3" ]]
	then
		echo -e  "$Cyan#> Set DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 $Color_Off"
		sshpass -p $ssh_password ssh $ssh_user@$vm_ip "export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 && cd wikiref && ./wikiref script -i $remote_output_json --tool /usr/local/bin/yt-dlp -d './video' --output-script download_script.sh" | cat
	else 
		sshpass -p $ssh_password ssh $ssh_user@$vm_ip "cd wikiref && ./wikiref script -i $remote_output_json --tool /usr/local/bin/yt-dlp -d './video' --output-script download_script.sh" | cat
	fi
	
	echo -e  "$Cyan#> publish report $Color_Off"
	if [[ $vm_name = "CentOS Server 7" ]] || [[ $vm_name = "Alpine Standard 3.17.3" ]]
	then
		echo -e  "$Cyan#> Set DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 $Color_Off"
		sshpass -p $ssh_password ssh $ssh_user@$vm_ip "export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 && cd wikiref && ./wikiref publish --api $wiki_api -i $remote_output_json -u $wiki_user -p $wiki_password --report-page $wiki_report_page" | cat
	else
		sshpass -p $ssh_password ssh $ssh_user@$vm_ip "cd wikiref && ./wikiref publish --api $wiki_api -i $remote_output_json -u $wiki_user -p $wiki_password --report-page $wiki_report_page" | cat
	fi
	
	echo -e  "$Cyan#> download json $Color_Off"
	local_output_json=$(pwd)"/$output_folder/output_$vm_code.json"
	sshpass -p $ssh_password scp $ssh_user@$vm_ip:/home/eis/wikiref/output.json _output.json
	mv _output.json "$local_output_json"
	
	echo -e  "$Cyan#> download download script $Color_Off"
	local_output_script=$(pwd)"/$output_folder/download_$vm_code.sh"
	sshpass -p $ssh_password scp $ssh_user@$vm_ip:/home/eis/wikiref/download_script.sh _download.json
	mv _download.json "$local_output_script"

	echo -e  "$Cyan#> convert eol $Color_Off"
	unix2dos "$local_output_script" -q > /dev/null
	unix2dos "$local_output_json" -q > /dev/null
	
	echo -e  "$Cyan#> Diff $output_script and $expected_script $Color_Off"
	if [ "$(wc -l < $local_output_script)" -eq "$(wc -l < $expected_script)" ];
	then 
		echo -e "$Green#> Download file contains the same number of downloads$Color_Off"; 
	else 
		echo -e "$Red#> Differenre number of lines$Color_Off"; 
		echo -e "$Red#> Expected: $(wc -l < $expected_script)$Color_Off"; 
		echo -e "$Red#> Provided: $(wc -l < "$local_output_script")$Color_Off"; 
	fi

	echo -e  "$Cyan#> Diff $local_output_json and $expected_json $Color_Off"
	$compare_tool --file-a "$local_output_json" --file-b "$expected_json"
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

ssh_user=eis
ssh_password=1234

PortableTest "Ubuntu Server 22.04" "192.168.2.101" "fresh" "ubuntu_2204" $ssh_user $ssh_password
PortableTest "Ubuntu Server 20.04" "192.168.2.102" "fresh" "ubuntu_2004" $ssh_user $ssh_password
PortableTest "Ubuntu Server 18.04" "192.168.2.103" "fresh" "ubuntu_1804" $ssh_user $ssh_password

PortableTest "Fedora Server 37.1" "192.168.2.104" "fresh" "fedora_371" $ssh_user $ssh_password

PortableTest "Debian 10.13" "192.168.2.105" "fresh" "debian_1013" $ssh_user $ssh_password
PortableTest "Debian 11.6" "192.168.2.106" "fresh" "debian_116" $ssh_user $ssh_password

PortableTest "CentOS Server 7" "192.168.2.107" "fresh" "centos_7" $ssh_user $ssh_password
PortableTest "OpenSUSE Tumbleweed" "192.168.2.108" "fresh" "opensuse_tw" $ssh_user $ssh_password
PortableTest "Alpine Standard 3.17.3" "192.168.2.114" "fresh" "alpine_3173" $ssh_user $ssh_password

PortableTest "Ubuntu Desktop 20.04" "192.168.2.201" "fresh" "ubuntu_2004d" $ssh_user $ssh_password
PortableTest "Fedora Workstation 37.7" "192.168.2.202" "fresh" "fedora_377d" $ssh_user $ssh_password
PortableTest "CentOS Desktop 7" "192.168.2.203" "fresh" "centos_7d" $ssh_user $ssh_password