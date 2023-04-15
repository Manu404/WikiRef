# WikiRef


WikiRef is a public domain multiplatform tool built to analyze MediaWiki references, identifies errors, archive webpage references with WaybackMachine service and create local copies of YouTube references.

In the case of errors, like dead links, duplicated references, malformed references, etc. The tool will issue an error or warning message, allowing you to act and improve the quality of the sourcing of your wiki. This tool doesn't fix the issues, it shows them.

Feature overview:
 - Analyze references for specific page or all pages in specific categories
 - Check reference if urls are valid and accessible
 - Check if YouTube videos are still online and accessible
 - Generate a script to download video used as reference
 - Archive non-video sources using the WayBack Machine service (not yet implemented, but coming)

Some false positives are possible, for instance a website with a faulty SSL certificate will trigger an error, or some websites like LinkedIn fight against bot accessing their content. A "whitelist" system is being developed to ignore certain url or domains.

This tool, yet already working well for pointing out issues, is still in early stage, but working and tested. It already allowed us to fix dozens of issues on dozens of pages in our wiki. 

If you need anything, have any ideas or find any bugs or issues, let me know through the issue tracker, I'm totally open to new suggestions ! Open an issue or contact me by [mail](mailto:contact@emmanuelistace.be).

##  Functionality of the software

### General Overview

The core of the tool is the analysis mode, which analyze references and links, then produces JSON used as input for the other modes.

