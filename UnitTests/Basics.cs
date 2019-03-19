namespace UnitTests
{
    using Xunit;

    public class Basics
    {
        public static TheoryData<int, int> Masks =>
            new TheoryData<int, int>
            {
                { 1, 1 },
                { 2, 3 },
                { 3, 7 },
                { 4, 15 },
                { 5, 31 },
                { 6, 63 },
                { 7, 127 },
                { 8, 255 }
            };

        [Theory]
        [MemberData(nameof(Masks))]
        public void GetMask(int encodingBits, int expectedMask )
        {
            Assert.Equal(expectedMask, EncodeBase.Extensions.GetMask(encodingBits));
        }
    }
}
