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

            return (int)Math.Log(code.Length, 2);
        }

        public static IEnumerable<char> EncodeBase(this IEnumerable<byte> bytes, string code)
        {
            var level = 0;
            uint work = 0;
            var encodingBits = CheckCodeString(code);
            var mask = (int)Math.Pow(2, encodingBits) - 1;

            foreach (var b in bytes)
            {
                work = (uint)(((work & 0xff) << 8) | b);
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
