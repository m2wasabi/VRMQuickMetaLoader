using System;

namespace VRM.QuickMetaLoader.Model
{
    [Serializable]
    public class GltfTextureModel
    {
        public int sampler;
        public int source;
    }

    [Serializable]
    public class GltfImageModel
    {
        public string name;
        public int bufferView;
        public string mimeType;
    }

    [Serializable]
    public class GltfBufferViewModel
    {
        public int buffer;
        public int byteOffset;
        public int byteLength;
        public int target;
    }
}