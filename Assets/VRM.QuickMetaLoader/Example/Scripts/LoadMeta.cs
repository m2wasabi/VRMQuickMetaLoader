using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VRM;
using VRM.QuickMetaLoader;

public class LoadMeta : MonoBehaviour
{
    [SerializeField, Header("Info")]
    Text m_textModelTitle;
    [SerializeField]
    Text m_textModelVersion;
    [SerializeField]
    Text m_textModelAuthor;
    [SerializeField]
    Text m_textModelContact;
    [SerializeField]
    Text m_textModelReference;
    [SerializeField]
    RawImage m_thumbnail;

    [SerializeField, Header("CharacterPermission")]
    Text m_textPermissionAllowed;
    [SerializeField]
    Text m_textPermissionViolent;
    [SerializeField]
    Text m_textPermissionSexual;
    [SerializeField]
    Text m_textPermissionCommercial;
    [SerializeField]
    Text m_textPermissionOther;

    [SerializeField, Header("DistributionLicense")]
    Text m_textDistributionLicense;
    [SerializeField]
    Text m_textDistributionOther;

    void Start()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        stopwatch.Start();
        var bytes = File.ReadAllBytes("AliciaSolid_1.10.vrm");
        stopwatch.Stop();
        Debug.Log("ReadAllBytes: " + (float)stopwatch.Elapsed.TotalSeconds + " sec");
        
        stopwatch.Restart();
        var metaLoader = new MetaLoader(bytes);
        var meta = metaLoader.Read();
        stopwatch.Stop();
        Debug.Log("QuickMetaLoader: " + (float)stopwatch.Elapsed.TotalSeconds + " sec");

        ViewMeta(meta);
        LoadIcon(metaLoader);

        stopwatch.Restart();
        var context = new VRMImporterContext();
        context.ParseGlb(bytes);
        stopwatch.Stop();
        Debug.Log("VRM ParseGlb: " + (float)stopwatch.Elapsed.TotalSeconds + " sec");

        stopwatch.Restart();
        context.ReadMeta(true);
        stopwatch.Stop();
        Debug.Log("VRM ReadMeta: " + (float)stopwatch.Elapsed.TotalSeconds + " sec");

    }

    private void LoadIcon(MetaLoader metaLoader)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        var t = metaLoader.LoadAsyncThumbnail();
        m_thumbnail.texture = t;
        stopwatch.Stop();
        Debug.Log("LoadThumbnail: " + (float)stopwatch.Elapsed.TotalSeconds + " sec");
    }
    private void ViewMeta(VRMMetaObject meta)
    {

        m_textModelTitle.text = meta.Title;
        m_textModelVersion.text = meta.Version;
        m_textModelAuthor.text = meta.Author;
        m_textModelContact.text = meta.ContactInformation;
        m_textModelReference.text = meta.Reference;

        m_textPermissionAllowed.text = meta.AllowedUser.ToString();
        m_textPermissionViolent.text = meta.ViolentUssage.ToString();
        m_textPermissionSexual.text = meta.SexualUssage.ToString();
        m_textPermissionCommercial.text = meta.CommercialUssage.ToString();
        m_textPermissionOther.text = meta.OtherPermissionUrl;

        m_textDistributionLicense.text = meta.LicenseType.ToString();
        m_textDistributionOther.text = meta.OtherLicenseUrl;

        m_thumbnail.texture = meta.Thumbnail;
    }
}
