using System;

namespace VRM.QuickMetaLoader.Model
{
    [Serializable]
    public class QuickMetaObject
    {
        #region Info
        public string title;
        public string version;
        public string author;
        public string contactInformation;
        public string reference;
        #endregion

        #region Permission
        public string allowedUserName;
        public string violentUssageName;
        public string sexualUssageName;
        public string commercialUssageName;
        public string otherPermissionUrl;
        #endregion

        #region Distribution License
        public string licenseName;

        public string otherLicenseUrl;
        #endregion

        public int texture;

        public void PushMeta(ref VRMMetaObject meta){
            meta.Version = version; // model version
            meta.Author = author;
            meta.ContactInformation = contactInformation;
            meta.Reference = reference;
            meta.Title = title;
            meta.AllowedUser = EnumUtil.TryParseOrDefault<AllowedUser>(allowedUserName);
            meta.ViolentUssage = EnumUtil.TryParseOrDefault<UssageLicense>(violentUssageName);
            meta.SexualUssage = EnumUtil.TryParseOrDefault<UssageLicense>(sexualUssageName);
            meta.CommercialUssage = EnumUtil.TryParseOrDefault<UssageLicense>(commercialUssageName);
            meta.OtherPermissionUrl = otherPermissionUrl;

            meta.LicenseType = EnumUtil.TryParseOrDefault<LicenseType>(licenseName);
            meta.OtherLicenseUrl = otherLicenseUrl;
        }
    }


}