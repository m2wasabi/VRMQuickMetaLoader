using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using VRM.QuickMetaLoader.Model;
using Debug = UnityEngine.Debug;

namespace VRM.QuickMetaLoader
{
    internal unsafe struct JobMetaLoaderInternal : IDisposable
    {
        public int Status;
        public int PathCharCount;
        public IntPtr Path;

        public IntPtr ReadCommandPtr;
        public ref ReadCommand ReadCommandReference => ref *(ReadCommand*)ReadCommandPtr;
        public IntPtr ReadHandlePtr;
        public ref ReadHandle ReadHandleReference => ref *(ReadHandle*)ReadHandlePtr;

        public IntPtr MagicBytePtr;
        public byte* MagicBytes => (byte*)MagicBytePtr;

        public IntPtr JsonPtr;
        public uint JsonLength;
        public byte* JsonBytes => (byte*)JsonPtr;

        public IntPtr InterpretMagicAndLoadJsonJobPtr;
        private ref InterpretMagicAndLoadJsonJob InterpretMagicAndLoadJsonJobReference => ref *(InterpretMagicAndLoadJsonJob*)InterpretMagicAndLoadJsonJobPtr;
        public JobHandle InterpretMagicAndLoadJsonJobHandle;

        public IntPtr InterpretMetaAndLoadBinaryJobPtr;
        private ref InterpretMetaAndLoadBinaryJob InterpretMetaAndLoadBinaryJobReference => ref *(InterpretMetaAndLoadBinaryJob*)InterpretMetaAndLoadBinaryJobPtr;
        public JobHandle InterpretMetaAndLoadBinaryJobHandle;

        public IntPtr QuickMetaStructDataStockPtr;
        public ref QuickMetaStruct.DataStock QuickMetaStructDataStockReference => ref *(QuickMetaStruct.DataStock*)QuickMetaStructDataStockPtr;

        public void Dispose()
        {
            if (Path != IntPtr.Zero)
            {
                UnsafeUtility.Free(Path.ToPointer(), Allocator.Persistent);
            }
            if (ReadCommandPtr != IntPtr.Zero)
            {
                UnsafeUtility.Free(ReadCommandPtr.ToPointer(), Allocator.Persistent);
            }
            if (ReadHandlePtr != IntPtr.Zero)
            {
                if (ReadHandleReference.IsValid())
                {
                    ReadHandleReference.Dispose();
                }
                UnsafeUtility.Free(ReadHandlePtr.ToPointer(), Allocator.Persistent);
            }
            if (MagicBytePtr != IntPtr.Zero)
            {
                UnsafeUtility.Free(MagicBytes, Allocator.Persistent);
            }
            if (JsonPtr != IntPtr.Zero)
            {
                UnsafeUtility.Free(JsonBytes, Allocator.Persistent);
            }
            if (InterpretMagicAndLoadJsonJobPtr != IntPtr.Zero)
            {
                UnsafeUtility.Free(InterpretMagicAndLoadJsonJobPtr.ToPointer(), Allocator.Persistent);
            }
            if (QuickMetaStructDataStockPtr != IntPtr.Zero)
            {
                QuickMetaStructDataStockReference.Dispose();
                UnsafeUtility.Free(QuickMetaStructDataStockPtr.ToPointer(), Allocator.Persistent);
            }
        }

        public static JobMetaLoaderInternal* Create(string path)
        {
            var answer = (JobMetaLoaderInternal*)UnsafeUtility.Malloc(sizeof(JobMetaLoaderInternal), 4, Allocator.Persistent);
            *answer = default;
            answer->Status = 1;
            InitializePath(path, answer);
            MallocStatus1(answer);
            StartReadMagicBytes(path, answer);
            ScheduleJob(answer);

            return answer;
        }

