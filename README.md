# WikiRef


WikiRef is a public domain multiplateform tool built to analyze MediaWiki references, identifies errors, archive webpage references with WaybackMachine service and create local copy of youtube references.

In case of errors, like dead link, duplicated references, malformed references, etc, the tool will issue an error or warning message, allowing you to act and improve the quality of the sourcing of your wiki.

Feature overview:
 - Analyse references for specific page or all pages in specific categories
 - Check reference urls are valid and accessible
 - Check youtube videos are still online and visible
 - Generate a list of url used, that can be aggregated (for instance youtube url containing timestamp)
 - Output youtube url list as json file for use by other tools
 - Archive non video source using WayBack Machine service (not yet implemented, but coming)

Some false positive are possible, for instance a website with a faulty SSL certificate will trigger an error, or some website like linkedin fight against bot accessing their content. A "whitelist" system is being developed to allow to ignore certain url or domains.

This tool, yet already working well for pointing out issues, is stil in early stage, tested only on one project and missing features I haven't talked about until they are done. But it allowed us to fix few dozens issues on few dozen pages. 

If you need anything, have any ideas or find any bugs or issues, let me know through the issue tracker !

## Example

Analyze all references from pages in category Science on the wiki https://demowiki.com/
> wikiref analyze -w https://demowiki.com/ -c Science

Analyze all references from the page Informatic on the wiki https://demowiki.com/
> wikiref analyze -w https://demowiki.com/ -p Informatic

Analyze all references from pages in category Science on the wiki https://demowiki.com/; put the output in a file and output nothing in the console
> wikiref analyze -w https://demowiki.com/ -c Science -o -s

Analyze all references from page Informatic on the wiki https://demowiki.com/ and find all youtube videos, aggregate them and output them on the screen and in a json file
> wikiref youtube -w https://demowiki.com/ -p Informatic -a -j -d

Analyze all references from page Informatic on the wiki https://demowiki.com/ and find all youtube videos, aggregate them, write the terminal output in a file but output nothing to the terminal
> wikiref youtube -w https://demowiki.com/ -p Informatic -a -j -s -o

Note: Under windows, wikiref will be replaced by wikiref‧exe

##  Command line arguments

### The 'analyze' mode

Those options are related to the analyse mode.
Flags argument don't require value.

|  |  | Flag | Description |
|---|---|---|---|
| -w | --wiki |  | The url of the wiki to analyze |
| -c | --category |  | The name of the category to analyze |
| -p | --page |  | The name of the page to analyze |
| -o | --file-output | x | Output the analysis to a file with a generated name based on date in the executing folder |
| -v | --verbose | x | Verbose output |
| -s | --silent | x | Produce no output in the console |
| -f | --no-color | x | Disable coloring of the output for terminal having trouble with the coloring of the text |
| -T | --throttle |  | Give a value in second to enable throttling to avoid '429 : Too Many Request' errors. Mainly for YouTube. That will slow down the speed of the too but avoid temporary banning. |

### The 'youtube' mode

Those options are a superset of arguments for the YouTube mode. Cfr. example.

|  |  | Flag | Description |
|---|---|---|---|
| -a | -aggregate | x | Aggregate urls based on VideoId                              |
| -j | --json-output | x | Output the YouTube urls grouped by page in a file in json format for usage by other tools |
| -d | --display | x | Display the list of the collected youtube url and a count |

### The 'archive' mode

Currently in development. TBD

### Multiplatform

##### Supported system

For now, supported systems are:

- Windows 64 bits from 7 and above.
- "Standard" Linux 64 bits like CentOS, Debian, Fedora, Ubuntu, and derivatives

##### Not yet supported system

- MacOS support will be coming soon, after testing can be done.
- Arm and arm64 architecture, both linux and windows, will be supported in the future, for instance for raspberries.
- There's no plan to support x86 architecture for any platform unless explicitly asked, let me know if it's the case. I consider this architecture to be totally outdated in 2023 and only remaining in niche sectors. 
- RHEL systems low to no chances to be supported.

Remarks: I,considered supported platform on which we have feedback of the software working. Tons of platform can be technically already supported, but without guarantee and feedback it works on those systems, I prefer to let them in the "not yet supported system" by caution.

In any case, if you need to build the solution for an architecture or system not supported, cfr "Build for not officially supported system" section at the end of this page.

### Building the tool

Version build through these scripts are portable and require no dependencies. But might be a bit heavy as they embed a lot of the .net framework libraries wit them. They are all "self-contained", meaning the application is a single file, containing all it's required dependencies.

##### Dependencies

Build is meant to be done on windows with using gitbash (normally provided with git). You don't need any IDE like Visual Studio or similar. What you need is:

- Git for Windows: https://git-scm.com/download/win
- Framework .Net Core 3.1: https://dotnet.microsoft.com/en-us/download/dotnet/3.1
- Latest .net framework sdk: https://dotnet.microsoft.com/en-us/download 
- Zip and bzip2 2 need to be added to your gitbash, it's really easy; here's a straight forward tutorial: https://ranxing.wordpress.com/2016/12/13/add-zip-into-git-bash-on-windows/

##### Compilation

Once the dependencies are installed, you're ready to compile by yourself the project.

A script is present in the root folder of the repo named *"multiplateform_build.sh"*, run it through git bash.

He will present you a list of supported architecture and platform, you just need to pick the one that fit your need. 

If you want to bypass the question, you can use the parameter -p and provide the platform you want to target, might be useful in CI/CD context or for other automations. Supported values are available [here](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog). Usage in this case will be for ex: *"./multiplateform_build -p \<plateform\>\"*

The build output is placed in ./output/build/\<plateform\>

A zip containing the build output is placed in ./output/zip/\<plateform\>.zip

###### Remarks

- On linux a 'chmod 755 wikiref' might be required to have it work.
- Sadly, WSL have compatibility issues with the "dotnet" command, prohibiting it to be used.

##### Build for not officially supported system

You can build for 'unofficially supported system' using the -p paramter of the build script and using for a plateform available in the lise [here](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog)

Exemple, building for macOS 13 Ventura ARM 64 : "./multiplateform_build -p osx.13-arm64"
