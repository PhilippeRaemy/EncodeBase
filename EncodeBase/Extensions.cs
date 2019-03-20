namespace EncodeBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using static CodingStrings;
    using KeyValuePairs = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<char, string>>;

    public static class Extensions
    {
        public static int CheckCodeString(string code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));
            if (code.Length < 2)
                throw new InvalidOperationException("Please use a coding string of 2 characters at least");

            if (code.Any(c => code.Count(co => co == c) > 1))
                throw new InvalidOperationException("Please use a coding string with all distinct characters");
            var len = 2;
            var bits = 1;
            while (len < code.Length)
            {
                len *= 2;
                bits += 1;
            }
            return bits;
        }

        public static IEnumerable<char> EncodeBase(this IEnumerable<byte> bytes, string code) => 
            bytes.EncodeBase(CheckCodeString(code), i => code[i]);

        public static IEnumerable<T> EncodeBase<T>(this IEnumerable<byte> bytes, int encodingBits, Func<int, T> coder)
        {
            var level = 0;
            uint work = 0;

            var mask = GetMask(encodingBits);

            foreach (var b in bytes)
            {
                work = ((work & 0xff) << 8) | b;
                level += 8;
                while (level >= encodingBits)
                {
                    yield return coder((int)((work >> (level - encodingBits)) & mask));
                    level -= encodingBits;
                }
            }
            if (level > 0)
                yield return coder((int)((work << (encodingBits - level)) & mask));
        }

        public static int GetMask(int encodingBits)
        {
            var m = 1;
            while (encodingBits-- > 0) m *= 2;
            return m - 1;
        }


        public static IEnumerable<byte> DecodeBase<T>(this IEnumerable<T> chars, int encodingBits, Func<T, int> decoder, string separators = null)
        {
            var level = 0;
            uint work = 0;

            foreach (var c in chars.Where(c => (separators?.IndexOf(c.ToString()) ?? -1) < 0))
            {
                var b5 = decoder(c);
                work = (uint)((work << encodingBits) | b5);
                level += encodingBits;
                while (level >= 8)
                {
                    var b = (byte)((work >> (level - 8)) & 0xff);
                    level -= 8;
                    yield return b;
                }
            }
        }

        static IEnumerable<string> GroupBy(this IEnumerable<char> chars, int length)
        {
            var s = string.Empty;
            foreach (var c in chars)
            {
                s += c;
                if (s.Length < length) continue;
                yield return s;
                s = string.Empty;
            }
            if (s.Length > 0) 
                yield return s;
        }

        public static IEnumerable<byte> DecodeBase(this string s, int codeLength, int encodingBits, Func<string, int> decoder, string separators = null) 
            => s.Where(c => (separators?.IndexOf(c.ToString()) ?? -1) < 0)
                .GroupBy(codeLength)
                .DecodeBase(encodingBits, decoder);

        public static IEnumerable<byte> DecodeBase(this IEnumerable<char> chars, string code, KeyValuePairs aliases = null, string separators = null)
        {
            var encodingBits = CheckCodeString(code);
            var dic = code.Select((c, i) => new {c, b = (byte)i})
                          .ToDictionary(p => p.c, p => p.b);

            if (aliases!= null)
                foreach (var kvp in aliases)
                    foreach (var alias in kvp.Value)
                        dic.Add(alias, dic[kvp.Key]);

            return chars.DecodeBase(encodingBits, b => dic[b], separators);
        }

        public static string EncodeBase(this string s, Encoding encoding, string code)
            => new string(encoding.GetBytes(s).EncodeBase(code).ToArray());

        public static string DecodeBase(this string s, string code, Encoding encoding, KeyValuePairs aliases = null, string separators = null)
            => encoding.GetString(s.DecodeBase(code, aliases, separators).ToArray());

        public static IEnumerable<byte> DecodeBase(this string s, string code, KeyValuePairs aliases = null, string separators = null)
            => s.ToCharArray().DecodeBase(code, aliases, separators);

        public static string EncodeBase2 (this string s, Encoding encoding) => s.EncodeBase(encoding, Base2 );
        public static string EncodeBase4 (this string s, Encoding encoding) => s.EncodeBase(encoding, Base4 );
        public static string EncodeBase8 (this string s, Encoding encoding) => s.EncodeBase(encoding, Base8 ); 
        public static string EncodeBase16(this string s, Encoding encoding) => s.EncodeBase(encoding, Base16);
        public static string EncodeBase32(this string s, Encoding encoding) => s.EncodeBase(encoding, Base32);
        public static string EncodeBase64(this string s, Encoding encoding) => s.EncodeBase(encoding, Base64);

        public static IEnumerable<char> EncodeBase2 (this IEnumerable<byte> bytes) => bytes.EncodeBase(Base2 );
        public static IEnumerable<char> EncodeBase4 (this IEnumerable<byte> bytes) => bytes.EncodeBase(Base4 );
        public static IEnumerable<char> EncodeBase8 (this IEnumerable<byte> bytes) => bytes.EncodeBase(Base8 );
        public static IEnumerable<char> EncodeBase16(this IEnumerable<byte> bytes) => bytes.EncodeBase(Base16);
        public static IEnumerable<char> EncodeBase32(this IEnumerable<byte> bytes) => bytes.EncodeBase(Base32);
        public static IEnumerable<char> EncodeBase64(this IEnumerable<byte> bytes) => bytes.EncodeBase(Base64);
        public static IEnumerable<string> EncodeBase256(this IEnumerable<byte> bytes) => bytes.EncodeBase(16, b => b.ToString("x"));

        public static string DecodeBase2 (this string s, Encoding encoding, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base2 , encoding, aliases, separators);
        public static string DecodeBase4 (this string s, Encoding encoding, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base4 , encoding, aliases, separators);
        public static string DecodeBase8 (this string s, Encoding encoding, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base8 , encoding, aliases, separators);
        public static string DecodeBase16(this string s, Encoding encoding, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base16, encoding, aliases, separators);
        public static string DecodeBase32(this string s, Encoding encoding, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base32, encoding, aliases, separators);
        public static string DecodeBase64(this string s, Encoding encoding, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base64, encoding, aliases, separators);

        public static IEnumerable<byte> DecodeBase2 (this string s, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base2 , aliases, separators);
        public static IEnumerable<byte> DecodeBase4 (this string s, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base4 , aliases, separators);
        public static IEnumerable<byte> DecodeBase8 (this string s, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base8 , aliases, separators);
        public static IEnumerable<byte> DecodeBase16(this string s, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base16, aliases, separators);
        public static IEnumerable<byte> DecodeBase32(this string s, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base32, aliases, separators);
        public static IEnumerable<byte> DecodeBase64(this string s, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base64, aliases, separators);
        public static IEnumerable<byte> DecodeBase256(this string s, string separators = null) => s.DecodeBase(16, 2, h => Convert.ToByte(h, 16), separators);

        public static IEnumerable<byte> DecodeBase2 (this IEnumerable<char>s, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base2 , aliases, separators);
        public static IEnumerable<byte> DecodeBase4 (this IEnumerable<char>s, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base4 , aliases, separators);
        public static IEnumerable<byte> DecodeBase8 (this IEnumerable<char>s, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base8 , aliases, separators);
        public static IEnumerable<byte> DecodeBase16(this IEnumerable<char>s, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base16, aliases, separators);
        public static IEnumerable<byte> DecodeBase32(this IEnumerable<char>s, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base32, aliases, separators);
        public static IEnumerable<byte> DecodeBase64(this IEnumerable<char>s, KeyValuePairs aliases = null, string separators = null) => s.DecodeBase(Base64, aliases, separators);
        public static IEnumerable<byte> DecodeBase256(this IEnumerable<string> s, string separators = null) => s.DecodeBase(16, h => Convert.ToByte(h, 16), separators);
    }
}
