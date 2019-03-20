
namespace UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using EncodeBase;

    public class BaseEncodeDecodeHex
    {
        static readonly byte[] Bytes = Enumerable.Range(0, 255).Select(b => (byte)b).ToArray();

        static readonly int[] Prefixes = Enumerable.Range(0, 5).ToArray();

        public static IEnumerable<object[]> Cases =>
            from b in Bytes
            from p in Prefixes
            select new object[] {b, p};

        [Theory]
        [MemberData(nameof(Cases))]
        public void EncodeDecode(byte b, int prefix)
        {
            var bytes = Bytes.Take(prefix).Append(b).ToArray();
            Assert.Equal(bytes, bytes.EncodeBaseHex().DecodeBaseHex().ToArray());
        }
    }
}
