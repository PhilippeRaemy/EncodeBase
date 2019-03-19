namespace UnitTests
{
    using Xunit;

    public class CheckCodeStrings
    {
        public static TheoryData<string, int> CodeData =>
            new TheoryData<string, int>
            {
                { "01", 1 },
                { "0123", 2 },
                { "01234567", 3 },
                { "0123456789abcdef", 4 },
                { "0123456789abcdefghijklmnopqrstuv", 5 },
            };

        [Theory]
        [MemberData(nameof(CodeData))]
        public void GetMask(string code, int expectedEncodingBits)
        {
            Assert.Equal(expectedEncodingBits, EncodeBase.Extensions.CheckCodeString(code));
        }
    }
}
