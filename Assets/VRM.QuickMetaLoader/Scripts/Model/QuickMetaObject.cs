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

        public void PushMeta(ref VRMMetaObject meta)
        {
            meta.Version = version; // model version
            meta.Author = author;
            meta.ContactInformation = contactInformation;
            meta.Reference = reference;
            meta.Title = title;
            meta.AllowedUser = allowedUserName.ParseAllowedUser();
            meta.ViolentUssage = violentUssageName.ParseUssageLicense();
            meta.SexualUssage = sexualUssageName.ParseUssageLicense();
            meta.CommercialUssage = commercialUssageName.ParseUssageLicense();
            meta.OtherPermissionUrl = otherPermissionUrl;

            meta.LicenseType = licenseName.ParseLicenseType();
            meta.OtherLicenseUrl = otherLicenseUrl;
        }
    }


}