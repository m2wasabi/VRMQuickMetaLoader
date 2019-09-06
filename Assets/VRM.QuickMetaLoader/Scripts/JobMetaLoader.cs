using System;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using VRM.QuickMetaLoader.Model;

namespace VRM.QuickMetaLoader
{
    public unsafe struct JobMetaLoader : IDisposable, IMetaLoader
    {
        private string path;
        private JobMetaLoaderInternal* ptr;
        private byte[] thumbnailBytes;
        private ulong thumbnailHandle;
        private readonly bool preloadThumbnail;

        public JobMetaLoader(string path, bool preloadThumbnail = true)
        {
            thumbnailBytes = null;
            thumbnailHandle = default;
            this.path = path;
            ptr = JobMetaLoaderInternal.Create(path);
            this.preloadThumbnail = preloadThumbnail;
        }

        public JobHandle LoadJsonJobHandle => ptr->InterpretMagicAndLoadJsonJobHandle;

        public JobHandle InterpretMetaJobHandle => ptr->InterpretMetaAndLoadBinaryJobHandle;

        public JobHandle LoadThumbnailJobHandle => ptr->ReadHandleReference.JobHandle;

        public VRMMetaObject Read()
        {
            var metaObject = ScriptableObject.CreateInstance<VRMMetaObject>();

            ptr->InterpretMetaAndLoadBinaryJobHandle.Complete();
            if (ptr->Status != 3) throw new Exception();
            ref var dataStock = ref ptr->QuickMetaStructDataStockReference;
            dataStock.PushMeta(metaObject);

            if (!preloadThumbnail) return metaObject;

            var length = dataStock.BinaryLength;
            if (length == 0) return metaObject;

            thumbnailBytes = new byte[length];
            ptr->ReadCommandReference = new ReadCommand
            {
                Buffer = UnsafeUtility.PinGCArrayAndGetDataAddress(thumbnailBytes, out thumbnailHandle),
                Size = length,
                Offset = 28 + ptr->JsonLength + dataStock.BinaryOffset,
            };
            ptr->ReadHandleReference = AsyncReadManager.Read(path, (ReadCommand*)ptr->ReadCommandPtr, 1);
            ptr->Status = 4;
            return metaObject;
        }

        public Texture2D LoadThumbnail()
        {
            ref var dataStock = ref ptr->QuickMetaStructDataStockReference;
            if (preloadThumbnail)
            {
                return CreateThumbnailPreloaded(ref dataStock);
            }
            return CreateThumbnailSync(dataStock);
        }

        private Texture2D CreateThumbnailSync(QuickMetaStruct.DataStock dataStock)
        {
            var length = dataStock.BinaryLength;
            if (length == 0) return null;

            thumbnailBytes = new byte[length];
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
            {
                stream.Seek(28 + ptr->JsonLength + dataStock.BinaryOffset, SeekOrigin.Begin);
                stream.Read(thumbnailBytes, 0, length);
            }
            return CreateThumbnail(ref dataStock);
        }

        private Texture2D CreateThumbnailPreloaded(ref QuickMetaStruct.DataStock dataStock)
        {
            if (dataStock.Texture < 0) return null;
            if (ptr->Status != 4) throw new InvalidOperationException(ptr->Status.ToString());

            ptr->ReadHandleReference.JobHandle.Complete();
            if (ptr->ReadHandleReference.Status != ReadStatus.Complete) throw new Exception();

            ptr->ReadHandleReference.Dispose();
            UnsafeUtility.ReleaseGCObject(thumbnailHandle);
            thumbnailHandle = default;

            return CreateThumbnail(ref dataStock);
        }

        private Texture2D CreateThumbnail(ref QuickMetaStruct.DataStock dataStock)
        {
            var thumbnail = new Texture2D(2, 2);
            thumbnail.LoadImage(thumbnailBytes);
            thumbnail.name = new string((char*)dataStock.ThumbnailNamePtr, 0, dataStock.ThumbnailNameLength);
            thumbnailBytes = null;

            ptr->Status = 5;

            return thumbnail;
        }

        public void Dispose()
        {
            if (ptr != null)
            {
                ptr->Dispose();
                UnsafeUtility.Free(ptr, Allocator.Persistent);
            }
            path = null;
            thumbnailBytes = null;
            if (thumbnailHandle != 0)
            {
                UnsafeUtility.ReleaseGCObject(thumbnailHandle);
            }
        }
    }
}