using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;
using VRM.QuickMetaLoader.Model;

namespace VRM.QuickMetaLoader
{
    public class MetaLoader
    {
        public static VRMMetaObject Read(byte[] bytes, bool createThumbnail = false)
        {
            if (bytes.Length == 0)
            {
                throw new Exception("empty bytes");
            }

            var pos = JumpToJsonAddress(bytes);
            if (pos < 0) return null;

            return ByteReadMetaData(bytes, pos, createThumbnail);
        }

        private static int JumpToJsonAddress(byte[] bytes)
        {
            int pos = 0;
            if (Encoding.ASCII.GetString(bytes, 0, 4) != UniGLTF.glbImporter.GLB_MAGIC)
            {
                throw new Exception("invalid magic");
            }
            pos += 4;

            var version = BitConverter.ToUInt32(bytes, pos);
            if (version != UniGLTF.glbImporter.GLB_VERSION)
            {
                Debug.LogWarningFormat("unknown version: {0}", version);
                return -1;
            }
            pos += 4;

            //var totalLength = BitConverter.ToUInt32(bytes, pos);
            pos += 4;
            return pos;
        }

        private static VRMMetaObject ByteReadMetaData(byte[] bytes, int pos, bool createThumbnail)
        {
            var meta = ScriptableObject.CreateInstance<VRMMetaObject>();
            meta.name = "Meta";

            var chunkDataSize = BitConverter.ToInt32(bytes, pos);
            pos += 4;

            var chunkTypeBytes = bytes.Skip(pos).Take(4).Where(x => x != 0).ToArray();
            var chunkTypeStr = Encoding.ASCII.GetString(chunkTypeBytes);
            if (chunkTypeStr != "JSON")
            {
                throw new FormatException("unknown chunk type: " + chunkTypeStr);
            }
            pos += 4;

//            var fp = File.OpenWrite("vrmJson.json");
//            fp.Write(bytes,pos,chunkDataSize);
            var jsonString = Encoding.UTF8.GetString(bytes, pos, chunkDataSize);

            var VRMPos = jsonString.IndexOf("\"VRM\":{", StringComparison.Ordinal);
            var exVerPos = jsonString.IndexOf("\"exporterVersion\":\"", VRMPos + 6 ,StringComparison.Ordinal);
            var exVerEndPos = jsonString.IndexOf('"', exVerPos + 19 );
            meta.ExporterVersion = jsonString.Substring(exVerPos + 19 , exVerEndPos -19 - exVerPos);
            
            var metaPos = jsonString.IndexOf("\"meta\":", VRMPos + 6 ,StringComparison.Ordinal);
            var metaEndPos = jsonString.IndexOf('}', metaPos + 7 );
            var metaString = jsonString.Substring(metaPos + 7 , metaEndPos - 6 - metaPos);
            Debug.Log(metaString);
            var QMeta = JsonUtility.FromJson<QuickMetaObject>(metaString);
            QMeta.PushMeta(ref meta);


            return meta;
        }
    }
}