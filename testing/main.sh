#!/bin/bash

# cygwin (wget required) : https://www.cygwin.com/install.html
# apt-cyg : https://github.com/transcode-open/apt-cyg
# sshpass: https://github.com/huan/sshpass.sh

export PATH=$PATH:"C:\\Program Files\\Oracle\\VirtualBox\\"

outputFolder=$(date)
version=1.0.0.0
targetwiki=

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
vm_name="Ubuntu Server 18.04" 
#restore snapshot
VBoxManage.exe snapshot "$vm_name" restore "fresh"
#start vm
VBoxManage.exe startvm "$vm_name"
#wait for key press
waitingForKeyPress
#setup target dir
echo "recreate distant wikiref dir"
sshpass -p 1234 ssh eis@192.168.2.103 -o StrictHostKeyChecking=accept-new 'rm -rf wikiref && 'mkdir wikiref''
#upload file
zip_filename="wikiref_""$version""_linux-x64_portable.zip"
zip_location="..\\output\\zip\\""$zip_filename"
echo "upload wikiref file"
sshpass -p 1234 scp $zip_location eis@192.168.2.103:/home/eis/wikiref/wikiref.zip
#unzip
echo "unzip wikiref"
sshpass -p 1234 ssh eis@192.168.2.103 -o StrictHostKeyChecking=accept-new 'cd wikiref && unzip wikiref.zip && chmod +x wikiref'
echo "Proceed to test ?"
waitingForKeyPress