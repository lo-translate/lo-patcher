## Patching

[Watch a GIF](applying-process.md) (Warning: 10MB GIF)

1. Copy `__data` from `<PHONE>/Android/data/com.pig.laojp.aos/files/UnityCache/Shared/testassets/<RANDOM_ID>/__data` to a folder on your computer.
2. Open `__data` in [UABE](https://github.com/DerPopo/UABE/releases/). When it asks if you want to unpack the bundle say 'Yes' and save it as `__data.unpacked`
3. Open `__data.unpacked` in UABE (it should already be open if told UABE to unpack).
4. In UABE: Using the 'Info' dialog and 'Extract Raw' button, extract `data.bin` to a file named `data.bin` and `LocalizationPatch` to a file named `LocalizationPatch`. It is easier to find them if you sort by the 'Name' column. The exported file name does not matter but makes it easier to deal with than UABE's auto generated one.
5. In Patcher: Open both `data.bin` and `LocalizationPatch` and click 'Start Patching' then wait for it to complete. Patching may take a few minutes.
6. In UABE: Using the 'Info' dialog and the 'Import Raw' button, import the patched files (`data.bin.patched` and `LocalizationPatch.patched`) replacing the exported versions. You must select the file you want to replace in the assets list before clicking the 'Import Raw' button and they must be done one at a time.
7. In UABE: After replacing both files click OK.  UABE will ask if you want to save the changes, click 'Yes'. You should be at the UABE main screen now, save the file to `__data.patched` using the 'File > Save' menu.
8. Optional: To save space on your phone, in UABE use the 'File > Compress' menu. Open `__data.patched`, don't change any options (LZ4 in all settings) and click OK, then save the file to `__data.compressed`.
9. Copy `__data.patched` (or `__data.compressed` if step 8 was followed) to the phone directory, delete the old `__data` and remove the `.patched` or `.compressed` from the copied file name.

If you run into issues launching the game delete `__data` from your phone and it will be re-downloaded the next time the game is launched.
