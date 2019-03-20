<Query Kind="Program">
  <NuGetReference>Mannex</NuGetReference>
  <NuGetReference>morelinq</NuGetReference>
  <Namespace>MoreLinq</Namespace>
</Query>

void Main()
{
    new[]
    {
        "abc",
         "Rdatasets.survival.leukemia.Data",
         "\xff",
         "\x80"
    }
    .SelectMany(s => new[]
    {
        @"01234567",
        @"0123456789ABCDEF",
        @"0123456789ABCDEFGHJKMNPQRSTVWXYZ",
        @"0123456789ABCDEFGHJKMNPQRSTVWXYZabcdefghijklmnopqrstuvwxyz[]\`~!@#$%^&*()-_=+"
    }
        .Select(c => new {s, c}))
    .Select(x => new { x.s, x.c, e = x.s.Encode32(x.c) })
    .Select(x => new { String = x.s, Code = x.c, Encoded = x.e, Decoded = x.e.Decode32(x.c)})
    .Dump();
}
public static class Extensions
{

    public static string Encode32(this string s, string code)
      => new string(Encoding.Default.GetBytes(s).Encode32(code).ToArray());

    static int CheckCodeString(string code)
    {
        if ((code?.Length ?? 0) < 2)
            throw new InvalidOperationException("Please use a coding string of 2 characters at least");

        return ((int)Math.Log(code.Length, 2));
    }
    
    public static IEnumerable<char> Encode32(this IEnumerable<byte> bytes, string code)
    {
        var level = 0;
        uint work = 0;
        var encodingBits = CheckCodeString(code);
        var mask = (int)Math.Pow(2, encodingBits) - 1;
        
        foreach (var b in bytes)
        {
            work = (uint)(((work & 0xff) <<8)  | b);
            level += 8;
            while (level >= encodingBits)
            {
                yield return code[(int)((work >> (level - encodingBits)) & mask)];
                level -= encodingBits;
            }
        }
        if (level > 0)
            yield return code[(int)((work << (encodingBits - level)) & mask) ];
    }

    public static string Decode32(this string s, string code)
        => new string(s.ToCharArray().Decode32(code).Select(b => (char)b).ToArray());

    public static IEnumerable<byte> Decode32(this char[] chars, string code)
    {
        var level = 0;
        uint work = 0;
        var encodingBits = CheckCodeString(code);
        foreach (var c in chars)
        {
            var b5 = (byte)code.IndexOf(c);
            work = (uint)((work << encodingBits) | b5 );
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