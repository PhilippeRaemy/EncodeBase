namespace UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using EncodeBase;
    using MoreLinq.Extensions;
    using Xunit;
    using static EncodeBase.CodingStrings;

    public class Basics
    {
        [Fact]
        public void CaseOxFF()
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
            var clean = Base32;
            var corrupt = aliases.Aggregate(clean, (s, kvp) => s.Replace(kvp.Key.ToString(), kvp.Value));
            Assert.Equal(clean.DecodeBase32(), corrupt.DecodeBase32(aliases));
        }

        [Fact]
        public void Separators()
        {
            var clean = Base32;
            var separators = "-";
            var separated = clean.Batch(5).Select(b => new string(b.ToArray())).ToDelimitedString(separators);
            Assert.Equal(clean.DecodeBase32(), separated.DecodeBase32(separators: separators));
        }
    }
}
