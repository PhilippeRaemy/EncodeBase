
namespace UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using EncodeBase;

    public class BaseEncodeDecode
    {
        static readonly string [] Codes = {
            "01", 
            "0123",
            "01234567",
            "0123456789abcdef",
            "0123456789abcdefghijklmnopqrstuv",
        };

        static readonly byte[] Bytes = Enumerable.Range(0, 255).Select(b => (byte)b).ToArray();

        static readonly int[] Prefixes = Enumerable.Range(0, 8).ToArray();

        public static IEnumerable<object[]> Cases =>
            from c in Codes
            from b in Bytes
            from p in Prefixes
            select new object[] {c, b, p};

        [Theory]
        [MemberData(nameof(Cases))]
        public void EncodeDecode(string code, byte b, int prefix)
        {
            var bytes = Bytes.Take(prefix).Append(b).ToArray();
            Assert.Equal(bytes, bytes.EncodeBase(code).ToArray().DecodeBase(code).ToArray());
        }

    }
}
