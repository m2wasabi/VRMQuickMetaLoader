using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRM;

public class CharacterCard : MonoBehaviour
{
    [SerializeField] private Text title;
    [SerializeField] private Text version;
    [SerializeField] private Text violence;
    [SerializeField] private Text sexual;
    [SerializeField] private Text commercial;
    [SerializeField] private RawImage thumbnail;

    public void SetMetaText(VRMMetaObject meta)
    {
        title.text = meta.Title;
        version.text = meta.Version;
        violence.text = meta.ViolentUssage.ToString();
        sexual.text = meta.SexualUssage.ToString();
        commercial.text = meta.CommercialUssage.ToString();
    }

    public void SetThumbnail(Texture2D tex)
    {
        thumbnail.texture = tex;
    }
}
