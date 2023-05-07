![Logo WikiRef](https://raw.githubusercontent.com/Manu404/WikiRef/master/doc/logo.png)

[![Build status](https://ci.appveyor.com/api/projects/status/knry4e5v84tds4p6/branch/master?svg=true&passingText=master&pendingText=master&failingText=master)](https://ci.appveyor.com/project/Manu404/wikiref/branch/master)
[![Build status](https://ci.appveyor.com/api/projects/status/7opqyl1w9a8uocsq/branch/politiwiki?svg=true&passingText=politiwiki&pendingText=politiwiki&failingText=politiwiki)](https://ci.appveyor.com/project/Manu404/wikiref-politiwiki/branch/politiwiki)

WikiRef is a free/libre and open source cross-platform tool that helps you verify and archive the references on a [MediaWiki](https://www.mediawiki.org/wiki/MediaWiki) instance.

WikiRef is a tool that can analyze MediaWiki references, identify errors, and archive webpage references using the [WaybackMachine](https://archive.org/web/) service, as well as locally archiving YouTube references. By helping  you improve the quality of your instance's sourcing, this tool notifies  you of any errors it finds, such as dead links, duplicated or malformed  references, and more. It is designed to identify issues in your content, rather than fixing them. It provides a report of the identified issues, making it a useful reporting tool.

## Key features

 - Analyze references site-wise, for specific pages or categories
 - Check if references URLs are valid and accessible, so you don't have to worry about dead links
 - Verify the accessibility of YouTube video references, their online status and permissions
 - Generate a script to download those videos if needed
 - Archive non-video sources using the WayBack Machine service
 - Publish to MediaWiki reporting result

### Limits

Some false positives are possible, for instance a website with a faulty SSL certificate will trigger an error, or some websites like LinkedIn fight against bot accessing their content. A "whitelist" system is implemented to handle those cases.

## Support or contribute

If you need anything, have any ideas or find any bugs or issues, let me know through the issue tracker, I'm totally open to new suggestions ! Open an issue or contact me by [mail](mailto:contact@emmanuelistace.be).

##  Usage

### General Overview

The core of the tool is analysis mode, which analyze references and links, then produces JSON used as input for the other modes.

![Schema of the architecture of WikiRef](https://github.com/Manu404/WikiRef/raw/master/doc/Overview_Archi.drawio.png)

##### Note

- On Windows, wikiref will be replaced by wikiref.exe

## General options

Here's the list of options that apply to all modes.

|      |               | Flag | Value              | Description                                                  |
| ---- | ------------- | :--: | ------------------ | ------------------------------------------------------------ |
| -v   | --verbose     |  ⬤   |                    | Output more information in the console                       |
| -s   | --silent      |  ⬤   |                    | No console output                                            |
| -b   | --color-blind |  ⬤   |                    | Disable coloring of the console output - Compatibility for certain terminal; |
| -t   | --throttle    |      | Duration in second | Enable throtteling between request. Required for youtube who blacklist ip making too many request too quickly from datacenters (6second seems to work great) |
| -l   |               |  ⬤   |                    | Ouptut console to a log file with the date and time as name. |
|      | --log         |      | Filename           | Same as -l but with a specific filename                      |
| -h   |               |  ⬤   |                    | Output the console in an HTMl file with rendering close to colored the console output. |
|      | --html        |      | Filename           | Same as -h but with a specific filename                      |
|      | --subdir      |  ⬤   |                    | Place output files in dedicated folders (json, html, log)    |
| -4   | -ipv4         |  ⬤   |                    | Force ipv4 DNS resolution and queries for compatibility in certain corner case |

## Analyse mode

This mode analyzes references and checks the validity of used URLs and generate the json used by the other modes.

##### Options

Here's the list of options for the Analyze mode.

|      |             | Flag |                      Required                       | Description                                                  |
| ---- | ----------- | :--: | :-------------------------------------------------: | ------------------------------------------------------------ |
| -a   | --api       |      |                          ⬤                          | Url of api.php                                               |
| -c   | --category  |      |   ⬤<br />(mutually exclusive with page parameter)   | The name of the category to analyze                          |
| -p   | --page      |      | ⬤<br />(mutually exclusive with category parameter) | The name of the page to analyze                              |
| -j   |             |  ⬤   |                                                     | Output the analysis to a file with a generated name based on the date |
|      | --json      |      |                      Filename                       | Same as -h but with a specific filename                      |
|      | --whitelist |      |                                                     | Filename of a json file containing domain to whitelist.      |

##### Example usages

Analyze all references from pages in the category Science on the wiki https://demowiki.com/

```
wikiref analyse -a https://demowiki.com/w/api.php -c Science
```

Analyze all references from the page Informatic on the wiki https://demowiki.com/

```
wikiref analyse -a https://demowiki.com/w/api.php -p Informatic
```

Analyze all references from pages in the category Science on the wiki https://demowiki.com/; put the output in a file called "output.json" and output nothing in the console

```
wikiref analyze -a https://demowiki.com/w/api.php -c Science --json output.json -s
```

##### Whitelisting

A whitelisting system is present to avoid checking domains that prevents tools like this to check their page status or domains you trust. 

You can provide a JSON file using the following format:

`[ "google.com", "www.linkedin.com/", "www.odysee.com" ]`

The URLs starting with these domains will not be checked by the system to avoid false positive in the report.

## Script mode

This mode relies on the output of the analyze mode and uses yt-dlp for downloading, but other tools can be used as well.

This mode relies on a free/libre open-source and cross-platform tool [yt-dlp](https://github.com/yt-dlp/yt-dlp). Check their document for installation depending on your system [here](https://github.com/yt-dlp/yt-dlp/wiki/Installation).

The filenames are composed of the video name followed by the YouTube VideoId.

#### Options

Here's the list of options for the Script mode.

|  |  | Flag | Required | Value | Description |
|---|---|:-:|:-:|---|---|
| -i   | --input-json        |      |    ⬤    | Filename                       | Input JSON source from analyze to generate the download script file |
| -d   | --directory         |      |    ⬤    | Path                           | The root folder where to put videos. They will then be placed in a subfolder based on the page name. |
|      | --output-script     |      |          | Filename                       | Allow to change the default name of the output script        |
|      | --tool              |      |    ⬤    | Filename                       | Path to yt-dlp                                               |
|    | --tool-arguments |      |          | Tool arguments                 | Default arguments are  "-S res,ext:mp4:m4a --recode mp4" to download 480p verison of the videos. |
| -e   | --extension         |      |          | File extension without the "." | Extension of the video file.                                 |
|      | --redownload        |  ⬤  |          |                                | Download the video, even if it already exist.                |
|      | --download-playlist |  ⬤  |          |                                | Download videos in playlist URLS                             |
|      | --download-channel  |  ⬤  |          |                                | Download videos in channel URLS                              |

##### Example usages

Generate a script called download.sh to download all videos contained in the analyse file into the folder "video" using yt-dlp

```
wikiref script -i output.json -d ./videos/ --tool /bin/usr/yt-dlp --output-script download.sh
```

Note: Under windows, wikiref will be replaced by wikiref‧exe

## Archive mode

This mode archive URL through WaybackMachine. YouTube links are not archived through WaybackMachine, due to YouTube not allowing it.

#### Options

Here's the list of options for the Archive mode.

|      |              | Flag | Required | Value    | Description                                                  |
| ---- | ------------ | :--: | :------: | -------- | ------------------------------------------------------------ |
| -a   | --input-json |      |    ⬤     | Filename | Input JSON source from analyze to get URL to archive.        |
| -w   | --wait       |  ⬤   |          |          | Wait for confirmation of archival from Archive.org. Use with caution, archive.org might usually take a long time to archive pages. Sometimes many minutes. |


#### Example usage

```
wikiref archive -i output.json
```

## Publish mode

Publish the result of an JSON analysis to a MediaWiki page.

#### Options

Here's the list of options for the Archive mode.

|      |               | Flag | Required | Value    | Description                                            |
| ---- | ------------- | :--: | :------: | -------- | ------------------------------------------------------ |
| -a   | --api         |      |    ⬤     |          | Url of api.php                                         |
| -i   | --input       |      |    ⬤     | Filename | Input JSON source from analyze to generate report for. |
| -u   | --user        |      |    ⬤     |          | Wiki username used to publish the edit                 |
| -p   | --password    |      |    ⬤     |          | The user password.                                     |
|      | --report-page |      |    ⬤     |          | Name of the target page where to write the report.     |


#### Example usage

```
wikiref publish --api https://monwiki/wiki/api.php -i analyse.json -u Manu404 -p "a$sw0rd" --report-page "Report page"
```

## 

## Distribution

The binary is availabla as a *Self-contained*, or "portable" application. 
A single file with no other dependencies than yt-dlp for downloading youtube videos.

### Installation & Supported systems

#### Compatibility

Currently, supported systems are:

 - *Windows 64-bit* & *32-bit* from 10 and above.
 - *Linux 64-bit*  (Most desktop distributions like CentOS, Debian, Fedora, Ubuntu, and derivatives)
 - *Linux ARM* (Linux distributions running on Arm like Raspbian on Raspberry Pi Model 2+)
 - *Linux ARM64* (Linux distributions running on 64-bit Arm like Ubuntu Server 64-bit on Raspberry Pi Model 3+) 

Although the tool may theoretically support many platforms, it has only  been thoroughly tested on a select few. As such, we cannot guarantee  optimal functionality on all platforms without proper feedback. To err  on the side of caution, any untested systems will be categorized as "not yet supported." However, if you have feedback regarding a system that  is not currently listed as a supported platform, please don't hesitate  to contact us. We welcome your feedback and will work to improve the  tool's functionality across as many platforms as possible.

| System                       |     Supported     |
| ---------------------------- | :---------------: |
| Windows 10 64 bits 22H2      |         ⬤         |
| Windows 11 64b bits          |     Presumed      |
| Ubuntu Server 22.04          |         ⬤         |
| Ubuntu Server 20.04          |         ⬤         |
| Ubuntu Server 18.04          |         ⬤         |
| Ubuntu Server 18.04          |         ⬤         |
| Ubuntu Desktop 20.04         |         ⬤         |
| Debian 11.6                  |         ⬤         |
| Debian 10.13                 |         ⬤         |
| Fedora Workstation 37.1      |         ⬤         |
| Fedora Server 37.1           |         ⬤         |
| CentOS Server 7              |         ⬤         |
| CentOS Desktop 7             |         ⬤         |
| OpenSuse Tumbleweed          |         ⬤         |
| Raspbian (Raspberry pi)      |         ⬤         |
| Linux Alpine Standard 3.17.3 | Not Supported Yet |
| RHEL                         | Not Supported Yet |
| MacOS                        | Not Supported Yet |

#### CentOS

CentOS 7 user needs the following environment variable:

```
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
```

#### Not yet supported

  - *Linux Alpine Standard 3.17.3* : The software works but produce incomplete results, it can't be considered supported knowing there's error in his output json. Looking into it.
  - *MacOS*: Support for MacOS will be made available in the near future, once it has been tested.
  - *RHEL systems*: While CentOS and Fedora are supported systems, the likelihood of RHEL systems being supported is low.

  If you need to build the solution for a system or architecture that is not officially supported, please refer to the "Build for not officially supported system" section located at the end of the page for guidance.

## Building the tool

#### Getting the code

Retreive submodules when cloning the repository as WikiRef.Commons is in a separated repo due to being shared with other projects.

#### Dependencies

Build is done on Windows using gitbash (normally provided with git) or on Linux. You don't need any IDE like Visual Studio or similar. What you need is:

- Git for Windows: https://git-scm.com/download/win
- DotNet 7 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/7.0
- Zip and bzip2 2 need to be added to your gitbash if you're on windows, (it's really easy to install); here's a straight forward tutorial: https://ranxing.wordpress.com/2016/12/13/add-zip-into-git-bash-on-windows/

Under linux:

- DotNet 7 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/7.0
- Zip command

#### Cloning the repository

Use recursive git cloning to get the application submodules.

#### Compilation

Once the dependencies and submodules are installed and, you're ready to compile by yourself the project.

- After installing the dependencies and submodules, you will be able to compile the project on your own. The compilation process relies on two scripts:

  - The root script, `build.sh`, contains the project variables and calls a generic build script. You can edit this script to modify the parameters passed to the real build script - it's a straightforward process.
  - The actual compilation script is `multiplateform_build.sh`. It can take the following parameters. If a parameter is not provided, an interactive prompt will ask you to enter the information.
  
  | Parameter |           | Description                                               |
  | --------- | --------- | --------------------------------------------------------- |
  | -t        | --target  | The target plateteforme (cfr suppported plateform)        |
  | -p        | --project | Path to the project file                                  |
  | -n        | --name    | Project name used for the zip file                        |
  | -v        | --version | Version use for the zip file                              |
  | -e        | --embeded | Produce a SelfContained ("portable") file (default false) |
  | -a        | --all     | Build all plateform available                             |

A clean is done before each build.

The build output is placed in *""./output/build/\<plateform\>""*

A zip containing the build output is placed in *"./output/zip/\<plateform\>.zip"*

The zip name use the folllowing convention: 

```
<name>_<version>_<plateform>.zip
```

#### Remarks

- Sadly, WSL has compatibility issues with the "dotnet" command, so it can't being used.

#### Build for unofficially supported system

You can build for 'unofficially supported system' using the -p parameter of the build script and using for a platform available in the list [here](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog)

Example, building for macOS 13 Ventura ARM 64 : "./multiplateform_build -p osx.13-arm64"

## Beerz, greetz and personal words

If you like this tool, let me know, it's always appreciated, you can contact me [by mail](mailto:contact@emmmanuelistace.be) . 

Also, if you have any comments or would like any request, I'm totally open for them, [open an issue](https://github.com/Manu404/WikiRef/issues) or send me an [email](mailto:contact@emmmanuelistace.be).

I would like to shout-out [BadMulch](https://twitter.com/badmulch), [BienfaitsPourTous](https://bienfaitspourtous.fr/) and the [community](https://discord.gg/Y43NAEDCBy) around the [PolitiWiki project](https://politiwiki.fr/) for which this tool was developed, for their support, greetz, ideas, company during the development when I was streaming it on the discord server and overall moral support.

## Licensing

- Software is under GNU General Public License version 3. [More](https://github.com/Manu404/WikiRef/blob/master/LICENSE)
- Logos and graphics are under Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International licence. [More](https://github.com/Manu404/WikiRef/blob/master/doc/cc-by-nc-sa-4.txt)
