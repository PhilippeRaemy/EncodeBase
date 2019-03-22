namespace EncodeBase
{
    using CharStringPair = System.Collections.Generic.KeyValuePair<char, string>;
    using CharStringPairs = System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<char, string>>;

    public static class CodingStrings
    {
        public const string CanonicalSeparators = "-\\.,;:";

        public const string Base2 = "01";
        public const string Base4 = "0123";
        public const string Base8 = "01234567";
        public const string Base16 = "0123456789ABCDEF";
        public const string Base32 = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
        public const string Base64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        public static readonly CharStringPairs Base32CanonicalAliases = new CharStringPairs
        {
            new CharStringPair('0', "O"),
            new CharStringPair('1', "IL"),
            new CharStringPair('V', "U")
        };

    }
}