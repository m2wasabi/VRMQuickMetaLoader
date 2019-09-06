using UnityEngine;

namespace VRM.QuickMetaLoader
{
    public interface IMetaLoader
    {
        VRMMetaObject Read();
        Texture2D LoadThumbnail();
    }
}