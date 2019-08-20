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
        private readonly byte[] _bytes;
        private VRMMetaObject _meta;
        private string _jsonString;
        public MetaLoader(byte[] bytes)
        {
            this._bytes = bytes;
        }
        public VRMMetaObject Read()
        {
            if (_bytes.Length == 0)
            {
                throw new Exception("empty bytes");
            }

            var pos = JumpToJsonAddress();
            if (pos < 0) return null;

            return ByteReadMetaData(pos);
        }

        private int JumpToJsonAddress()
        {
            int pos = 0;
            if (Encoding.ASCII.GetString(_bytes, 0, 4) != UniGLTF.glbImporter.GLB_MAGIC)
            {
                throw new Exception("invalid magic");
            }
            pos += 4;

            var version = BitConverter.ToUInt32(_bytes, pos);
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

        private  VRMMetaObject ByteReadMetaData(int pos)
        {
            _meta = ScriptableObject.CreateInstance<VRMMetaObject>();
            _meta.name = "Meta";

            var chunkDataSize = BitConverter.ToInt32(_bytes, pos);
            pos += 4;

            var chunkTypeBytes = _bytes.Skip(pos).Take(4).Where(x => x != 0).ToArray();
            var chunkTypeStr = Encoding.ASCII.GetString(chunkTypeBytes);
            if (chunkTypeStr != "JSON")
            {
                throw new FormatException("unknown chunk type: " + chunkTypeStr);
            }
            pos += 4;

//            var fp = File.OpenWrite("vrmJson.json");
//            fp.Write(bytes,pos,chunkDataSize);
            _jsonString = Encoding.UTF8.GetString(_bytes, pos, chunkDataSize);

            var VRMPos = _jsonString.IndexOf("\"VRM\":{", StringComparison.Ordinal);
            var exVerPos = _jsonString.IndexOf("\"exporterVersion\":\"", VRMPos + 6 ,StringComparison.Ordinal);
            var exVerEndPos = _jsonString.IndexOf('"', exVerPos + 19 );
            _meta.ExporterVersion = _jsonString.Substring(exVerPos + 19 , exVerEndPos -19 - exVerPos);
            
            var metaPos = _jsonString.IndexOf("\"meta\":", VRMPos + 6 ,StringComparison.Ordinal);
            var metaEndPos = _jsonString.IndexOf('}', metaPos + 7 );
            var metaString = _jsonString.Substring(metaPos + 7 , metaEndPos - 6 - metaPos);
            Debug.Log(metaString);
            var QMeta = JsonUtility.FromJson<QuickMetaObject>(metaString);
            QMeta.PushMeta(ref _meta);

            return _meta;
        }
    }
}