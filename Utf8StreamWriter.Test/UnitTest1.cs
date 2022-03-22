using Xunit;
using IO.Specialized;
using System.IO;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            using var writer = new Utf8StreamWriter(new MemoryStream());
            writer.WriteFormattable(1);

            writer.Write(string.Create(short.MaxValue, 0, (span, state) =>
            {
                for (int i = 0; i < span.Length; i++)
                {
                    span[i] = (char)(i + 0x20);
                }
            }));
        }
    }
}