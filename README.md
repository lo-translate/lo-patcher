# Lo Patcher

## Instructions

 * Usung UABE extract `data.bin` and `LocalizationPath` from `Android/data/com.pig.laojp.aos/files/UnityCache/Shared/textassets/[RANDOM_ID]/__data`
 * Choose them in the patcher and click patch.
 * Import the patched files back in to UABE and save the bundle.

## Steps to build

 * Clone repo
 * Init submodules `git submodule update --init`
 * Re-install NuGet packages to fix paths `Update-Package Mono.Cecil -Reinstall` (From NuGet package manager console)
