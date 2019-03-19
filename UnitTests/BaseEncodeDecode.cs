
namespace UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using EncodeBase;

    public class Bytes : TheoryData<byte>
    {
        public Bytes()
        {
            for (byte b = 0; b <= 255; b++)
                Add(b);
        }
    }

    public class BaseEncodeDecode
    {
        public static IEnumerable<object[]> Bytes => Enumerable.Range(0, 255).Select(i => new object []{i});


        [Theory]
        [MemberData(nameof(Bytes))]
        public void OneByte(byte b)
        {
            var bytes = new[] {b};
            Assert.Equal(bytes, bytes.EncodeBase("01234567").ToArray().DecodeBase("01234567").ToArray());
        }
    }
}
