namespace UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using EncodeBase;
    using MoreLinq.Extensions;
    using Xunit;
    using static EncodeBase.CodingStrings;

    public class Basics
    {
        [Fact]
        public void CaseOxff()
        {
            Assert.Equal("ZW", new string(new byte[] { 255 }.EncodeBase(Base32).ToArray()));
        }

        [Fact]
        public void Aliases()
        {
            var aliases = new[]
            {
                new KeyValuePair<char,string>('0', "O"),
                new KeyValuePair<char,string>('1', "I"),
                new KeyValuePair<char,string>('V', "U"),
            };
            var corrupt = aliases.Aggregate(Base32, (s, kvp) => s.Replace(kvp.Key.ToString(), kvp.Value));
            Assert.Equal(Base32.DecodeBase32(), corrupt.DecodeBase32(aliases));
        }

        [Fact]
        public void Separators()
        {
            const string separators = "-";
            var separated = Base32.Batch(5).Select(b => new string(b.ToArray())).ToDelimitedString(separators);
            Assert.Equal(Base32.DecodeBase32(), separated.DecodeBase32(separators: separators));
        }
    }
}
