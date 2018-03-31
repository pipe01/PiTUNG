# Mod auto-updating

`Mod` classes may have an [`UpdateUrl`][1] property, containing a URL that points to an update manifest in the following format:

```
[mod package name]
Version = 1.0.0
FileName = ModBinary.dll

[another package name]
Version = 1.2.3
FileName = CoolMod.dll
```

The `FileName` entry isn't necessary, although it's strongly recommended. When specified, a file with the specified name will be searched for in the same URL directory as the manifest. E.g.: if the url was `localhost/foo/bar/manifest.ptm` and `FileName = MyMod.dll`, the file `localhost/foo/bar/MyMod.dll` would be downloaded.

A single manifest file can contain as many mod entries as you wish. As an example, [here][2] you can see the update manifest for all my mods.

[1]: ../api/PiTung.Mod.html#PiTung_Mod_UpdateUrl
[2]: http://pipe0481.heliohost.org/pitung/mods/manifest.ptm