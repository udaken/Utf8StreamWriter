// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using Xunit;
using IO.Specialized;
using System.Text;

namespace System.IO.Tests
{
    public class CtorTests
    {
        [Fact]
        public static void CreateStreamWriter()
        {
            Utf8StreamWriter sw2;
            StreamReader sr2;
            string str2;
            MemoryStream memstr2;

            // [] Construct writer with MemoryStream
            //-----------------------------------------------------------------

            memstr2 = new MemoryStream();
            sw2 = new Utf8StreamWriter(memstr2);
            sw2.Write("HelloWorld");
            sw2.Flush();
            sr2 = new StreamReader(memstr2);
            memstr2.Position = 0;
            str2 = sr2.ReadToEnd();
            Assert.Equal("HelloWorld", str2);
        }

        [Fact]
        public static void UTF8Encoding()
        {
            TestEnconding(true, "This is UTF8\u00FF");
        }

        [Fact]
        public static void BigEndianUnicodeEncoding()
        {
            TestEnconding(false, "This is UTF8 with bom\u00FF");
        }


        private static void TestEnconding(bool bom, string testString)
        {
            Utf8StreamWriter sw2;
            StreamReader sr2;
            string str2;

            var ms = new MemoryStream();
            sw2 = new Utf8StreamWriter(ms, bom);
            sw2.Write(testString);
            sw2.Dispose();

            var ms2 = new MemoryStream(ms.ToArray());
            sr2 = new StreamReader(ms2, Encoding.UTF8, bom);
            str2 = sr2.ReadToEnd();
            Assert.Equal(testString, str2);
        }
    }
}
