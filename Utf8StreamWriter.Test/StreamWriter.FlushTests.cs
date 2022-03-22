// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using Xunit;
using IO.Specialized;

namespace System.IO.Tests
{
    public class FlushTests
    {
        protected virtual Stream CreateStream()
        {
            return new MemoryStream();
        }

        [Fact]
        public void AutoFlushSetTrue()
        {
            // [] Set the autoflush to true
            var sw2 = new Utf8StreamWriter(CreateStream());
            Assert.False(sw2.AutoFlush);
            sw2.Write("SomeData");
            sw2.AutoFlush = true;
            Assert.True(sw2.AutoFlush);
            Assert.NotEqual(0, sw2.BaseStream.Position);
        }

        [Fact]
        public void AutoFlushSetFalse()
        {
            // [] Set autoflush to false
            var sw2 = new Utf8StreamWriter(CreateStream());
            Assert.False(sw2.AutoFlush);
            sw2.Write("SomeData");
            sw2.AutoFlush = false;
            Assert.False(sw2.AutoFlush);
            Assert.Equal(0, sw2.BaseStream.Position);
        }
    }
}
