namespace EncodeBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class Extensions
    {
        public static string EncodeBase(this string s, string code)
      => new string(Encoding.Default.GetBytes(s).EncodeBase(code).ToArray());

        static int CheckCodeString(string code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));
            if (code.Length < 2)
                throw new InvalidOperationException("Please use a coding string of 2 characters at least");

            if (code.Any(c => code.Count(co => co == c) > 1))
                throw new InvalidOperationException("Please use a coding string with all distinct characters");
            var len = 2;
            while (len < code.Length) len *= 2;
            return len / 2;
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

        static int GetMask(int encodingBits)
        {
            var m = 1;
            while (encodingBits-- > 0) m *= 2;
            return m - 1;
        }

        public static string DecodeBase(this string s, string code)
            => new string(s.ToCharArray().DecodeBase(code).Select(b => (char)b).ToArray());

        public static IEnumerable<byte> DecodeBase(this char[] chars, string code)
        {
            var level = 0;
            uint work = 0;
            var encodingBits = CheckCodeString(code);
            foreach (var c in chars)
            {
                var b5 = (byte)code.IndexOf(c);
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
    }
}
