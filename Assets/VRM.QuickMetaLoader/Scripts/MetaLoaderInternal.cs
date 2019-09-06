using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using VRM.QuickMetaLoader.Model;

namespace VRM.QuickMetaLoader
{
    public unsafe struct MetaLoaderInternal
    {
        private byte* bytes;
        private long length;
        private byte* binaryStart;
        private long binaryRestLength;
        private long binaryChunkSize;
        private int texture;

        public MetaLoaderInternal(byte* bytes, long length)
        {
            if (bytes == null) throw new ArgumentNullException();
            if (length <= 0) throw new ArgumentOutOfRangeException(length + " : length shouldn't be minus!");
            this = default;
            this.bytes = bytes;
            this.length = length;
        }

        private bool Initialize()
        {
            return ValidateMagicWords(ref bytes, ref length) &&
                ValidateVersion(ref bytes, ref length) &&
                ReadTotalLength(ref bytes, ref length, out _) &&
                ReadChunkLength(ref bytes, ref length, out var chunkLength) &&
                ValidateChunkTypeWhetherToBeJson(ref bytes, ref length) &&
                ReadBinaryData(bytes + chunkLength, length - chunkLength, out binaryStart, out binaryRestLength, out binaryChunkSize);
        }

        public bool Read(VRMMetaObject metaObject, bool createThumbnail = false)
        {
            if (!Initialize()) throw new ArgumentException();

            var bytes0 = bytes;
            var length0 = length;
            if (!FindIndexOf___VRM___(ref bytes0, ref length0)) throw new InvalidDataException();
            bytes0 += 7;
            length0 -= 7;
            var bytes1 = bytes0;
            var length1 = length0;
            if (!ReadExporterVersion(ref bytes0, ref length0, out var strSBytes0, out var strLength0))
                return false;
            if (!ReadMeta(ref bytes1, ref length1, out var strSBytes1, out var strLength1))
                return false;
            metaObject.ExporterVersion = new string(strSBytes0, 0, strLength0);
            var metaString = new string(strSBytes1, 0, strLength1);
            var QMeta = JsonUtility.FromJson<QuickMetaObject>(metaString);
            QMeta.PushMeta(ref metaObject);
            texture = QMeta.texture;

            if (createThumbnail)
            {
                metaObject.Thumbnail = LoadThumbnail();
            }

            return true;
        }

        public Texture2D LoadThumbnail()
        {
            if (texture < 0) return null;
            var kvp = LoadThumbnailBin(texture);

            var thumbnail = new Texture2D(2, 2);
            thumbnail.LoadImage(kvp.Value);
            thumbnail.name = kvp.Key;

            return thumbnail;
        }

        private KeyValuePair<string, byte[]> LoadThumbnailBin(int textureIndex)
        {
            if (!FindIndexOf___textures(bytes, length, textureIndex, out var start, out var byteLen))
                throw new Exception();
            var textureString = new string((sbyte*)start, 0, byteLen);
            var tex = JsonUtility.FromJson<GltfTextureModel>(textureString);


            if (!FindIndexOf___images(bytes, length, tex.source, out start, out byteLen))
                throw new Exception();
            var imageString = new string((sbyte*)start, 0, byteLen);
            var img = JsonUtility.FromJson<GltfImageModel>(imageString);

            if (!FindIndexOf___bufferViews(bytes, length, img.bufferView, out start, out byteLen))
                throw new Exception();
            var bufferViewString = new string((sbyte*)start, 0, byteLen);
            var bufferView = JsonUtility.FromJson<GltfBufferViewModel>(bufferViewString);

            var buffer = new byte[bufferView.byteLength];
            fixed (byte* dest = &buffer[0])
            {
                UnsafeUtility.MemCpy(dest, binaryStart + bufferView.byteOffset, bufferView.byteLength);
            }

            return new KeyValuePair<string, byte[]>(img.name, buffer);
        }


