# VRMQuickMetaLoader
load quick VRM.Meta information

![Demo](Docs/images/demo_ss01.png)

## Usage

### Just Simple.

```csharp
var bytes = File.ReadAllBytes(file);
VRMMetaObject meta;
using(var metaLoader = new VRM.QuickMetaLoader.MetaLoader(bytes, false))
{
	meta = metaLoader.Read();
}
```

And, you got VRMMetaObject with thumbnail.

### Thumbnail on your hands

```csharp
var bytes = File.ReadAllBytes(file);
using(var metaLoader = new VRM.QuickMetaLoader.MetaLoader(bytes))
{
	VRMMetaObject meta = metaLoader.Read();

	// some process...

	Texture2D thumbnail = metaLoader.LoadThumbnail();
}
```

### Job Read

You can also use AsyncReadManager Read.

```csharp
using(var metaLoader = new JobMetaLoader(file, preloadThumbnail: true))
{
	VRMMetaObject meta = metaLoader.Read();

	// some process...

	meta.Thumbnail =  metaLoader.LoadThumbnail();
}
```

## License

MIT License

## Download

Go [GitHub Release page](https://github.com/m2wasabi/VRMQuickMetaLoader/releases)
  and get .unitypackage

## Dependencies

 + UniVRM 0.53.0 : [https://github.com/vrm-c/UniVRM/releases](https://github.com/vrm-c/UniVRM/releases)

## Also recommended

 + VRMLoaderUI : [https://github.com/m2wasabi/VRMLoaderUI](https://github.com/m2wasabi/VRMLoaderUI)
