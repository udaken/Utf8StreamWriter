// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using System.Threading.Tasks;
using Xunit;
using IO.Specialized;

namespace System.IO.Tests
{
    public class StreamWriterTests
    {

        [Fact]
        public void DisposeAsync_CanInvokeMultipleTimes()
        {
            var ms = new MemoryStream();
            var sw = new Utf8StreamWriter(ms);
            Assert.True(sw.DisposeAsync().IsCompletedSuccessfully);
            Assert.True(sw.DisposeAsync().IsCompletedSuccessfully);
        }

        [Fact]
        public void DisposeAsync_CanDisposeAsyncAfterDispose()
        {
            var ms = new MemoryStream();
            var sw = new Utf8StreamWriter(ms);
            sw.Dispose();
            Assert.True(sw.DisposeAsync().IsCompletedSuccessfully);
        }

        [Fact]
        public async Task DisposeAsync_FlushesStream()
        {
            var ms = new MemoryStream();
            var sw = new Utf8StreamWriter(ms);
            try
            {
                sw.Write("hello");
                Assert.Equal(0, ms.Position);
            }
            finally
            {
                await sw.DisposeAsync();
            }
            Assert.Throws<ObjectDisposedException>(() => ms.Position);
            Assert.Equal(5, ms.ToArray().Length);
        }

        [Fact]
        public async Task DisposeAsync_LeaveOpenTrue_LeftOpen()
        {
            var ms = new MemoryStream();
            var sw = new Utf8StreamWriter(ms, bom: false, 0x1000, leaveOpen: true);
            try
            {
                sw.Write("hello");
            }
            finally
            {
                await sw.DisposeAsync();
            }
            Assert.Equal(5, ms.Position); // doesn't throw
        }

    }
}