        private static void MallocStatus1(JobMetaLoaderInternal* answer)
        {
            answer->ReadCommandPtr = new IntPtr(UnsafeUtility.Malloc(sizeof(ReadCommand), 4, Allocator.Persistent));
            answer->ReadHandlePtr = new IntPtr(UnsafeUtility.Malloc(sizeof(ReadHandle), 4, Allocator.Persistent));
            answer->MagicBytePtr = new IntPtr(UnsafeUtility.Malloc(20, 4, Allocator.Persistent));
            answer->InterpretMagicAndLoadJsonJobPtr = new IntPtr(UnsafeUtility.Malloc(sizeof(InterpretMagicAndLoadJsonJob), 4, Allocator.Persistent));
            answer->InterpretMetaAndLoadBinaryJobPtr = new IntPtr(UnsafeUtility.Malloc(sizeof(InterpretMetaAndLoadBinaryJob), 4, Allocator.Persistent));
            answer->QuickMetaStructDataStockPtr = new IntPtr(UnsafeUtility.Malloc(sizeof(QuickMetaStruct.DataStock), 4, Allocator.Persistent));
        }

        private static void ScheduleJob(JobMetaLoaderInternal* answer)
        {
            answer->InterpretMagicAndLoadJsonJobHandle = (answer->InterpretMagicAndLoadJsonJobReference = new InterpretMagicAndLoadJsonJob(answer)).Schedule(answer->ReadHandleReference.JobHandle);
            answer->InterpretMetaAndLoadBinaryJobHandle = (answer->InterpretMetaAndLoadBinaryJobReference = new InterpretMetaAndLoadBinaryJob(answer)).Schedule(answer->InterpretMagicAndLoadJsonJobHandle);
        }

        private static void StartReadMagicBytes(string path, JobMetaLoaderInternal* answer)
        {
            answer->ReadCommandReference = new ReadCommand
            {
                Buffer = (void*)answer->MagicBytePtr,
                Offset = 0L,
                Size = 20L,
            };
            answer->ReadHandleReference = AsyncReadManager.Read(path, (ReadCommand*)answer->ReadCommandPtr, 1);
        }

        private static void InitializePath(string path, JobMetaLoaderInternal* answer)
        {
            answer->PathCharCount = path.Length;
            answer->Path = new IntPtr(UnsafeUtility.Malloc(sizeof(char) * answer->PathCharCount, 2, Allocator.Persistent));
            fixed (char* ptr = path)
            {
                UnsafeUtility.MemCpy((void*)answer->Path, ptr, answer->PathCharCount * sizeof(char));
            }
        }

        public struct InterpretMetaAndLoadBinaryJob : IJob
        {
            private readonly IntPtr loader;
            private ref JobMetaLoaderInternal MetaLoader => ref *(JobMetaLoaderInternal*)loader;

            public InterpretMetaAndLoadBinaryJob(JobMetaLoaderInternal* loader)
            {
                this.loader = new IntPtr(loader);
            }