        private static bool ReadMeta(ref byte* bytes, ref long length, out sbyte* strSBytes, out int strLength)
        {
            strSBytes = default;
            strLength = default;
            if (!FindIndexOf___meta(ref bytes, ref length))
            {
                return false;
            }
            bytes += 7;
            length -= 7L;
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

        private static bool FindIndexOf___meta(ref byte* bytes, ref long length)
        {
            if (!FindIndexOf(ref bytes, ref length, 0x74656d22U, 0x2261, 0x3a)) // "meta":
            {
                Debug.LogWarning(@"'""meta"":' not found");
                return false;
            }
            return true;
        }


        private static bool FindIndexOf___bufferViews(byte* bytes, long length, int imageIndex, out byte* start, out int byteLen) // "bufferViews":[
        {
            start = default;
            byteLen = default;
            if (!FindIndexOf(ref bytes, ref length, 0x5672656666756222UL, (uint)0x73776569U, (ushort)0x3a22, (byte)0x5b)) // "bufferViews":[
            {
                Debug.LogWarning(@"'""bufferViews"":[' not found");
                return false;
            }
            return 抜粋(bytes, length, imageIndex, ref start, ref byteLen);
        }
        private static bool FindIndexOf___images(byte* bytes, long length, int imageIndex, out byte* start, out int byteLen) // "images":[
        {
            start = default;
            byteLen = default;
            if (!FindIndexOf(ref bytes, ref length, 0x22736567616d6922UL, (ushort)0x5b3a)) // "images":[
            {
                Debug.LogWarning(@"'""images"":[' not found");
                return false;
            }
            return 抜粋(bytes, length, imageIndex, ref start, ref byteLen);
        }
        private static bool FindIndexOf___textures(byte* bytes, long length, int textureIndex, out byte* start, out int byteLen) // "textures":[
        {
            start = default;
            byteLen = default;
            if (!FindIndexOf(ref bytes, ref length, 0x6572757478657422UL, 0x5b3a2273U)) // "textures":[
            {
                Debug.LogWarning(@"'""textures"":[' not found");
                return false;
            }
            return 抜粋(bytes, length, textureIndex, ref start, ref byteLen);
        }

        private static bool 抜粋(byte* bytes, long length, int index, ref byte* start, ref int byteLen)
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

        private static bool FindIndexOf___VRM___(ref byte* bytes, ref long length)
        {
            if (!FindIndexOf(ref bytes, ref length, 0x4d525622, 0x3a22, 0x7b)) // "VRM ": {
            {
                Debug.LogWarning(@"'""VRM"":{' not found");
                return false;
            }
            return true;
        }

        private static bool FindIndexOf___exporterVersion(ref byte* bytes, ref long length)
        {
            if (!FindIndexOf(ref bytes, ref length, 0x6574726f70786522UL, 0x6e6f697372655672UL, 0x3a22, 0x22)) // "exporterVersion":"
            {
                Debug.LogWarning(@"'""exporterVersion"":""' not found");
                return false;
            }
            return true;
        }

        private static bool ReadExporterVersion(ref byte* bytes, ref long length, out sbyte* strSBytes, out int strLength)
        {
            strSBytes = default;
            strLength = default;
            if (!FindIndexOf___exporterVersion(ref bytes, ref length))
            {
                return false;
            }
            bytes += 19;
            length -= 19L;
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

        private static bool FindIndexOf(ref byte* bytes, ref long length, ulong first8, uint second4, ushort third2, byte last)
        {
            const long count = 15L;
            for (; length >= count; bytes++, length--)
            {
                if (*(ulong*)bytes == first8 && *(uint*)(bytes + 8) == second4 && *(ushort*)(bytes + 12) == third2 && bytes[count - 1L] == last)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool FindIndexOf(ref byte* bytes, ref long length, ulong first8, ulong second8, ushort third2, byte last)
        {
            const long count = 19L;
            for (; length >= count; bytes++, length--)
            {
                if (*(ulong*)bytes == first8 && *(ulong*)(bytes + 8) == second8 && *(ushort*)(bytes + 16) == third2 && bytes[count - 1L] == last)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool FindIndexOf(ref byte* bytes, ref long length, uint first4, ushort second2, byte last)
        {
            const long count = 7L;
            for (; length >= count; bytes++, length--)
            {
                if (*(uint*)bytes == first4 && *(ushort*)(bytes + 4) == second2 && bytes[count - 1L] == last)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool FindIndexOf(ref byte* bytes, ref long length, ulong first8, uint last)
        {
            for (; length >= 12L; bytes++, length--)
            {
                if (*(ulong*)bytes == first8 && *(uint*)(bytes + 8) == last)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool FindIndexOf(ref byte* bytes, ref long length, ulong first8, ushort last)
        {
            for (; length >= 10L; bytes++, length--)
            {
                if (*(ulong*)bytes == first8 && *(ushort*)(bytes + 8) == last)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool FindIndexOf(ref byte* bytes, ref long length, byte value)
        {
            for (; length >= 1L; bytes++, length--)
            {
                if (*bytes == value)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ReadBinaryData(byte* bytes, long length, out byte* binaryStart, out long binaryLength, out long chunkSize)
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

        private static bool ValidateChunkTypeWhetherToBeJson(ref byte* bytes, ref long length)
        {
            if (length < 4)
            {
                Debug.LogWarning("Contents should contain chunk type name");
                return false;
            }
            if (*(uint*)bytes != 0x4e4f534a)
            {
                Debug.LogWarning("unknown chunk type:" + new string((sbyte*)bytes, 0, 4));
                return false;
            }
            bytes += 4;
            length -= 4L;
            return true;
        }

        private static bool ReadChunkLength(ref byte* bytes, ref long length, out uint chunkLength)
        {
            if (length < 4)
            {
                Debug.LogWarning("Contents should contain chunk length");
                chunkLength = default;
                return false;
            }
            chunkLength = *(uint*)bytes;
            bytes += 4;
            length -= 4L;
            return true;
        }

        private static bool ReadTotalLength(ref byte* bytes, ref long length, out uint totalLength)
        {
            if (length < 4)
            {
                Debug.LogWarning("Contents should contain total length");
                totalLength = default;
                return false;
            }
            totalLength = *(uint*)bytes;
            bytes += 4;
            length -= 4L;
            return true;
        }

        private static bool ValidateVersion(ref byte* bytes, ref long length)
        {
            if (length < 4)
            {
                Debug.LogWarning("Contents should contain version number");
                return false;
            }
            if (*(uint*)bytes != UniGLTF.glbImporter.GLB_VERSION)
            {
                Debug.LogWarning("Unknown Version: " + *(uint*)bytes);
            }
            bytes += 4;
            length -= 4L;
            return true;
        }

        private static bool ValidateMagicWords(ref byte* bytes, ref long length)
        {
            if (length < 4)
            {
                Debug.LogWarning("Contents should contain MAGIC BYTES!");
                return false;
            }
            if (*(uint*)bytes != 0x46546c67)
            {
                Debug.LogWarning("MAGIC BYTES is different from original bytes.");
                return false;
            }
            bytes += 4;
            length -= 4L;
            return true;
        }
    }
}