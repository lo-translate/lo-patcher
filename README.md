# Lo Patcher

Patching the asset bundle (`__data`) directly is not currently supported. You must extract the text assets first before patching them, see instructions below.

## Steps to patch

 * Usung [UABE](https://github.com/DerPopo/UABE/releases/) extract `data.bin` and `LocalizationPath` from `Android/data/com.pig.laojp.aos/files/UnityCache/Shared/textassets/[RANDOM_ID]/__data`
 * Patch them using the patcher.
 * Import the patched files back in to UABE and save the bundle.
 
[Detailed instructions](.docs/applying-patch.md)

## Steps to build patcher

 * Clone repo
 * Init submodules `git submodule update --init`
 * Open solution and re-install NuGet packages to fix paths `Update-Package Mono.Cecil -Reinstall` (From NuGet package manager console)

## License

[CC0 1.0 Universal](LICENSE.md)
