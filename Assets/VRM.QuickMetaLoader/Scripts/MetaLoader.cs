using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRM.QuickMetaLoader.Model;

namespace VRM.QuickMetaLoader
{
    public class MetaLoader
    {
        private readonly byte[] _bytes;
        private VRMMetaObject _meta;
        private string _jsonString;
        private int _binOffset;
        private int _textureIndex;

        public MetaLoader(byte[] bytes)
        {
            this._bytes = bytes;
        }
        public VRMMetaObject Read(bool createThumbnail = false)
        {
            if (_bytes.Length == 0)
            {
                throw new Exception("empty bytes");
            }

            var pos = JumpToJsonAddress();
            if (pos < 0) return null;

            var vrmMetaObject = ByteReadMetaData(pos);
            if (createThumbnail)
            {
                vrmMetaObject.Thumbnail = LoadThumbnail();
            }
            
            return vrmMetaObject;
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

            pos += 4;
            var jsonPos = pos;
            
            return jsonPos;
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

            _jsonString = Encoding.UTF8.GetString(_bytes, pos, chunkDataSize);
            GetBinChunk(pos + chunkDataSize);

            var VRMPos = _jsonString.IndexOf("\"VRM\":{", StringComparison.Ordinal);
            var exVerPos = _jsonString.IndexOf("\"exporterVersion\":\"", VRMPos + 6 ,StringComparison.Ordinal);
            var exVerEndPos = _jsonString.IndexOf('"', exVerPos + 19 );
            _meta.ExporterVersion = _jsonString.Substring(exVerPos + 19 , exVerEndPos -19 - exVerPos);
            
            var metaPos = _jsonString.IndexOf("\"meta\":", VRMPos + 6 ,StringComparison.Ordinal);
            var metaEndPos = _jsonString.IndexOf('}', metaPos + 7 );
            var metaString = _jsonString.Substring(metaPos + 7 , metaEndPos - 6 - metaPos);
//            Debug.Log(metaString);
            var QMeta = JsonUtility.FromJson<QuickMetaObject>(metaString);
            QMeta.PushMeta(ref _meta);
            _textureIndex = QMeta.texture;

            return _meta;
        }

        private void GetBinChunk(int offset)
        {
            var pos = offset;
            var chunkDataSize = BitConverter.ToInt32(_bytes, pos);
            pos += 4;

            var chunkTypeBytes = _bytes.Skip(pos).Take(4).Where(x => x != 0).ToArray();
            var chunkTypeStr = Encoding.ASCII.GetString(chunkTypeBytes);
            if (chunkTypeStr != "BIN")
            {
                throw new FormatException("unknown chunk type: " + chunkTypeStr);
            }
            pos += 4;
            _binOffset = pos;
        }

        public Texture2D LoadThumbnail()
        {
            if (_textureIndex < 0) return null;
            var textureString = GetIndexOfJsonArray("\"textures\":[", _textureIndex);
            var tex = JsonUtility.FromJson<GltfTextureModel>(textureString);

            var imageString = GetIndexOfJsonArray("\"images\":[", tex.source);
            var img = JsonUtility.FromJson<GltfImageModel>(imageString);

            var bufferViewString = GetIndexOfJsonArray("\"bufferViews\":[", img.bufferView);
            var bufferView = JsonUtility.FromJson<GltfBufferViewModel>(bufferViewString);

            var buffer = new byte[bufferView.byteLength];
            Buffer.BlockCopy(_bytes,_binOffset + bufferView.byteOffset,buffer, 0, bufferView.byteLength);

            var thumbnail = new Texture2D(2,2);
            thumbnail.LoadImage(buffer);
            thumbnail.name = img.name;
            
            return thumbnail;
        }

        private string GetIndexOfJsonArray(string elementKey, int index)
        {
            var pos = _jsonString.IndexOf(elementKey, StringComparison.Ordinal) + elementKey.Length;
            var elementStartPos = 0;
            for (int i = 0; i <= index; i++)
            {
                elementStartPos = _jsonString.IndexOf('{',pos);
                pos = elementStartPos + 1;
            }
            var elementEndPos = _jsonString.IndexOf('}', elementStartPos );

            return _jsonString.Substring(elementStartPos , elementEndPos + 1 - elementStartPos);
        }
    }
}