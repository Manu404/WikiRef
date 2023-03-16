# WikiRef


WikiRef is a multiplateform tool built to analyze MediaWiki references, identifies errors and archive reference through WaybackMachine service.

In case of errors, like dead link, duplicated references, malformed references, etc, the tool will issue an error or warning message.

<img align="left" src="https://github.com/Manu404/WikiRef/raw/main/icon.png" width="350" height="350">

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

### The 'youtube' mode

Those options are a superset of arguments for the youtube mode. Cfr. example.

|  |  | Flag | Description |
|---|---|---|---|
| -a | -aggregate | x | Aggregate urls refering to the same videos, but for instance pointing to different timestamp |
| -j | --json-output | x | Output the youtube urls grouped by page in json format for usage by other tools |
| -d | --display | x | Display the list of the collected youtube url and a count |

### The 'archive' mode

Currently in development. TBD

### Running on different systems

Version build through these scripts are portable and require no dependencies. But might be a bit heavy as they embed a lot of the .net framework libraries wit them.

For windows, some lighter versions are available, as they don't need to bundle a big part of the .net framework with them.

On linux a 'chmod 755 wikiref' might be required to have it work.

- build_for_win-x64.bat => build compatible for all windows 7 >
- build_for_linux-x64.bat => build compatible for most desktop linux distributions like CentOS, Debian, Fedora, Ubuntu, and derivatives.

### Not yet supported system
- MacOs support is coming, but require a specific script for each specific version of mac os, but will be coming soon.
- Arm and arm64 architecture, both linux and windows, might be supported in the future.
- There's no plan to support x86 architecture for any plateform unless explicitely asked.
- RHEL systems low to no chances to be supported.

In any case, if you need to build the solution for an architecture or system not supported, it's totally open-source and written with .net core. So it can target a lot of plateform jut by changing the "-r" argument of the build script. More informations about supported architecture and plateform here: https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