            public void Execute()
            {
                ref var handle = ref MetaLoader.ReadHandleReference;
                if (MetaLoader.Status != 2 || !handle.IsValid()) throw new Exception();
                do
                {
                    switch (handle.Status)
                    {
                        case ReadStatus.Complete:
                            handle.Dispose();
                            break;
                        case ReadStatus.Failed:
                            throw new Exception("Read failed");
                        case ReadStatus.InProgress:
                            continue;
                    }
                    break;
                } while (true);
                var bytes0 = MetaLoader.JsonBytes;
                var length0 = MetaLoader.JsonLength;

                if (!FindIndexOf___VRM___(ref bytes0, ref length0))
                {
                    throw new Exception();
                }
                bytes0 ++;
                length0 --;

                var bytes1 = bytes0;
                var length1 = length0;
                if (!ReadExporterVersion(ref bytes0, ref length0, out var strSBytes0, out var strLength0) || !ReadMeta(ref bytes1, ref length1, out var strSBytes1, out var strLength1))
                {
                    throw new Exception();
                }
                MetaLoader.QuickMetaStructDataStockReference = JsonUtility.FromJson<QuickMetaStruct>(new string(strSBytes1, 0, strLength1));
                ref var metaLoaderQuickMetaStructDataStockReference = ref MetaLoader.QuickMetaStructDataStockReference;
                (metaLoaderQuickMetaStructDataStockReference.ExporterVersionPtr, metaLoaderQuickMetaStructDataStockReference.ExporterVersionLength) = (new IntPtr(strSBytes0), strLength0);

                MetaLoader.Status = 3;

                var textureIndex = metaLoaderQuickMetaStructDataStockReference.Texture;
                if (textureIndex < 0) return;

                var bytes2 = MetaLoader.JsonBytes;
                var length2 = MetaLoader.JsonLength;

                if (!FindIndexOf___textures(bytes2, length2, textureIndex, out var start, out var byteLen))
                    throw new Exception();
                var textureString = new string((sbyte*)start, 0, byteLen);
                var tex = JsonUtility.FromJson<GltfTextureModel>(textureString);

                if (!FindIndexOf___images(bytes2, length2, tex.source, out start, out byteLen))
                    throw new Exception();
                var imageString = new string((sbyte*)start, 0, byteLen);
                var img = JsonUtility.FromJson<GltfImageModel>(imageString);

                (metaLoaderQuickMetaStructDataStockReference.ThumbnailNamePtr, metaLoaderQuickMetaStructDataStockReference.ThumbnailNameLength) = img.name;

                if (!FindIndexOf___bufferViews(bytes2, length2, img.bufferView, out start, out byteLen))
                    throw new Exception();
                var bufferViewString = new string((sbyte*)start, 0, byteLen);
                var bufferView = JsonUtility.FromJson<GltfBufferViewModel>(bufferViewString);

                metaLoaderQuickMetaStructDataStockReference.BinaryOffset = bufferView.byteOffset;
                metaLoaderQuickMetaStructDataStockReference.BinaryLength = bufferView.byteLength;
            }
        }

        public struct InterpretMagicAndLoadJsonJob : IJob
        {
            private readonly IntPtr loader;
            private ref JobMetaLoaderInternal MetaLoader => ref *(JobMetaLoaderInternal*)loader;

            public InterpretMagicAndLoadJsonJob(JobMetaLoaderInternal* loader)
            {
                this.loader = new IntPtr(loader);
            }

            public void Execute()
            {
                if (MetaLoader.Status != 1) throw new Exception("Status should be 1");

                MetaLoader.ReadHandleReference.Dispose();
                var uints = (uint*)MetaLoader.MagicBytes;
                if (*uints != 0x46546c67U) throw new Exception("Magic Word should be 'glTF'");
                if (uints[1] != UniGLTF.glbImporter.GLB_VERSION) throw new Exception("Version mismatch");
                if (uints[4] != 0x4e4f534aU) throw new Exception("Content-Type should be 'JSON'");
                MetaLoader.JsonLength = uints[3];
                UnsafeUtility.Free(MetaLoader.MagicBytes, Allocator.Persistent);
                MetaLoader.MagicBytePtr = IntPtr.Zero;

                MetaLoader.JsonPtr = new IntPtr(UnsafeUtility.Malloc(MetaLoader.JsonLength, 1, Allocator.Persistent));
                var path = new string((char*)MetaLoader.Path, 0, MetaLoader.PathCharCount);
                MetaLoader.ReadCommandReference = new ReadCommand()
                {
                    Buffer = MetaLoader.JsonBytes,
                    Offset = 20,
                    Size = MetaLoader.JsonLength,
                };
                MetaLoader.ReadHandleReference = AsyncReadManager.Read(path, (ReadCommand*)MetaLoader.ReadCommandPtr, 1);

                MetaLoader.Status = 2;
            }
        }


        private static bool FindIndexOf___VRM___(ref byte* bytes, ref uint length)
        {
            if (!FindIndexOf(ref bytes, ref length, 0x4d525622, 0x3a22, 0x7b)) // "VRM ": {
            {
                Debug.LogWarning(@"'""VRM"":{' not found");
                return false;
            }
            return true;
        }

