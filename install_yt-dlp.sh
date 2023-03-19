#!/bin/bash

sudo wget https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp -O /usr/local/bin/yt-dlp
sudo chmod a+rx /usr/local/bin/yt-dlp
sudo apt-get install pip
sudo apt-get install python3.7
sudo apt install python3-pip
sudo update-alternatives --install /usr/bin/python3 python3 /usr/bin/python3.7 2
sudo pip install contextvars