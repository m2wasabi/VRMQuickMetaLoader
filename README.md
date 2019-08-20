# VRMQuickMetaLoader
load quick VRM.Meta information

![Demo](Docs/images/demo_ss01.png)

## Usage

### Just Simple.

```csharp
var bytes = File.ReadAllBytes(file);
var metaLoader = new VRM.QuickMetaLoader.MetaLoader(bytes);
VRMMetaObject meta = metaLoader.Read(true);
```

And, you got VRMMetaObject with thumbnail.

### Thumbnail on your hands

```csharp
var bytes = File.ReadAllBytes(file);
var metaLoader = new VRM.QuickMetaLoader.MetaLoader(bytes);
VRMMetaObject meta = metaLoader.Read();  // without thumbnail but fast

// some process...

Texture2D thumbnail = metaLoader.LoadThumbnail();
```


## Dependencies

 + UniVRM 0.53.0 : [https://github.com/vrm-c/UniVRM/releases](https://github.com/vrm-c/UniVRM/releases)

## Also recommended

 + VRMLoaderUI : [https://github.com/m2wasabi/VRMLoaderUI](https://github.com/m2wasabi/VRMLoaderUI)
