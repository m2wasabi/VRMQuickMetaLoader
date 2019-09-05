using System;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace VRM.QuickMetaLoader.Model
{
    [Serializable]
    public unsafe struct QuickMetaStruct
    {
        public struct DataStock : IDisposable
        {
            public IntPtr ExporterVersionPtr;
            public int ExporterVersionLength;

            public IntPtr TitlePtr;
            public int TitleLength;

            public IntPtr VersionPtr;
            public int VersionLength;

            public IntPtr AuthorPtr;
            public int AuthorLength;

            public IntPtr ContactInformationPtr;
            public int ContactInformationLength;

            public IntPtr ReferencePtr;
            public int ReferenceLength;

            public IntPtr OtherPermissionUrlPtr;
            public int OtherPermissionUrlLength;

            public IntPtr OtherLicenseUrlPtr;
            public int OtherLicenseUrlLength;

            public IntPtr ThumbnailNamePtr;
            public int ThumbnailNameLength;

            public int Texture;
            public AllowedUser AllowedUser;
            public UssageLicense ViolentUssage;
            public UssageLicense SexualUssage;
            public UssageLicense CommercialUssage;
            public LicenseType License;

            public int BinaryOffset;
            public int BinaryLength;

            public void Dispose()
            {
                if (TitlePtr != IntPtr.Zero)
                    UnsafeUtility.Free(TitlePtr.ToPointer(), Allocator.Persistent);
                if (VersionPtr != IntPtr.Zero)
                    UnsafeUtility.Free(VersionPtr.ToPointer(), Allocator.Persistent);
                if (AuthorPtr != IntPtr.Zero)
                    UnsafeUtility.Free(AuthorPtr.ToPointer(), Allocator.Persistent);
                if (ContactInformationPtr != IntPtr.Zero)
                    UnsafeUtility.Free(ContactInformationPtr.ToPointer(), Allocator.Persistent);
                if (ReferencePtr != IntPtr.Zero)
                    UnsafeUtility.Free(ReferencePtr.ToPointer(), Allocator.Persistent);
                if (OtherPermissionUrlPtr != IntPtr.Zero)
                    UnsafeUtility.Free(OtherPermissionUrlPtr.ToPointer(), Allocator.Persistent);
                if (OtherLicenseUrlPtr != IntPtr.Zero)
                    UnsafeUtility.Free(OtherLicenseUrlPtr.ToPointer(), Allocator.Persistent);
                if (ThumbnailNamePtr != IntPtr.Zero)
                    UnsafeUtility.Free(ThumbnailNamePtr.ToPointer(), Allocator.Persistent);
                //if (ExporterVersionPtr != IntPtr.Zero) 
                //    UnsafeUtility.Free(ExporterVersionPtr.ToPointer(), Allocator.Persistent); 
            }

            private static string Construct(IntPtr ptr, int length)
            {
                if (ptr == IntPtr.Zero || length <= 0)
                    return "";
                return new string((char*)ptr, 0, length);
            }

            public void PushMeta(VRMMetaObject metaObject)
            {
                metaObject.AllowedUser = AllowedUser;
                metaObject.CommercialUssage = CommercialUssage;
                metaObject.SexualUssage = SexualUssage;
                metaObject.ViolentUssage = ViolentUssage;
                metaObject.LicenseType = License;
                metaObject.Title = Construct(TitlePtr, TitleLength);
                metaObject.Author = Construct(AuthorPtr, AuthorLength);
                metaObject.ContactInformation = Construct(ContactInformationPtr, ContactInformationLength);
                metaObject.Reference = Construct(ReferencePtr, ReferenceLength);
                metaObject.Version = Construct(VersionPtr, VersionLength);
                metaObject.OtherLicenseUrl = Construct(OtherLicenseUrlPtr, OtherLicenseUrlLength);
                metaObject.OtherPermissionUrl = Construct(OtherPermissionUrlPtr, OtherPermissionUrlLength);
                metaObject.ExporterVersion = ExporterVersionPtr == IntPtr.Zero || ExporterVersionLength == 0 ? "" : Encoding.UTF8.GetString((byte*)ExporterVersionPtr, ExporterVersionLength);
            }
        }

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

        public static implicit operator DataStock(QuickMetaStruct from)
        {
            DataStock meta;
            meta.AllowedUser = from.allowedUserName.ParseAllowedUser();
            meta.ViolentUssage = from.violentUssageName.ParseUssageLicense();
            meta.SexualUssage = from.sexualUssageName.ParseUssageLicense();
            meta.CommercialUssage = from.commercialUssageName.ParseUssageLicense();
            meta.License = from.licenseName.ParseLicenseType();
            (meta.TitlePtr, meta.TitleLength) = from.title;
            (meta.AuthorPtr, meta.AuthorLength) = from.author;
            (meta.ContactInformationPtr, meta.ContactInformationLength) = from.contactInformation;
            (meta.OtherLicenseUrlPtr, meta.OtherLicenseUrlLength) = from.otherLicenseUrl;
            (meta.OtherPermissionUrlPtr, meta.OtherPermissionUrlLength) = from.otherPermissionUrl;
            (meta.ReferencePtr, meta.ReferenceLength) = from.reference;
            (meta.VersionPtr, meta.VersionLength) = from.version;
            meta.Texture = from.texture;
            (meta.ExporterVersionPtr, meta.ExporterVersionLength) = (default, default);
            meta.BinaryLength = default;
            meta.BinaryOffset = default;
            (meta.ThumbnailNamePtr, meta.ThumbnailNameLength) = (default, default);
            return meta;
        }
    }
}