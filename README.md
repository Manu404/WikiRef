# WikiRef


WikiRef is a public domain multiplatform tool built to analyze MediaWiki references, identifies errors, archive webpage references with WaybackMachine service and create local copies of YouTube references.

In the case of errors, like dead links, duplicated references, malformed references, etc. The tool will issue an error or warning message, allowing you to act and improve the quality of the sourcing of your wiki. This tool doesn't fix the issues, it shows them.

Feature overview:
 - Analyze references for specific page or all pages in specific categories
 - Check reference urls are valid and accessible
 - Check YouTube videos are still online and visible
 - Generate a list of url used, that can be aggregated (for instance YouTube url containing timestamp)
 - Output YouTube url list as JSON file for use by other tools
 - Archive non-video sources using WayBack Machine service (not yet implemented, but coming)

Some false positives are possible, for instance a website with a faulty SSL certificate will trigger an error, or some websites like LinkedIn fight against bot accessing their content. A "whitelist" system is being developed to allow to ignore certain url or domains.

This tool, yet already working well for pointing out issues, is still in early stage, tested only on one project and missing features I haven't talked about until they are done. But it allowed us to fix a few dozens of issues on a few dozen pages. 

If you need anything, have any ideas or find any bugs or issues, let me know through the issue tracker, I'm totally open to new suggestions !

##  Functionality of the software



### The 'analyze' mode

This mode analyzes references and checks the validity of the urls used.

All these parameters are used and compatible in the other modes.

#### Example usages

Analyze all references from pages in the category Science on the wiki https://demowiki.com/

```
wikiref analyze -w https://demowiki.com/ -c Science
```

Analyze all references from the page Informatic on the wiki https://demowiki.com/

```
wikiref analyze -w https://demowiki.com/ -p Informatic
```

Analyze all references from pages in the category Science on the wiki https://demowiki.com/; put the output in a file and output nothing in the console

```
wikiref analyze -w https://demowiki.com/ -c Science -o -s
```



#### Parameter reference

| -w | --wiki |  | x | The url of the wiki to analyze |
|---|---|:-:|:-:|---|
| -c | --category |  | x (but mutually exclusive with page parameter) | The name of the category to analyze |
|  |  | Flag | Required | Description |
| -p | --page |  | x (but mutually exclusive with category parameter) | The name of the page to analyze |
| -o | --file-output | x |  | Output the analysis to a file with a generated name based on the date in the executing folder |
| -v | --verbose | x |  | Verbose output |
| -s | --silent | x |  | Produce no output in the console |
|  | --no-color | x |  | Disable coloring of the output for terminal having trouble with the coloring of the text |
|  | --throttle |  |  | Give a value in second to enable throttling to avoid '429 : Too Many Request' errors. Mainly for YouTube. That will slow down the speed but avoid temporary banning. |

### The 'YouTube' mode

Those options are a superset of arguments for the YouTube mode. 

#### Example usages

Analyze all references from page Informatic on the wiki https://demowiki.com/ and find all YouTube videos, aggregate them and output them on the screen and in a JSON file

```
wikiref youtube -w https://demowiki.com/ -p Informatic -a --json-output -d
```

Analyze all references from page Informatic on the wiki https://demowiki.com/ and find all YouTube videos, whiteout aggregating , write the terminal output in a file but output nothing to the terminal

```
wikiref youtube -w https://demowiki.com/ -p Informatic --json-output -s -o
```

Note: Under windows, wikiref will be replaced by wikiref‧exe

#### Parameter reference

|  |  | Flag | Required | Description |
|---|---|:-:|:-:|---|
| -d | --display | x |  | Display complete list of  YouTube references. Default behavior. |
| -a | --aggregate | x |  | Display an aggregated view of YouTube reference based on VideoId. |
|  | --json-output | x |  | Output the YouTube urls grouped by page in a file in JSON format for usage by other tools |
|  | --valid-links | x | | Display or export only valid links. |

### The 'archive' mode

Currently, in development. TBD

### The 'backup' mode

Those options are a superset of arguments for creating local backup of YouTube videos used as references. See example.

This mode relies on a free, open-source and multiplatform, the usage of the tool [yt-dlp](https://github.com/yt-dlp/yt-dlp). Check their document for installation depending on your system [here](https://github.com/yt-dlp/yt-dlp/wiki/Installation).

The videos a placed in a folder given as parameter. Within this folder, each page will create a folder, containing their related video.

The files use the video name followed by the YouTube VideoId.

|      |                     | Flag | Required |      | Description                                                  |
| ---- | ------------------- | :--: | :------: | ---- | ------------------------------------------------------------ |
| -d   | --download          |  x   |    x     |      | Default to true. Download the videos. Might be useful to have it set to false for testing purposes. |
|      | --redownload        |  x   |    x     |      | Redownload the video, even if they already exist locally.    |
| -t   | --tool-path         |      |    x     |      | Location of yt-dlp tool.                                     |
| -a   | --arguments         |      |          |      | Provide default argument "-S res,ext:mp4:m4a --recode mp4", that produces a good compromise between quality and weight, close to 480p format, our goal when developing the tool being mainly focused on audio. But feel free to provide the arguments that fit best your needs. Check the documentation [here](https://github.com/yt-dlp/yt-dlp#usage-and-options). Warning: The filename output parameter can't be changed as it's handled by the tool itself. Everything else can be adapted. |
| -r   | --root-folder       |      |    x     |      | The folder in which the video will be saved                  |
|      | --download-playlist |  x   |          |      | Download playlist url content. Default: false                |
|      | --download-channel  |  x   |          |      | "Download channel url content. Default: false                |

### Multiplatform

##### Supported system

For now, supported systems are:

- Windows 64 bits from 7 and above.
- "Standard" Linux 64 bits like CentOS, Debian, Fedora, Ubuntu, and derivatives

##### Not yet supported system

- MacOS support will be coming soon, after testing can be done.
- Arm and arm64 architecture, both Linux and windows, will be supported in the future, for instance for raspberries.
- There's no plan to support x86 architecture for any platform unless explicitly asked, let me know if it's the case. I consider this architecture to be totally outdated in 2023 and only remaining in niche sectors. 
- RHEL systems low to no chances to be supported.

Remarks: I considered supported platform on which we have feedback of the software working. Tons of platforms can be technically already supported, but without guarantee and feedback it works on those systems, I prefer to let them in the "not yet supported system" by caution.

In any case, if you need to build the solution for an architecture or system not supported, see "Build for not officially supported system" section at the end of this page.

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