namespace VRM.QuickMetaLoader
{
    internal static class EnumParser
    {
        public static AllowedUser ParseAllowedUser(this string text)
        {
            if (string.IsNullOrEmpty(text)) return default;
            switch (text.Length)
            {
                case 8: return AllowedUser.Everyone;
                case 10: return AllowedUser.OnlyAuthor;
                case 22: return AllowedUser.ExplicitlyLicensedPerson;
                default: return default;
            }
        }

        public static UssageLicense ParseUssageLicense(this string text)
        {
            if (string.IsNullOrEmpty(text)) return default;
            switch (text.Length)
            {
                case 5: return UssageLicense.Allow;
                case 8: return UssageLicense.Disallow;
                default: return default;
            }
        }

        public static LicenseType ParseLicenseType(this string text)
        {
            if (string.IsNullOrEmpty(text)) return default;
            switch (text.Length)
            {
                case 25:
                    return LicenseType.Redistribution_Prohibited;
                case 3:
                    if ((text[0] == 'c' || text[0] == 'C') &&
                     (text[1] == 'c' || text[1] == 'C') &&
                     text[2] == '0')
                        return LicenseType.CC0;
                    return default;
                case 5:
                    if ((text[0] == 'c' || text[0] == 'C') &&
                        (text[1] == 'c' || text[1] == 'C') &&
                        text[2] == '_' &&
                        (text[3] == 'b' || text[3] == 'B') &&
                        (text[4] == 'y' || text[4] == 'Y'))
                        return LicenseType.CC_BY;
                    return LicenseType.Other;
                case 8:
                    if ((text[0] != 'c' && text[0] != 'C') || (text[1] != 'c' && text[1] != 'C') || text[2] != '_' || (text[3] != 'b' && text[3] != 'B') || (text[4] != 'y' && text[4] != 'Y') || text[5] != '_') return LicenseType.Other;
                    if (text[6] == 'n' || text[6] == 'N')
                    {
                        if (text[7] == 'c' || text[7] == 'C')
                        {
                            return LicenseType.CC_BY_NC;
                        }
                        return LicenseType.CC_BY_ND;
                    }
                    if (text[6] == 's' || text[6] == 'S')
                    {
                        return LicenseType.CC_BY_SA;
                    }
                    return LicenseType.Other;
                case 11:
                    if (text[10] == 'a' || text[10] == 'A')
                    {
                        return LicenseType.CC_BY_NC_SA;
                    }
                    return LicenseType.CC_BY_NC_ND;
                default:
                    return LicenseType.Other;
            }
        }
    }
}