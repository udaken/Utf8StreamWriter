using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using IO.Specialized;
using System.Text;

var config = DefaultConfig.Instance.AddDiagnoser(MemoryDiagnoser.Default);
BenchmarkRunner.Run(typeof(Program).Assembly, config, args: args);

public partial class SynchronousBenchmark
{

    public IEnumerable<object[]> GetTextWriter()
    {
        yield return new object[] { "BCL StreamWriter on Memory", new StreamWriter(new MemoryStream(), Encoding.UTF8, bufferSize: 1024, leaveOpen: false) };
        yield return new object[] { "Utf8StreamWriter on Memory", new Utf8StreamWriter(new MemoryStream(), bom: false, bufferSize: 1024, leaveOpen: false) };
        yield return new object[] { "BCL StreamWriter on File", new StreamWriter(
            Path.Combine(Path.GetTempPath(), Path.GetTempFileName()), false, Encoding.UTF8, bufferSize: 4096) };
        yield return new object[] { "Utf8StreamWriter on File", Utf8StreamWriter.OpenPath(
            bom: false, Path.Combine(Path.GetTempPath(), Path.GetTempFileName()), append: false, bufferSize: 4096) };
    }
    [Benchmark]
    [ArgumentsSource(nameof(GetTextWriter))]
    public object Text(string description, StreamWriter writer)
    {
        writer.WriteLine("Hello World!");
        writer.WriteLine("The quick brown fox jumps over the lazy dog");
        writer.Flush();
        writer.BaseStream.SetLength(0);

        return writer;
    }
    [Benchmark]
    [ArgumentsSource(nameof(GetTextWriter))]
    public object Values(string description, StreamWriter writer)
    {
        writer.WriteLine(DateTime.Now);
        writer.WriteLine(int.MaxValue);
        writer.WriteLine(long.MaxValue);
        writer.WriteLine(Math.PI);
        writer.Flush();
        writer.BaseStream.SetLength(0);

        return writer;
    }
}

public partial class AsyncBenchmark
{
    [Benchmark]
    public async Task<object> TextAsyncUtf8()
    {
        var writer = new Utf8StreamWriter(new MemoryStream(), bom: false, bufferSize: 4096, leaveOpen: false);
        await writer.WriteLineAsyncSlim("Hello World!".AsMemory());
        await writer.WriteLineAsyncSlim("The quick brown fox jumps over the lazy dog".AsMemory());
        await writer.FlushAsyncSlim();
        writer.BaseStream.SetLength(0);

        return writer;
    }
    [Benchmark]
    public async Task<object> TextAsync()
    {
        var writer = new StreamWriter(new MemoryStream(), Encoding.UTF8, bufferSize: 4096, leaveOpen: false);
        await writer.WriteLineAsync("Hello World!");
        await writer.WriteLineAsync("The quick brown fox jumps over the lazy dog");
        await writer.FlushAsync();
        writer.BaseStream.SetLength(0);

        return writer;
    }
}
