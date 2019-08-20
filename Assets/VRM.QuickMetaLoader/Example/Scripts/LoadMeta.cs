using System.Collections;
using System.Collections.Generic;
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
        var bytes = File.ReadAllBytes("AliciaSolid_1.10.vrm");
        stopwatch.Start();
        var meta = MetaLoader.Read(bytes);
        stopwatch.Stop();
        float elapsed = (float)stopwatch.Elapsed.TotalSeconds;
        Debug.Log("QuickMetaLoader: " + elapsed);
        ViewMeta(meta);
        
        var context = new VRMImporterContext();
        stopwatch.Restart();
        context.ParseGlb(bytes);
        stopwatch.Stop();
        float elapsed2 = (float)stopwatch.Elapsed.TotalSeconds;
        Debug.Log("VRM ParseGlb: " + elapsed2);

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