        private static bool ReadMeta(ref byte* bytes, ref uint length, out sbyte* strSBytes, out int strLength)
        {
            strSBytes = default;
            strLength = default;
            if (!FindIndexOf___meta(ref bytes, ref length))
            {
                return false;
            }
            bytes ++;
            length --;
            strSBytes = (sbyte*)bytes;
            var tmpLength = length;
            if (!FindIndexOf(ref bytes, ref length, 0x7d)) // }
            {
                return false;
            }
            strLength = (int)(tmpLength - length) + 1;
            bytes++;
            length--;
            return true;
        }

        private static bool FindIndexOf___meta(ref byte* bytes, ref uint length)
        {
            if (!FindIndexOf(ref bytes, ref length, 0x74656d22U, 0x2261, 0x3a)) // "meta":
            {
                Debug.LogWarning(@"'""meta"":' not found");
                return false;
            }
            return true;
        }

        private static bool FindIndexOf___exporterVersion(ref byte* bytes, ref uint length)
        {
            if (!FindIndexOf(ref bytes, ref length, 0x6574726f70786522UL, 0x6e6f697372655672UL, 0x3a22, 0x22)) // "exporterVersion":"
            {
                Debug.LogWarning(@"'""exporterVersion"":""' not found");
                return false;
            }
            return true;
        }

        private static bool ReadExporterVersion(ref byte* bytes, ref uint length, out sbyte* strSBytes, out int strLength)
        {
            strSBytes = default;
            strLength = default;
            if (!FindIndexOf___exporterVersion(ref bytes, ref length))
            {
                return false;
            }
            bytes ++;
            length --;
            strSBytes = (sbyte*)bytes;
            var tmpLength = length;
            if (!FindIndexOf(ref bytes, ref length, 0x22))
            {
                return false;
            }
            strLength = (int)(tmpLength - length);
            bytes++;
            length--;
            return true;
        }

