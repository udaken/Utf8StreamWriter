// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IO.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.IO.Tests
{
    public class TextWriterTests
    {
        protected static Utf8StreamWriter GetNewTextWriter()
        {
            return new Utf8StreamWriter(new MemoryStream()) { NewLine = "---" };
        }

        private static string GetAsText(Utf8StreamWriter writer)
        {
            writer.Flush();
            writer.BaseStream.Position = 0;
            _ = ((MemoryStream)writer.BaseStream).TryGetBuffer(out var buffer);
            writer.BaseStream.SetLength(0);

            return Encoding.UTF8.GetString(buffer.AsSpan());
        }

        private static void Clear(Utf8StreamWriter tw)
        {
            tw.Flush();
            tw.BaseStream.Position = 0;
        }

        #region Write Overloads

        [Fact]
        public void WriteCharTest()
        {
            using (var tw = GetNewTextWriter())
            {
                for (int count = 0; count < TestDataProvider.CharData.Length; ++count)
                {
                    tw.Write(TestDataProvider.CharData[count]);
                }
                Assert.Equal(new string(TestDataProvider.CharData), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteCharArrayTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(TestDataProvider.CharData);
                Assert.Equal(new string(TestDataProvider.CharData), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteCharArrayIndexCountTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(TestDataProvider.CharData, 3, 5);
                Assert.Equal(new string(TestDataProvider.CharData, 3, 5), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteBoolTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(true);
                Assert.Equal("True", GetAsText(tw));

                Clear(tw);
                tw.Write(false);
                Assert.Equal("False", GetAsText(tw));
            }
        }

        [Fact]
        public void WriteIntTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(int.MinValue);
                Assert.Equal(int.MinValue.ToString(tw.FormatProvider), GetAsText(tw));

                Clear(tw);
                tw.Write(int.MaxValue);
                Assert.Equal(int.MaxValue.ToString(tw.FormatProvider), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteUIntTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(uint.MinValue);
                Assert.Equal(uint.MinValue.ToString(tw.FormatProvider), GetAsText(tw));

                Clear(tw);
                tw.Write(uint.MaxValue);
                Assert.Equal(uint.MaxValue.ToString(tw.FormatProvider), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLongTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(long.MinValue);
                Assert.Equal(long.MinValue.ToString(tw.FormatProvider), GetAsText(tw));

                Clear(tw);
                tw.Write(long.MaxValue);
                Assert.Equal(long.MaxValue.ToString(tw.FormatProvider), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteULongTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(ulong.MinValue);
                Assert.Equal(ulong.MinValue.ToString(tw.FormatProvider), GetAsText(tw));

                Clear(tw);
                tw.Write(ulong.MaxValue);
                Assert.Equal(ulong.MaxValue.ToString(tw.FormatProvider), GetAsText(tw));

            }
        }

        [Fact]
        public void WriteFloatTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(float.MinValue);
                Assert.Equal(float.MinValue.ToString(tw.FormatProvider), GetAsText(tw));

                Clear(tw);
                tw.Write(float.MaxValue);
                Assert.Equal(float.MaxValue.ToString(tw.FormatProvider), GetAsText(tw));

                Clear(tw);
                tw.Write(float.NaN);
                Assert.Equal(float.NaN.ToString(tw.FormatProvider), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteDoubleTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(double.MinValue);
                Assert.Equal(double.MinValue.ToString(tw.FormatProvider), GetAsText(tw));
                Clear(tw);

                tw.Write(double.MaxValue);
                Assert.Equal(double.MaxValue.ToString(tw.FormatProvider), GetAsText(tw));
                Clear(tw);

                tw.Write(double.NaN);
                Assert.Equal(double.NaN.ToString(tw.FormatProvider), GetAsText(tw));
                Clear(tw);
            }
        }

        [Fact]
        public void WriteDecimalTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(decimal.MinValue);
                Assert.Equal(decimal.MinValue.ToString(tw.FormatProvider), GetAsText(tw));

                Clear(tw);
                tw.Write(decimal.MaxValue);
                Assert.Equal(decimal.MaxValue.ToString(tw.FormatProvider), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteStringTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(new string(TestDataProvider.CharData));
                Assert.Equal(new string(TestDataProvider.CharData), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteObjectTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(TestDataProvider.FirstObject);
                Assert.Equal(TestDataProvider.FirstObject.ToString(), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteStringObjectTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(TestDataProvider.FormatStringOneObject, TestDataProvider.FirstObject);
                Assert.Equal(string.Format(TestDataProvider.FormatStringOneObject, TestDataProvider.FirstObject), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteStringTwoObjectsTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(TestDataProvider.FormatStringTwoObjects, TestDataProvider.FirstObject, TestDataProvider.SecondObject);
                Assert.Equal(string.Format(TestDataProvider.FormatStringTwoObjects, TestDataProvider.FirstObject, TestDataProvider.SecondObject), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteStringThreeObjectsTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(TestDataProvider.FormatStringThreeObjects, TestDataProvider.FirstObject, TestDataProvider.SecondObject, TestDataProvider.ThirdObject);
                Assert.Equal(string.Format(TestDataProvider.FormatStringThreeObjects, TestDataProvider.FirstObject, TestDataProvider.SecondObject, TestDataProvider.ThirdObject), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteStringMultipleObjectsTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.Write(TestDataProvider.FormatStringMultipleObjects, TestDataProvider.MultipleObjects);
                Assert.Equal(string.Format(TestDataProvider.FormatStringMultipleObjects, TestDataProvider.MultipleObjects), GetAsText(tw));
            }
        }

        #endregion

        #region WriteLine Overloads

        [Fact]
        public void WriteLineTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine();
                Assert.Equal(tw.NewLine, GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineCharTest()
        {
            using (var tw = GetNewTextWriter())
            {
                for (int count = 0; count < TestDataProvider.CharData.Length; ++count)
                {
                    tw.WriteLine(TestDataProvider.CharData[count]);
                }
                Assert.Equal(string.Join(tw.NewLine, TestDataProvider.CharData.Select(ch => ch.ToString()).ToArray()) + tw.NewLine, GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineCharArrayTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(TestDataProvider.CharData);
                Assert.Equal(new string(TestDataProvider.CharData) + tw.NewLine, GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineCharArrayIndexCountTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(TestDataProvider.CharData, 3, 5);
                Assert.Equal(new string(TestDataProvider.CharData, 3, 5) + tw.NewLine, GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineBoolTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(true);
                Assert.Equal("True" + tw.NewLine, GetAsText(tw));

                Clear(tw);
                tw.WriteLine(false);
                Assert.Equal("False" + tw.NewLine, GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineIntTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(int.MinValue);
                Assert.Equal(int.MinValue.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));

                Clear(tw);
                tw.WriteLine(int.MaxValue);
                Assert.Equal(int.MaxValue.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineUIntTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(uint.MinValue);
                Assert.Equal(uint.MinValue.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));

                Clear(tw);
                tw.WriteLine(uint.MaxValue);
                Assert.Equal(uint.MaxValue.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineLongTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(long.MinValue);
                Assert.Equal(long.MinValue.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));

                Clear(tw);
                tw.WriteLine(long.MaxValue);
                Assert.Equal(long.MaxValue.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineULongTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(ulong.MinValue);
                Assert.Equal(ulong.MinValue.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));

                Clear(tw);
                tw.WriteLine(ulong.MaxValue);
                Assert.Equal(ulong.MaxValue.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineFloatTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(float.MinValue);
                Assert.Equal(float.MinValue.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));

                Clear(tw);
                tw.WriteLine(float.MaxValue);
                Assert.Equal(float.MaxValue.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));

                Clear(tw);
                tw.WriteLine(float.NaN);
                Assert.Equal(float.NaN.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineDoubleTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(double.MinValue);
                Assert.Equal(double.MinValue.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));
                Clear(tw);

                tw.WriteLine(double.MaxValue);
                Assert.Equal(double.MaxValue.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));
                Clear(tw);

                tw.WriteLine(double.NaN);
                Assert.Equal(double.NaN.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));
                Clear(tw);
            }
        }

        [Fact]
        public void WriteLineDecimalTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(decimal.MinValue);
                Assert.Equal(decimal.MinValue.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));

                Clear(tw);
                tw.WriteLine(decimal.MaxValue);
                Assert.Equal(decimal.MaxValue.ToString(tw.FormatProvider) + tw.NewLine, GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineStringTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(new string(TestDataProvider.CharData));
                Assert.Equal(new string(TestDataProvider.CharData) + tw.NewLine, GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineObjectTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(TestDataProvider.FirstObject);
                Assert.Equal(TestDataProvider.FirstObject.ToString() + tw.NewLine, GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineStringObjectTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(TestDataProvider.FormatStringOneObject, TestDataProvider.FirstObject);
                Assert.Equal(string.Format(TestDataProvider.FormatStringOneObject + tw.NewLine, TestDataProvider.FirstObject), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineStringTwoObjectsTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(TestDataProvider.FormatStringTwoObjects, TestDataProvider.FirstObject, TestDataProvider.SecondObject);
                Assert.Equal(string.Format(TestDataProvider.FormatStringTwoObjects + tw.NewLine, TestDataProvider.FirstObject, TestDataProvider.SecondObject), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineStringThreeObjectsTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(TestDataProvider.FormatStringThreeObjects, TestDataProvider.FirstObject, TestDataProvider.SecondObject, TestDataProvider.ThirdObject);
                Assert.Equal(string.Format(TestDataProvider.FormatStringThreeObjects + tw.NewLine, TestDataProvider.FirstObject, TestDataProvider.SecondObject, TestDataProvider.ThirdObject), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineStringMultipleObjectsTest()
        {
            using (var tw = GetNewTextWriter())
            {
                tw.WriteLine(TestDataProvider.FormatStringMultipleObjects, TestDataProvider.MultipleObjects);
                Assert.Equal(string.Format(TestDataProvider.FormatStringMultipleObjects + tw.NewLine, TestDataProvider.MultipleObjects), GetAsText(tw));
            }
        }

        #endregion

        #region Write Async Overloads

        public async Task WriteAsyncCharTest()
        {
            using (var tw = GetNewTextWriter())
            {
                await tw.WriteAsync('a');
                Assert.Equal("a", GetAsText(tw));
            }
        }

        public async Task WriteAsyncStringTest()
        {
            using (var tw = GetNewTextWriter())
            {
                var toWrite = new string(TestDataProvider.CharData);
                await tw.WriteAsync(toWrite);
                Assert.Equal(toWrite, GetAsText(tw));
            }
        }

        public async Task WriteAsyncCharArrayIndexCountTest()
        {
            using (var tw = GetNewTextWriter())
            {
                await tw.WriteAsync(TestDataProvider.CharData, 3, 5);
                Assert.Equal(new string(TestDataProvider.CharData, 3, 5), GetAsText(tw));
            }
        }

        #endregion

        #region WriteLineAsync Overloads

        public async Task WriteLineAsyncTest()
        {
            using (var tw = GetNewTextWriter())
            {
                await tw.WriteLineAsync();
                Assert.Equal(tw.NewLine, GetAsText(tw));
            }
        }

        public async Task WriteLineAsyncCharTest()
        {
            using (var tw = GetNewTextWriter())
            {
                await tw.WriteLineAsync('a');
                Assert.Equal("a" + tw.NewLine, GetAsText(tw));
            }
        }

        public async Task WriteLineAsyncStringTest()
        {
            using (var tw = GetNewTextWriter())
            {
                var toWrite = new string(TestDataProvider.CharData);
                await tw.WriteLineAsync(toWrite);
                Assert.Equal(toWrite + tw.NewLine, GetAsText(tw));
            }
        }

        public async Task WriteLineAsyncCharArrayIndexCount()
        {
            using (var tw = GetNewTextWriter())
            {
                await tw.WriteLineAsync(TestDataProvider.CharData, 3, 5);
                Assert.Equal(new string(TestDataProvider.CharData, 3, 5) + tw.NewLine, GetAsText(tw));
            }
        }

        #endregion

        [Fact]
        public void WriteCharSpanTest()
        {
            using (var tw = GetNewTextWriter())
            {
                var rs = new ReadOnlySpan<char>(TestDataProvider.CharData, 4, 6);
                tw.Write(rs);
                Assert.Equal(new string(rs), GetAsText(tw));
            }
        }

        [Fact]
        public void WriteLineCharSpanTest()
        {
            using (var tw = GetNewTextWriter())
            {
                var rs = new ReadOnlySpan<char>(TestDataProvider.CharData, 4, 6);
                tw.WriteLine(rs);
                Assert.Equal(new string(rs) + tw.NewLine, GetAsText(tw));
            }
        }

        public async Task WriteCharMemoryTest()
        {
            using (var tw = GetNewTextWriter())
            {
                var rs = new Memory<char>(TestDataProvider.CharData, 4, 6);
                await tw.WriteAsync(rs);
                Assert.Equal(new string(rs.Span), GetAsText(tw));
            }
        }

        public async Task WriteLineCharMemoryTest()
        {
            using (var tw = GetNewTextWriter())
            {
                var rs = new Memory<char>(TestDataProvider.CharData, 4, 6);
                await tw.WriteLineAsync(rs);
                Assert.Equal(new string(rs.Span) + tw.NewLine, GetAsText(tw));
            }
        }

        [Theory]
        [MemberData(nameof(GetStringBuilderTestData))]
        public void WriteStringBuilderTest(bool isSynchronized, StringBuilder testData)
        {
            using (var ctw = GetNewTextWriter())
            {
                TextWriter tw = isSynchronized ? TextWriter.Synchronized(ctw) : ctw;
                tw.Write(testData);
                tw.Flush();
                Assert.Equal(testData.ToString(), GetAsText(ctw));
            }
        }

        [Theory]
        [MemberData(nameof(GetStringBuilderTestData))]
        public void WriteLineStringBuilderTest(bool isSynchronized, StringBuilder testData)
        {
            using (var ctw = GetNewTextWriter())
            {
                TextWriter tw = isSynchronized ? TextWriter.Synchronized(ctw) : ctw;
                tw.WriteLine(testData);
                tw.Flush();
                Assert.Equal(testData.ToString() + tw.NewLine, GetAsText(ctw));
            }
        }

        [Theory]
        [MemberData(nameof(GetStringBuilderTestData))]
        public async Task WriteAsyncStringBuilderTest(bool isSynchronized, StringBuilder testData)
        {
            using (var ctw = GetNewTextWriter())
            {
                TextWriter tw = isSynchronized ? TextWriter.Synchronized(ctw) : ctw;
                await tw.WriteAsync(testData);
                tw.Flush();
                Assert.Equal(testData.ToString(), GetAsText(ctw));
            }
        }

        [Theory]
        [MemberData(nameof(GetStringBuilderTestData))]
        public async Task WriteLineAsyncStringBuilderTest(bool isSynchronized, StringBuilder testData)
        {

            using (var ctw = GetNewTextWriter())
            {
                TextWriter tw = isSynchronized ? TextWriter.Synchronized(ctw) : ctw;
                await tw.WriteLineAsync(testData);
                tw.Flush();
                Assert.Equal(testData + tw.NewLine, GetAsText(ctw));
            }
        }

        [Fact]
        public void DisposeAsync_InvokesDisposeSynchronously()
        {
            bool disposeInvoked = false;
            var tw = new InvokeActionOnDisposeTextWriter() { DisposeAction = () => disposeInvoked = true };
            Assert.False(disposeInvoked);
            Assert.True(tw.DisposeAsync().IsCompletedSuccessfully);
            Assert.True(disposeInvoked);
        }

        [Fact]
        public void DisposeAsync_ExceptionReturnedInTask()
        {
            Exception e = new FormatException();
            var tw = new InvokeActionOnDisposeTextWriter() { DisposeAction = () => { throw e; } };
            ValueTask vt = tw.DisposeAsync();
            Assert.True(vt.IsFaulted);
            Assert.Same(e, vt.AsTask().Exception?.InnerException);
        }

        private sealed class InvokeActionOnDisposeTextWriter : TextWriter
        {
            public Action DisposeAction;
            public override Encoding Encoding => Encoding.UTF8;
            protected override void Dispose(bool disposing) => DisposeAction?.Invoke();
        }

        // Generate data for TextWriter.Write* methods that take a stringBuilder.
        // We test both the synchronized and unsynchronized variation, on strinbuilder swith 0, small and large values.
        public static IEnumerable<object[]> GetStringBuilderTestData()
        {
            // Make a string that has 10 or so 8K chunks (probably).
            StringBuilder complexStringBuilder = new StringBuilder();
            for (int i = 0; i < 4000; i++)
                complexStringBuilder.Append(TestDataProvider.CharData); // CharData ~ 25 chars

            foreach (StringBuilder testData in new StringBuilder[] { new StringBuilder(""), new StringBuilder(new string(TestDataProvider.CharData)), complexStringBuilder })
            {
                foreach (bool isSynchronized in new bool[] { true, false })
                {
                    yield return new object[] { isSynchronized, testData };
                }
            }
        }
    }
}
