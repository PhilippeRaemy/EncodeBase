namespace UnitTests
{
    using System.Linq;
    using EncodeBase;
    using Xunit;

    public class Basics
    {
        [Fact]
        public void CaseOxFF()
        {
            Assert.Equal("ZW", new string(new byte[]{255}.EncodeBase(CodingStrings.Base32).ToArray()));
        }
    }
}
