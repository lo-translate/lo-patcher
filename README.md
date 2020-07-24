# Lo Patcher

The patcher will decompress bundle files but will not re-compress them. If you
need them compressed to save space use [UABE](https://github.com/DerPopo/UABE/)
after patching.

## Steps to build patcher

 * Clone repo
 * Init submodules: `git submodule update --init`
 * Apply [patch](.docs/extract-string-as-bytes.patch) to AssetsTools to support extraction of binary script asset
 * Open solution and re-install NuGet packages to fix paths in AssetsTools project: `Update-Package Mono.Cecil -Reinstall` (From NuGet package manager console)

## License

[CC0 1.0 Universal](LICENSE.md)