        private static bool FindIndexOf(ref byte* bytes, ref uint length, ulong first8, uint second4, ushort third2, byte last)
        {
            const uint count = 15U;
            for (; length >= count; bytes++, length--)
            {
                if (*(ulong*)bytes == first8 && *(uint*)(bytes + 8) == second4 && *(ushort*)(bytes + 12) == third2)
                {
                    bytes += count - 1;
                    length -= count - 1;
                    while(length >= count && (*bytes == 0x20 || *bytes == 0x0d || *bytes == 0x0a ||  *bytes == 0x09))
                    {
                        bytes++;
                        length--;
                    }

                    if (*bytes == last)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool FindIndexOf(ref byte* bytes, ref uint length, ulong first8, ulong second8, ushort third2, byte last)
        {
            const uint count = 19U;
            for (; length >= count; bytes++, length--)
            {
                if (*(ulong*)bytes == first8 && *(ulong*)(bytes + 8) == second8 && *(ushort*)(bytes + 16) == third2)
                {
                    bytes += count - 1;
                    length -= count - 1;
                    while(length >= count && (*bytes == 0x20 || *bytes == 0x0d || *bytes == 0x0a ||  *bytes == 0x09))
                    {
                        bytes++;
                        length--;
                    }

                    if (*bytes == last)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool FindIndexOf(ref byte* bytes, ref uint length, uint first4, ushort second2, byte last)
        {
            const uint count = 7U;
            for (; length >= count; bytes++, length--)
            {
                if (*(uint*)bytes == first4 && *(ushort*)(bytes + 4) == second2)
                {
                    bytes += count - 1;
                    length -= count - 1;
                    while(length >= count && (*bytes == 0x20 || *bytes == 0x0d || *bytes == 0x0a ||  *bytes == 0x09))
                    {
                        bytes++;
                        length--;
                    }

                    if (*bytes == last)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool FindIndexOf(ref byte* bytes, ref uint length, ulong first8, ushort second2, byte third1, byte last)
        {
            for (; length >= 12U; bytes++, length--)
            {
                if (*(ulong*)bytes == first8 && *(ushort*)(bytes + 8) == second2 && *(bytes + 10) == third1)
                {
                    bytes += 11U;
                    length -= 11U;
                    while(length >= 12U && (*bytes == 0x20 || *bytes == 0x0d || *bytes == 0x0a ||  *bytes == 0x09))
                    {
                        bytes++;
                        length--;
                    }

                    if (*bytes == last)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool FindIndexOf(ref byte* bytes, ref uint length, ulong first8, byte second1, byte last)
        {
            for (; length >= 10U; bytes++, length--)
            {
                if (*(ulong*)bytes == first8 && *(bytes + 8) == second1)
                {
                    bytes += 9U;
                    length -= 9U;
                    while(length >= 10U && (*bytes == 0x20 || *bytes == 0x0d || *bytes == 0x0a ||  *bytes == 0x09))
                    {
                        bytes++;
                        length--;
                    }

                    if (*bytes == last)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool FindIndexOf(ref byte* bytes, ref uint length, byte value)
        {
            for (; length >= 1U; bytes++, length--)
            {
                if (*bytes == value)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ReadBinaryData(byte* bytes, uint length, out byte* binaryStart, out long binaryLength, out long chunkSize)
        {
            binaryStart = bytes;
            binaryLength = length;
            if (binaryLength < 4L)
            {
                chunkSize = default;
                return false;
            }
            chunkSize = *(int*)binaryStart;
            binaryStart += 4;
            binaryLength -= 4L;

            if (binaryLength < 4L) return false;
            var chunkTypeBytes = *(uint*)binaryStart;
            if (chunkTypeBytes != 0x4e4942)
            {
                Debug.LogWarning("unknown chunk type: " + new string((sbyte*)binaryStart, 0, 3));
                return false;
            }
            binaryStart += 4;
            binaryLength -= 4L;
            return true;
        }

        private static bool FindIndexOf___bufferViews(byte* bytes, uint length, int imageIndex, out byte* start, out int byteLen) // "bufferViews":[
        {
            start = default;
            byteLen = default;
            if (!FindIndexOf(ref bytes, ref length, 0x5672656666756222UL, 0x73776569U, 0x3a22, 0x5b)) // "bufferViews":[
            {
                Debug.LogWarning(@"'""bufferViews"":[' not found");
                return false;
            }
            return FindTargetIndexItem(bytes, length, imageIndex, ref start, ref byteLen);
        }
        private static bool FindIndexOf___images(byte* bytes, uint length, int imageIndex, out byte* start, out int byteLen) // "images":[
        {
            start = default;
            byteLen = default;
            if (!FindIndexOf(ref bytes, ref length, 0x22736567616d6922UL, 0x3a, 0x5b)) // "images":[
            {
                Debug.LogWarning(@"'""images"":[' not found");
                return false;
            }
            return FindTargetIndexItem(bytes, length, imageIndex, ref start, ref byteLen);
        }
        private static bool FindIndexOf___textures(byte* bytes, uint length, int textureIndex, out byte* start, out int byteLen) // "textures":[
        {
            start = default;
            byteLen = default;
            if (!FindIndexOf(ref bytes, ref length, 0x6572757478657422UL, 0x2273, 0x3a, 0x5b)) // "textures":[
            {
                Debug.LogWarning(@"'""textures"":[' not found");
                return false;
            }
            return FindTargetIndexItem(bytes, length, textureIndex, ref start, ref byteLen);
        }

        private static bool FindTargetIndexItem(byte* bytes, uint length, int index, ref byte* start, ref int byteLen)
        {
            for (var i = 0; i <= index; i++)
            {
                if (!FindIndexOf(ref bytes, ref length, (byte)'{'))
                {
                    return false;
                }
                start = bytes;
                bytes++;
                length--;
            }
            if (!FindIndexOf(ref bytes, ref length, (byte)'}'))
            {
                return false;
            }
            byteLen = (int)(new IntPtr(bytes).ToInt64() - new IntPtr(start).ToInt64() + 1);
            return true;
        }
    }
}