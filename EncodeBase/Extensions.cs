namespace EncodeBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using static CodingStrings;
    using CharStringPairs = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<char, string>>;

    public static class Extensions
    {
        /// <summary>
        /// Check if a code string is acceptable for encoding and returns the useable code length.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static ushort CheckCodeString(string code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));
            if (code.Length < 2)
                throw new InvalidOperationException("Please use a coding string of 2 characters at least");

            if (code.Any(c => code.Count(co => co == c) > 1))
                throw new InvalidOperationException("Please use a coding string with all distinct characters");
            var len = 2;
            ushort bits = 1;
            while (len < code.Length)
            {
                len *= 2;
                bits += 1;
            }
            return bits;
        }

        /// <summary>
        /// Encode an enumeration of bytes
        /// </summary>
        /// <param name="bytes">Enumeration</param>
        /// <param name="code">Code string</param>
        /// <returns></returns>
        public static IEnumerable<char> EncodeBase(this IEnumerable<byte> bytes, string code) => 
            bytes.EncodeBase(CheckCodeString(code), i => code[i]);

        /// <summary>
        /// Encode an enumeration of bytes
        /// </summary>
        /// <typeparam name="T">The target encoding type. Usually char or string.</typeparam>
        /// <param name="bytes">Enumeration</param>
        /// <param name="encodingBits">The number of bits used for encoding</param>
        /// <param name="coder">The encoding function</param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the binary mask used to extract bits from the source.
        /// </summary>
        /// <param name="encodingBits"></param>
        /// <returns></returns>
        public static ushort GetMask(int encodingBits)
        {
            ushort m = 1;
            while (encodingBits-- > 0) m *= 2;
            return (ushort)(m - 1);
        }

        /// <summary>
        /// Decode an enumeration of T objects into an enumeration of bytes
        /// </summary>
        /// <typeparam name="T">The source encoding type. Usually char or string.</typeparam>
        /// <param name="coded"></param>
        /// <param name="encodingBits">The number of bits used for encoding</param>
        /// <param name="decoder">Decoding function</param>
        /// <param name="separators">Acceptable separators in the coded string which must be ignored while decoding. Considered as a char[].</param>
        /// <returns></returns>
        public static IEnumerable<byte> DecodeBase<T>(this IEnumerable<T> coded, int encodingBits, Func<T, int> decoder, IEnumerable<T> separators = null)
        {
            var level = 0;
            uint work = 0;
            separators = separators ?? Enumerable.Empty<T>();

            foreach (var c in coded.Where(c => !separators.Any(s => s.Equals(c))))
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

        /// <summary>
        /// Group strings in chunks of applicable encoding length
        /// </summary>
        /// <param name="chars">Enumeration</param>
        /// <param name="length">Chunk length</param>
        /// <returns></returns>
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

        /// <summary>
        /// Decode a string using a decoder function
        /// </summary>
        /// <param name="s"></param>
        /// <param name="codeLength">The coded length (in chars) of each bit chunk</param>
        /// <param name="encodingBits">The number of bits used for encoding</param>
        /// <param name="decoder">The decoder function</param>
        /// <param name="separators">Acceptable separators in the coded string which must be ignored while decoding. Considered as a char[].</param>
        /// <returns></returns>
        public static IEnumerable<byte> DecodeBase(this string s, int codeLength, int encodingBits, Func<string, int> decoder, string separators = null) 
            => s.Where(c => (separators?.IndexOf(c.ToString()) ?? -1) < 0)
                .GroupBy(codeLength)
                .DecodeBase(encodingBits, decoder);

        /// <summary>
        /// Decode a string using a coding string
        /// /// </summary>
        /// <param name="chars"></param>
        /// <param name="code"></param>
        /// <param name="aliases">An enumeration of aliases provided as key-value pairs. Each character given as a char key can be aliased by any character in the value string.</param>
        /// <param name="separators">Acceptable separators in the coded string which must be ignored while decoding. Considered as a char[].</param>
        /// <returns></returns>
        public static IEnumerable<byte> DecodeBase(this IEnumerable<char> chars, string code, CharStringPairs aliases = null, string separators = null)
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

        /// <summary>
        /// Encode a string using a specific character encoding.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encoding"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string EncodeBase(this string s, Encoding encoding, string code)
            => new string(encoding.GetBytes(s).EncodeBase(code).ToArray());

        /// <summary>
        /// Decode a string into a string using a specific character encoding.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="code"></param>
        /// <param name="encoding"></param>
        /// <param name="aliases">An enumeration of aliases provided as key-value pairs. Each character given as a char key can be aliased by any character in the value string.</param>
        /// <param name="separators">Acceptable separators in the coded string which must be ignored while decoding. Considered as a char[].</param>
        /// <returns></returns>
        public static string DecodeBase(this string s, string code, Encoding encoding, CharStringPairs aliases = null, string separators = null)
            => encoding.GetString(s.DecodeBase(code, aliases, separators).ToArray());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="code"></param>
        /// <param name="aliases">An enumeration of aliases provided as key-value pairs. Each character given as a char key can be aliased by any character in the value string.</param>
        /// <param name="separators"></param>
        /// <returns></returns>
        public static IEnumerable<byte> DecodeBase(this string s, string code, CharStringPairs aliases = null, string separators = null)
            => s.ToCharArray().DecodeBase(code, aliases, separators);

        public static string EncodeBase2 (this string s, Encoding encoding) => s.EncodeBase(encoding, Base2 );
        public static string EncodeBase4 (this string s, Encoding encoding) => s.EncodeBase(encoding, Base4 );
        public static string EncodeBase8 (this string s, Encoding encoding) => s.EncodeBase(encoding, Base8 ); 
        public static string EncodeBase16(this string s, Encoding encoding) => s.EncodeBase(encoding, Base16);
        public static string EncodeBase32(this string s, Encoding encoding) => s.EncodeBase(encoding, Base32);
        public static string EncodeBase64(this string s, Encoding encoding) => s.EncodeBase(encoding, Base64);
        public static string EncodeBaseHex(this string s, Encoding encoding)
        {
            var sb = new StringBuilder();
            foreach(var e in encoding.GetBytes(s).EncodeBase(8, b => b.ToString("x"))) sb.Append(e);
            return sb.ToString();
        }

        public static IEnumerable<char> EncodeBase2 (this IEnumerable<byte> bytes) => bytes.EncodeBase(Base2 );
        public static IEnumerable<char> EncodeBase4 (this IEnumerable<byte> bytes) => bytes.EncodeBase(Base4 );
        public static IEnumerable<char> EncodeBase8 (this IEnumerable<byte> bytes) => bytes.EncodeBase(Base8 );
        public static IEnumerable<char> EncodeBase16(this IEnumerable<byte> bytes) => bytes.EncodeBase(Base16);
        public static IEnumerable<char> EncodeBase32(this IEnumerable<byte> bytes) => bytes.EncodeBase(Base32);
        public static IEnumerable<char> EncodeBase64(this IEnumerable<byte> bytes) => bytes.EncodeBase(Base64);
        public static IEnumerable<string> EncodeBaseHex(this IEnumerable<byte> bytes) => bytes.EncodeBase(8, b => b.ToString("x"));

        public static string DecodeBase2 (this string s, Encoding encoding, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base2 , encoding, aliases, separators);
        public static string DecodeBase4 (this string s, Encoding encoding, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base4 , encoding, aliases, separators);
        public static string DecodeBase8 (this string s, Encoding encoding, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base8 , encoding, aliases, separators);
        public static string DecodeBase16(this string s, Encoding encoding, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base16, encoding, aliases, separators);
        public static string DecodeBase32(this string s, Encoding encoding, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base32, encoding, aliases, separators);
        public static string DecodeBase64(this string s, Encoding encoding, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base64, encoding, aliases, separators);

        public static IEnumerable<byte> DecodeBase2 (this string s, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base2 , aliases, separators);
        public static IEnumerable<byte> DecodeBase4 (this string s, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base4 , aliases, separators);
        public static IEnumerable<byte> DecodeBase8 (this string s, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base8 , aliases, separators);
        public static IEnumerable<byte> DecodeBase16(this string s, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base16, aliases, separators);
        public static IEnumerable<byte> DecodeBase32(this string s, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base32, aliases, separators);
        public static IEnumerable<byte> DecodeBase64(this string s, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base64, aliases, separators);
        public static IEnumerable<byte> DecodeBaseHex(this string s, string separators = null) => s.DecodeBase(2, 8, h => Convert.ToByte(h, 16), separators);

        public static IEnumerable<byte> DecodeBase2 (this IEnumerable<char>s, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base2 , aliases, separators);
        public static IEnumerable<byte> DecodeBase4 (this IEnumerable<char>s, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base4 , aliases, separators);
        public static IEnumerable<byte> DecodeBase8 (this IEnumerable<char>s, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base8 , aliases, separators);
        public static IEnumerable<byte> DecodeBase16(this IEnumerable<char>s, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base16, aliases, separators);
        public static IEnumerable<byte> DecodeBase32(this IEnumerable<char>s, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base32, aliases, separators);
        public static IEnumerable<byte> DecodeBase64(this IEnumerable<char>s, CharStringPairs aliases = null, string separators = null) => s.DecodeBase(Base64, aliases, separators);
        public static IEnumerable<byte> DecodeBaseHex(this IEnumerable<string> s, IEnumerable<string> separators = null) => s.DecodeBase(8, h => Convert.ToByte(h, 16), separators);
    }
}
