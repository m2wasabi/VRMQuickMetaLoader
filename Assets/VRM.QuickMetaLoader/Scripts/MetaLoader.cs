using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace VRM.QuickMetaLoader
{
    public sealed class MetaLoader : IDisposable, IMetaLoader
    {
        private MetaLoaderInternal metaLoader;
        private ulong handle;

        public unsafe MetaLoader(byte[] array)
        {
            metaLoader = new MetaLoaderInternal((byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out handle), array.LongLength);
        }

        public VRMMetaObject Read()
        {
            var meta = ScriptableObject.CreateInstance<VRMMetaObject>();
            metaLoader.Read(meta, false);
            return meta;
        }

        public VRMMetaObject Read(bool createThumbnail)
        {
            var meta = ScriptableObject.CreateInstance<VRMMetaObject>();
            metaLoader.Read(meta, createThumbnail);
            return meta;
        }

        public Texture2D LoadThumbnail() => metaLoader.LoadThumbnail();

        public void Dispose()
        {
            if (handle == 0) return;
            UnsafeUtility.ReleaseGCObject(handle);
            handle = default;
        }
    }
}