![Overview_Archi.drawio](https://github.com/Manu404/WikiRef/blob/master/doc/Overview_Archi.drawio.png)

### Available binaries

There are two kind of available binaries:

- *Self-contained*, or "portable", the software is a single file with no other dependencies than yt-dlp for downloading youtube videos.

- *Normal*, require the DotNet 7 runtime to be installed on the machine, but lighter in weight.

#### Note

- On Windows, wikiref will be replaced by wikiref.exe
- On Linux a 'chmod 755 wikiref' might be required to have it work.

### General options

These options apply to all modes

|      |               | Flag | Value              | Description                                                  |
| ---- | ------------- | :--: | ------------------ | ------------------------------------------------------------ |
| -v   | --verbose     |  ⬤   |                    | Output more information in the console                       |
| -s   | --silent      |  ⬤   |                    | No console output                                            |
| -b   | --color-blind |  ⬤   |                    | Disable coloring of the console output - Compatibility for certain terminal; |
| -t   | --throttle    |      | Duration in second | Enable throtteling between request. Required for youtube who blacklist ip making too many request too quickly (6second seems to work great) |
| -l   |               |  ⬤   |                    | Ouptut console to a log file with the date and time as name. |
|      | --log         |      | Filename           | Same as -l but with a specific filename                      |
| -h   |               |  ⬤   |                    | Output the console in an HTMl file with rendering close to colored the console output. |
|      | --html        |      | Filename           | Same as -h but with a specific filename                      |
|      | --subdir      |  ⬤   |                    | Place output files in dedicated folders (json, html, log)    |
| -4   | -ipv4         |  ⬤   |                    | Force ipv4 DNS resolution and queries for compatibility in certain corner case |

### The 'analyse' mode

This mode analyzes references and checks the validity of the urls used.

All these parameters are used and compatible in the other modes.

#### Example usages

Analyze all references from pages in the category Science on the wiki https://demowiki.com/

```
wikiref analyse -w https://demowiki.com/ -c Science
```

Analyze all references from the page Informatic on the wiki https://demowiki.com/

```
wikiref analyse -w https://demowiki.com/ -p Informatic
```

Analyze all references from pages in the category Science on the wiki https://demowiki.com/; put the output in a file and output nothing in the console

```
wikiref analyze -w https://demowiki.com/ -c Science -j -s
```

This mode produce also a JSON file used as data source for other modes.

#### Parameter reference

|      |            | Flag |                    Required                    | Description                                                  |
| ---- | ---------- | :--: | :--------------------------------------------: | ------------------------------------------------------------ |
| -w   | --wiki     |      |                       x                        | The url of the wiki to analyze                               |
| -c   | --category |      |   x (mutually exclusive with page parameter)   | The name of the category to analyze                          |
| -p   | --page     |      | x (mutually exclusive with category parameter) | The name of the page to analyze                              |
| -j   |            |  ⬤   |                                                | Output the analysis to a file with a generated name based on the date |
|      | --json     |      |                                                | Same as -h but with a specific filename                      |

### The 'Script' mode

This mode relies on the output of the analyze mode and uses yt-dlp for downloading, but other tools can be used as well.

This mode relies on a free, open-source and multiplatform, the usage of the tool [yt-dlp](https://github.com/yt-dlp/yt-dlp). Check their document for installation depending on your system [here](https://github.com/yt-dlp/yt-dlp/wiki/Installation).

The filenames are composed of the video name followed by the YouTube VideoId.

#### Example usages

Generate a script called download.sh to download all videos contained in the analyse file into the folder "video" using yt-dlp

```
wikiref script -i analyse.json -d ./videos/ --tool /bin/usr/yt-dlp --output-script download.sh
```

Note: Under windows, wikiref will be replaced by wikiref‧exe

#### Parameter reference

|  |  | Flag | Required | Value | Description |
|---|---|:-:|:-:|---|---|
| -i   | --input-json        |      |    ⬤    | Filename                       | Input JSON source to generate the download script file       |
| -d   | --directory         |      |    ⬤    | Path                           | The root folder where to put videos. They will then be placed in a subfolder based on the page name. |
|      | --output-script     |      |          | Filename                       | Allow to change the default name of the output script        |
|      | --tool              |      |    ⬤    | Filename                       | Path to yt-dlp                                               |
| -a   | --arguments         |      |          | Tool arguments                 | Default arguments are  "-S res,ext:mp4:m4a --recode mp4" to download 480p verison of the videos. |
| -e   | --extension         |      |          | File extension without the "." | Extension of the video file.                                 |
|      | --redownload        |  ⬤  |          |                                | Download the video, even if it already exist.                |
|      | --download-playlist |  ⬤  |          |                                | Download videos in playlist URLS                             |
|      | --download-channel  |  ⬤  |          |                                | Download videos in channel URLS                              |

### The 'archive' mode

This mode archive URL through WaybackMachine. YouTube links are not archived through WaybackMachine, due to YouTube not allowing it.

```
wikiref archive -i analyse.json
```

|      |              | Flag | Required | Value    | Description                                                  |
| ---- | ------------ | :--: | :------: | -------- | ------------------------------------------------------------ |
| -i   | --input-json |      |    ⬤     | Filename | Input JSON source to get URL to archive.                     |
| -w   | --wait       |  ⬤   |          |          | Wait for confirmation of archival from Archive.org. Use with caution, archive.org might usually take a long time to archive pages. Sometimes many minutes. |

### Multiplatforming

#### Supported system

For now, supported systems are:

- Windows 64bits & 32bitsbits from 7 and above.
- Linux 64 bits  (Most desktop distributions like CentOS, Debian, Fedora, Ubuntu, and derivatives)
- Linux ARM (Linux distributions running on Arm like Raspbian on Raspberry Pi Model 2+)
- Linux ARM64 (Linux distributions running on 64-bit Arm like Ubuntu Server 64-bit on Raspberry Pi Model 3+) 

Remarks: Supported platforms have been tested. Tons of platforms can be technically already supported, but without guarantee and feedback it works on those systems, I prefer to let them in the "not yet supported system" by caution. If you have any feedback about a system not listed on which the tools run, feel free to contact me.

#### Not yet supported system

- MacOS support will be coming soon, after testing can be done.
- RHEL systems low to no chances to be supported. (CentOS and Fedora are tho)

In any case, if you need to build the solution for an architecture or system not supported, see the "Build for not officially supported system" section at the end of this page.

#### Tested systems

The tool have been tested and working under those systems:

- Windows
  - Windows 10 Pro 64 bits 22H2
  - Windows 11 [to complete]
- Linux
  - Debian-based
    - Ubuntu Server 22.04
    - Ubuntu Server 20.04
    - Ubuntu Server 18.04
    - Ubuntu Desktop 20.04
    - Debian 11.6
    - Debian 10.13
  - Red Hat based
    - Fedora Workstation 37.1	
    - Fedora Server 37.1
    - CentOS Server 7
    - CentOS Desktop 7
  - OpenSuse Tumbleweed
  - Alpine Standard 3.17.3

### Building the tool

#### Dependencies

Build is done on Windows using gitbash (normally provided with git) or on Linux. You don't need any IDE like Visual Studio or similar. What you need is:

- Git for Windows: https://git-scm.com/download/win
- DotNet 7 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/7.0
- Zip and bzip2 2 need to be added to your gitbash if you're on windows, (it's really easy to install); here's a straight forward tutorial: https://ranxing.wordpress.com/2016/12/13/add-zip-into-git-bash-on-windows/

#### Compilation

Once the dependencies are installed, you're ready to compile by yourself the project.

The compilation rely on two compile script:

- *build.sh*: download a multiplateform script and call it. It can be given the following argument
  
   | Parameter | Description                                          |
   | --------- | ---------------------------------------------------- |
   | -s        | Compile for a single plateform (default)             |
   | -a        | Compile for all plateform, normal and self-contained |
   | -e        | Specify to build a self-contained assembly.          |

- *multiplateform_build.sh*: the "real" compile script, it can be given the following parameter:

  | Parameter |           | Description                                               |
  | --------- | --------- | --------------------------------------------------------- |
  | -t        | --target  | The target plateteforme (cfr suppported plateform)        |
  | -p        | --project | Path to the project file                                  |
  | -n        | --name    | Project name used for the zip file                        |
  | -v        | --version | Version use for the zip file                              |
  | -e        |           | Produce a SelfContained ("portable") file (default false) |

A clean is done before each build.

The build output is placed in *""./output/build/\<plateform\>""*

A zip containing the build output is placed in *"./output/zip/\<plateform\>.zip"*

The zip name use the folllowing convention: 

```
<name>_<versoin>_<plateform>.zip
```

#### Remarks

- Sadly, WSL has compatibility issues with the "dotnet" command, prohibiting it to being used.

#### Build for not officially supported system

You can build for 'unofficially supported system' using the -p parameter of the build script and using for a platform available in the list [here](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog)

Example, building for macOS 13 Ventura ARM 64 : "./multiplateform_build -p osx.13-arm64"

#### Beerz, greetz and personal words

If you like this tool, let me know, it's always appreciated, [contact@emmmanuelistace.be](mailto:contact@emmmanuelistace.be) . 

As well, if you have any comments or would like any request, I'm totally open for them.

I would like to give a big hug to [BadMulch](https://twitter.com/badmulch), [BienfaitsPourTous](https://bienfaitspourtous.fr/) and their [communities](https://discord.gg/VA3kbYjCMn) around the [PolitiWiki project](https://politiwiki.fr/), for their support, greetz, ideas and for whom this tool was developed and gave me company during the development when I was streaming it on their discord server.
