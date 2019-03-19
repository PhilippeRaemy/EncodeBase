namespace EncodeBase
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;

    public static class Extensions
    {
        public static string EncodeBase(this string s, Encoding encoding, string code)
      => new string(encoding.GetBytes(s).EncodeBase(code).ToArray());

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

        public static IEnumerable<char> EncodeBase(this IEnumerable<byte> bytes, string code)
        {
            var level = 0;
            uint work = 0;
            var encodingBits = CheckCodeString(code);

            var mask = GetMask(encodingBits);

            foreach (var b in bytes)
            {
                work = ((work & 0xff) << 8) | b;
                level += 8;
                while (level >= encodingBits)
                {
                    yield return code[(int)((work >> (level - encodingBits)) & mask)];
                    level -= encodingBits;
                }
            }
            if (level > 0)
                yield return code[(int)((work << (encodingBits - level)) & mask)];
        }

        public static int GetMask(int encodingBits)
        {
            var m = 1;
            while (encodingBits-- > 0) m *= 2;
            return m - 1;
        }

        public static string DecodeBase(this string s, string code, Encoding encoding)
            => encoding.GetString(s.DecodeBase(code).ToArray());

        public static IEnumerable<byte> DecodeBase(this string s, string code, NameValueCollection aliases = null, string separators = null) 
            => s.ToCharArray().DecodeBase(code, aliases, separators);

        public static IEnumerable<byte> DecodeBase(this IEnumerable<char> chars, string code, NameValueCollection aliases = null, string separators = null)
        {
            var level = 0;
            uint work = 0;
            var encodingBits = CheckCodeString(code);
            var dic = code.Select((c, i) => new {c, b = (byte)i})
                          .ToDictionary(p => p.c, p => p.b);

            if (aliases!= null)
                foreach (string key in aliases.Keys)
                    foreach (var alias in aliases[key])
                        dic.Add(alias, dic[alias]);

            foreach (var c in chars.Where(c => (separators?.IndexOf(c) ?? -1) < 0))
            {
                var b5 = dic[c];
                work = (work << encodingBits) | b5;
                level += encodingBits;
                while (level >= 8)
                {
                    var b = (byte)((work >> (level - 8)) & 0xff);
                    level -= 8;
                    yield return b;
                }
            }
        }
    }
}
