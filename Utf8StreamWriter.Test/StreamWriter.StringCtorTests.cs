// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using IO.Specialized;

namespace System.IO.Tests
{
    public class StreamWriter_OpenPathTests
    {
        [Fact]
        public static void NullArgs_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentException>("path", () => Utf8StreamWriter.OpenPath(false, path: (string)null!));
            Assert.Throws<ArgumentException>("path", () => Utf8StreamWriter.OpenPath(false, path: (string)null!, append: false));
            Assert.Throws<ArgumentException>("path", () => Utf8StreamWriter.OpenPath(false, path: (string)null!, append: true));
            Assert.Throws<ArgumentException>("path", () => Utf8StreamWriter.OpenPath(false, path: (string)null!, append: true));
            Assert.Throws<ArgumentException>("path", () => Utf8StreamWriter.OpenPath(false, path: (string)null!, true, -1));
        }

        [Fact]
        public static void EmptyPath_ThrowsArgumentException()
        {
            // No argument name for the empty path exception
            Assert.Throws<ArgumentException>("path", () => Utf8StreamWriter.OpenPath(false, path: ""));
            Assert.Throws<ArgumentException>("path", () => Utf8StreamWriter.OpenPath(false, path: "", new FileStreamOptions()));
            Assert.Throws<ArgumentException>("path", () => Utf8StreamWriter.OpenPath(bom: false, path: "", append: false));
            Assert.Throws<ArgumentException>("path", () => Utf8StreamWriter.OpenPath(bom: false, path: "", append: true));
            Assert.Throws<ArgumentException>("path", () => Utf8StreamWriter.OpenPath(bom: false, path: "", options: new FileStreamOptions()));
            Assert.Throws<ArgumentException>("path", () => Utf8StreamWriter.OpenPath(bom: false, path: "", append: true, bufferSize: -1));
        }

        [Fact]
        public static void NegativeBufferSize_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>("bufferSize", () => Utf8StreamWriter.OpenPath(bom: false, path: "path", append: false, bufferSize: -1));
            Assert.Throws<ArgumentOutOfRangeException>("bufferSize", () => Utf8StreamWriter.OpenPath(bom: false, path: "path", append: true, bufferSize: 0));
        }

        [Fact]
        public static void CreateStreamWriter()
        {
            string testfile = Path.GetTempFileName();
            string testString = "Hello World!";
            try
            {
                using (Utf8StreamWriter writer = Utf8StreamWriter.OpenPath(false, path: testfile))
                {
                    writer.Write(testString);
                }

                using (StreamReader reader = new StreamReader(testfile))
                {
                    Assert.Equal(testString, reader.ReadToEnd());
                }
            }
            finally
            {
                File.Delete(testfile);
            }
        }

        public static IEnumerable<object[]> EncodingsToTestStreamWriter()
        {
            yield return new object[] { false, "This is UTF8\u00FF" };
            yield return new object[] { true, "This is BigEndianUnicode\u00FF" };
        }

        [Theory]
        [MemberData(nameof(EncodingsToTestStreamWriter))]
        public static void TestEncoding(bool bom, string testString)
        {
            string testfile = Path.GetTempFileName();
            try
            {
                using (Utf8StreamWriter writer = Utf8StreamWriter.OpenPath(bom: bom, path: testfile, append: false))
                {
                    writer.Write(testString);
                }

                using (StreamReader reader = new StreamReader(testfile, Encoding.UTF8, bom))
                {
                    Assert.Equal(testString, reader.ReadToEnd());
                }
            }
            finally
            {
                File.Delete(testfile);
            }
        }
    }
}
