# WikiRef


WikiRef is a public domain multiplatform tool built to analyze MediaWiki references, identifies errors, archive webpage references with WaybackMachine service and create local copies of YouTube references.

In the case of errors, like dead links, duplicated references, malformed references, etc. The tool will issue an error or warning message, allowing you to act and improve the quality of the sourcing of your wiki. This tool doesn't fix the issues, it shows them.

Feature overview:
 - Analyze references for specific page or all pages in specific categories
 - Check reference if urls are valid and accessible
 - Check if YouTube videos are still online and accessible
 - Generate a script to download video used as reference
 - Archive non-video sources using WayBack Machine service (not yet implemented, but coming)

Some false positives are possible, for instance a website with a faulty SSL certificate will trigger an error, or some websites like LinkedIn fight against bot accessing their content. A "whitelist" system is being developed to ignore certain url or domains.

This tool, yet already working well for pointing out issues, is still in early stage, but working and tested. It already allowed us to fix dozens of issues on dozens of pages. 

If you need anything, have any ideas or find any bugs or issues, let me know through the issue tracker, I'm totally open to new suggestions !

##  Functionality of the software

### General Overview

The core of the tool is the analyse mode, who analyse reference and links then produce json used as input for the other modes.

![Overview_Archi.drawio](C:\git\WikiRef\doc\Overview_Archi.drawio.png)

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
wikiref analyze -w https://demowiki.com/ -c Science -o -s
```

This mode produce also a JSON file used as data source for other modes.

#### Parameter reference

|      |            | Flag |                      Required                      | Description                                                  |
| ---- | ---------- | :--: | :------------------------------------------------: | ------------------------------------------------------------ |
| -w   | --wiki     |      |                         x                          | The url of the wiki to analyze                               |
| -c   | --category |      |   x (but mutually exclusive with page parameter)   | The name of the category to analyze                          |
| -p   | --page     |      | x (but mutually exclusive with category parameter) | The name of the page to analyze                              |
| -j   |            |  x   |                                                    | Output the analysis to a file with a generated name based on the date in the executing folder |
|      | --json     |      |                                                    | Same as "-j" but allow to choose the output filename.        |

### The 'Script' mode

This more rely on the output of the analyze mode and use yt-dlp for downloading, but other tools can be used as well.

This mode relies on a free, open-source and multiplatform, the usage of the tool [yt-dlp](https://github.com/yt-dlp/yt-dlp). Check their document for installation depending on your system [here](https://github.com/yt-dlp/yt-dlp/wiki/Installation).

The filenames are composed of the the video name followed by the YouTube VideoId.

#### Example usages

Generate a script called download.sh to download all videos contained in the analyse file into the folder "video" using yt-dlp

```
wikiref script -i analyse.json -d ./videos/ --tool /bin/usr/yt-dlp --output-script download.sh
```

Note: Under windows, wikiref will be replaced by wikiref‧exe

#### Parameter reference

|  |  | Flag | Required | Value | Description |
|---|---|:-:|:-:|---|---|
| -i   | --input-json        |      |    X     | Filename                       | Input JSON source to generate the download script file       |
| -d   | --directory         |      |    X     | Path                           | The root folder where to put videos. They will then be placed in a subfolder based on the page name. |
|      | --output-script     |      |          | Filename                       | Allow to change the default name of the output script        |
|      | --tool              |      |    X     | Filename                       | Path to yt-dlp                                               |
| -a   | --arguments         |      |          | Tool argument                  | Default arguments are  "-S res,ext:mp4:m4a --recode mp4" to download 480p verison of the videos. |
| -e   | --extension         |      |          | File extension without the "." | Extension of the video file.                                 |
|      | --redownload        |  X   |          |                                | Download the video, even if it already exist.                |
|      | --download-playlist |  X   |          |                                | Download videos in playlist URLS                             |
|      | --download-channel  |  X   |          |                                | Download videos in channel URLS                              |

### The 'archive' mode

Those options are available for every mode.

|      |           | Flag | Required |      | Description                            |
| ---- | --------- | :--: | :------: | ---- | -------------------------------------- |
| -v   | --verbose |  X   |          |      | Output more information in the console |
| -s   | --silent  |  X   |          |      | No console output                      |

### General options

This mode archive all urls on https://archive.org

|      |               | Flag | Required |                    | Description                                                  |
| ---- | ------------- | :--: | :------: | ------------------ | ------------------------------------------------------------ |
| -i   | --input-json  |      |    x     |                    | Input JSON source to get URL to archive.                     |
| -w   | --wait        |  x   |          |                    | Wait for confirmation of archival from Archive.org. Use with caution, archive.org might usually take a long time to archive pages. |
| -b   | --color-blind |  X   |          |                    | Disable coloring of the console output - Compatibility for certain terminal; |
| -t   | --throttle    |      |          | Duration in second | Enable throtteling between request. Required for youtube who blacklist ip making too many request too quickly (6second seems to work great) |
| -l   |               |  X   |          |                    | Ouptut console to a log file with the date and time as name  |
|      | --log         |      |          | Filename           | Same as -l but to a specific file                            |
| -h   |               |  X   |          |                    | Output the console in an HTMl file with rendering close to the console rendering |
|      | --html        |      |          | Filename           | Same as -h but to a specific file                            |

### 

### Multiplatform

##### Supported system

For now, supported systems are:

- Windows 64bits & 32bitsbits from 7 and above.
- Linux 64 bits  (Most desktop distributions like CentOS, Debian, Fedora, Ubuntu, and derivatives)
- Linux ARM (Linux distributions running on Arm like Raspbian on Raspberry Pi Model 2+)
- Linux ARM64 (Linux distributions running on 64-bit Arm like Ubuntu Server 64-bit on Raspberry Pi Model 3+) 

##### Not yet supported system

- MacOS support will be coming soon, after testing can be done.
- RHEL systems low to no chances to be supported. (CentOS and Fedora are tho)

Remarks: Supported platforms have been tested. Tons of platforms can be technically already supported, but without guarantee and feedback it works on those systems, I prefer to let them in the "not yet supported system" by caution. if you have any feebdack about a system not listed on which the tools run, feel free to contact me.

In any case, if you need to build the solution for an architecture or system not supported, see "Build for not officially supported system" section at the end of this page.

##### Tested systems

The tool have been tested under those systems:

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

Version builds through these scripts are portable and require no dependencies. But might be a bit heavy as they embed a lot of the .net framework libraries wit them. They are all "self-contained", meaning the application is a single file, containing all it's required dependencies.

##### Dependencies

Build is meant to be done on Windows with using gitbash (normally provided with git). You don't need any IDE like Visual Studio or similar. What you need is:

- Git for Windows: https://git-scm.com/download/win
- Framework .Net Core 3.1: https://dotnet.microsoft.com/en-us/download/dotnet/3.1
- Latest .net framework SDK: https://dotnet.microsoft.com/en-us/download 
- Zip and bzip2 2 need to be added to your gitbash, it's really easy; here's a straight forward tutorial: https://ranxing.wordpress.com/2016/12/13/add-zip-into-git-bash-on-windows/

##### Compilation

Once the dependencies are installed, you're ready to compile by yourself the project.

A script is present in the root folder of the repo named *"multiplateform_build.sh"*, run it through git bash.

He will present you with a list of supported architecture and platforms, you just need to pick the one that fits your need. 

If you want to bypass the question, you can use the parameter -p and provide the platform you want to target, might be useful in CI/CD context or for other automations. Supported values are available [here](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog). Usage in this case will be for ex: *"./multiplateform_build -p \<plateform\>\"*

The build output is placed in ./output/build/\<plateform\>

A zip containing the build output is placed in ./output/zip/\<plateform\>.zip

###### Remarks

- On linux a 'chmod 755 wikiref' might be required to have it work.
- Sadly, WSL has compatibility issues with the "dotnet" command, prohibiting it to being used.

##### Build for not officially supported system

You can build for 'unofficially supported system' using the -p parameter of the build script and using for a platform available in the list [here](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog)

Example, building for macOS 13 Ventura ARM 64 : "./multiplateform_build -p osx.13-arm64"

#### Beerz, greetz and personal words

If you like this tool, let me know, it's always appreciated. As well, if you have any comments or would like any request, I'm totally open for it.

I would like to give a big hug to [BadMulch](https://twitter.com/badmulch), [BienfaitsPourTous](https://bienfaitspourtous.fr/) and their [communities](https://discord.gg/VA3kbYjCMn) around the [PolitiWiki project](https://politiwiki.fr/), for their support, greetz, ideas and for whom this tool was developed and gave me company during the development of this tool I streamed on their discord server.