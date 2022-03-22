// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System;
using System.IO;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IO.Specialized
{
    //
    // 概要:
    //     Implements a System.IO.TextWriter for writing characters to a stream in a particular
    //     encoding.
    public sealed partial class Utf8StreamWriter : StreamWriter
    {
        // For UTF-8, the values of 1K for the default buffer size and 4K for the
        // file stream buffer size are reasonable & give very reasonable
        // performance for in terms of construction time for the Utf8StreamWriter and
        // write perf.  Note that for UTF-8, we end up allocating a 4K byte buffer,
        // which means we take advantage of adaptive buffering code.
        // The performance using UnicodeEncoding is acceptable.
        private const int DefaultBufferSize = 1024;   // byte[]
        private const int DefaultFileStreamBufferSize = 4096;
        private const int MinBufferSize = 128;

        // Bit bucket - Null has no backing store. Non closable.
        public static new readonly Utf8StreamWriter Null = new Utf8StreamWriter(Stream.Null, false, MinBufferSize, leaveOpen: true);
        private readonly Encoder _encoder;
        private readonly byte[] _charBuffer;
        private int _charPos = 0;
        private readonly int _charLen;
        private bool _autoFlush = false;
        private bool _haveWrittenPreamble = false;
        private readonly bool _closable;
        private bool _disposed;
        private readonly Stream _stream;

        [DoesNotReturn]
        private static void ThrowAsyncIOInProgress() => throw new InvalidOperationException();

        private static UTF8Encoding? s_UTF8NoBOM;
        private static UTF8Encoding UTF8NoBOM => s_UTF8NoBOM ??= new UTF8Encoding(false, true);
        private static UTF8Encoding? s_UTF8WithBOM;
        private static UTF8Encoding UTF8WithBOM => s_UTF8WithBOM ??= new UTF8Encoding(true, true);

        public Utf8StreamWriter(Stream stream)
            : this(stream, false, DefaultBufferSize, false)
        {
        }

        public Utf8StreamWriter(Stream stream, bool bom = false, int bufferSize = -1, bool leaveOpen = false)
            : base(stream, bom ? UTF8WithBOM : UTF8NoBOM)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            if (!stream.CanWrite)
            {
                throw new ArgumentException("Stream must can write", nameof(stream));
            }
            if (bufferSize == -1)
            {
                bufferSize = DefaultBufferSize;
            }
            else if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            BaseStream = _stream = stream;
            _encoder = Encoding.GetEncoder();
            if (bufferSize < MinBufferSize)
            {
                bufferSize = MinBufferSize;
            }

            _charBuffer = new byte[bufferSize];
            _charLen = bufferSize;
            // If we're appending to a Stream that already has data, don't write
            // the preamble.
            if ((_stream.CanSeek && _stream.Position > 0) || !bom)
            {
                _haveWrittenPreamble = true;
            }

            _closable = !leaveOpen;
        }

        public static Utf8StreamWriter OpenPath(bool bom, string path)
            => OpenPath(bom, path, false, DefaultBufferSize);

        public static Utf8StreamWriter OpenPath(bool bom, string path, bool append = false, int bufferSize = -1)
            => new(ValidateArgsAndOpenPath(path, append, bufferSize), bom, bufferSize, leaveOpen: false);

        public static Utf8StreamWriter OpenPath(bool bom, string path, FileStreamOptions options)
            => new(ValidateArgsAndOpenPath(path, options), bom, DefaultFileStreamBufferSize);


        private static Stream ValidateArgsAndOpenPath(string path, FileStreamOptions options)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("", nameof(path));

            ArgumentNullException.ThrowIfNull(options, nameof(options));
            if ((options.Access & FileAccess.Write) == 0)
            {
                throw new ArgumentException("Stream must can write", nameof(options));
            }

            return new FileStream(path, options);
        }

        private static Stream ValidateArgsAndOpenPath(string path, bool append, int bufferSize)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("", nameof(path));

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            return new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, DefaultFileStreamBufferSize);
        }

        public override void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                // We need to flush any buffered data if we are being closed/disposed.
                // Also, we never close the handles for stdout & friends.  So we can safely
                // write any buffered data to those streams even during finalization, which
                // is generally the right thing to do.
                if (!_disposed && disposing)
                {
                    // Note: flush on the underlying stream can throw (ex., low disk space)
                    FlushInternal(flushStream: true);
                }
            }
            finally
            {
                CloseStreamFromDispose(disposing);
            }
        }

        private void CloseStreamFromDispose(bool disposing)
        {
            // Dispose of our resources if this Utf8StreamWriter is closable.
            if (_closable && !_disposed)
            {
                try
                {
                    // Attempt to close the stream even if there was an IO error from Flushing.
                    // Note that Stream.Close() can potentially throw here (may or may not be
                    // due to the same Flush error). In this case, we still need to ensure
                    // cleaning up internal resources, hence the finally block.
                    if (disposing)
                    {
                        _stream.Close();
                    }
                }
                finally
                {
                    _disposed = true;
                    _charPos = 0;
                    base.Dispose(disposing);
                }
            }
        }

        public override ValueTask DisposeAsync() =>
                DisposeAsyncCore();

        private async ValueTask DisposeAsyncCore()
        {
            try
            {
                if (!_disposed)
                {
                    await FlushAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                CloseStreamFromDispose(disposing: true);
            }
            GC.SuppressFinalize(this);
        }

        public override void Flush()
        {
            ThrowIfDisposed();

            FlushInternal(true);
        }

        private void FlushInternal(bool flushStream)
        {
            // Perf boost for Flush on non-dirty writers.
            if (_charPos == 0 && !flushStream)
            {
                return;
            }

            if (!_haveWrittenPreamble)
            {
                _haveWrittenPreamble = true;
                ReadOnlySpan<byte> preamble = Encoding.Preamble;
                if (preamble.Length > 0)
                {
                    _stream.Write(preamble);
                }
            }


            if (_charPos > 0)
            {
                _stream.Write(_charBuffer.AsSpan(0, _charPos));
                _charPos = 0;
            }

            if (flushStream)
            {
                _stream.Flush();
            }
        }

        public override bool AutoFlush
        {
            get => _autoFlush;

            set
            {
                ThrowIfDisposed();

                _autoFlush = value;
                if (value)
                {
                    FlushInternal(true);
                }
            }
        }

        public override IFormatProvider FormatProvider => System.Globalization.CultureInfo.InvariantCulture;


        public override Stream BaseStream { get; }

        public bool Bom { get; private set; }

        public void WriteLineFormattable<T>(T value, ReadOnlySpan<char> format = default) where T : ISpanFormattable
        {
            WriteFormattableInternal(value, true, format);
        }

        public void WriteFormattable<T>(T value, ReadOnlySpan<char> format = default) where T : ISpanFormattable
        {
            WriteFormattableInternal(value, false, format);
        }
        private void WriteFormattableInternal<T>(T value, bool appendNewLine, ReadOnlySpan<char> format = default) where T : ISpanFormattable
        {
            ThrowIfDisposed();

            char[]? array = null;
            try
            {
                Span<char> buf = stackalloc char[MinBufferSize];
                int charsWrittern = 0;
                while (!value.TryFormat(buf, out charsWrittern, format, FormatProvider))
                {
                    if (array != null)
                        ArrayPool<char>.Shared.Return(array);

                    buf = array = ArrayPool<char>.Shared.Rent(buf.Length * 2);
                }

                WriteCharSpan(buf[..charsWrittern], false);
            }
            finally
            {
                if (array != null)
                    ArrayPool<char>.Shared.Return(array);
            }

            if (appendNewLine)
                WriteLineInternal();

            if (_autoFlush)
            {
                FlushInternal(true);
            }
        }

        public override void Write(StringBuilder? value)
        {
            WriteInternal(value, false);
        }

        public override void WriteLine(StringBuilder? value)
        {
            WriteInternal(value, true);
        }

        private void WriteInternal(StringBuilder? value, bool appendNewLine)
        {
            if (value == null && !appendNewLine)
                return;

            ThrowIfDisposed();

            if (value != null)
            {
                foreach (var chunk in value.GetChunks())
                {
                    WriteCharSpan(chunk.Span, false);
                }
            }

            if (appendNewLine)
                WriteLineInternal();

            if (_autoFlush)
            {
                FlushInternal(true);
            }
        }

        public override void Write(char value)
        {
            ThrowIfDisposed();

            if (_charPos == _charLen)
            {
                FlushInternal(false);
            }

            if (value <= 0x7F)
            {
                _charBuffer[_charPos++] = (byte)value;
            }
            else
            {
                WriteCharSpan(stackalloc char[1] { value }, false);
            }

            if (_autoFlush)
            {
                FlushInternal(true);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // prevent WriteSpan from bloating call sites
        public override void Write(char[]? buffer)
        {
            WriteCharSpan(buffer, appendNewLine: false);
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // prevent WriteSpan from bloating call sites
        public override void Write(char[] buffer, int index, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (buffer.Length - index < count)
            {
                throw new ArgumentException("Invalid length");
            }

            WriteCharSpan(buffer.AsSpan(index, count), appendNewLine: false);
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // prevent WriteSpan from bloating call sites
        public override void Write(ReadOnlySpan<char> buffer)
        {
            WriteCharSpan(buffer, appendNewLine: false);
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // prevent WriteSpan from bloating call sites
        public override void Write(string? value)
        {
            WriteCharSpan(value, appendNewLine: false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteLineInternal()
        {
            char[] coreNewLine = CoreNewLine;
            for (int i = 0; i < coreNewLine.Length; i++) // Generally 1 (\n) or 2 (\r\n) iterations
            {
                if (_charPos == _charLen)
                {
                    FlushInternal(false);
                }

                _charBuffer[_charPos++] = (byte)coreNewLine[i];
            }
        }

        private delegate bool TryFormat<in T>(T value, Span<byte> destination, out int bytesWritten, StandardFormat format);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WritePrimitive<T>(T value, TryFormat<T> formatter, bool appendNewLine, StandardFormat format = default)
        {
            ThrowIfDisposed();

            if (formatter(value, _charBuffer.AsSpan()[_charPos..], out var written, format))
            {
                _charPos += written;
            }
            else
            {
                FlushInternal(false);
                if (formatter(value, _charBuffer.AsSpan()[_charPos..], out written, format))
                {
                    _charPos += written;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            if (appendNewLine)
                WriteLineInternal();

            if (_autoFlush)
            {
                FlushInternal(true);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteSpan(ReadOnlySpan<byte> buffer, bool appendNewLine)
        {
            if (buffer.IsEmpty && !appendNewLine)
                return;

            ThrowIfDisposed();

            int copied = 0;
            while (copied < buffer.Length)
            {
                if (_charPos == _charLen)
                {
                    FlushInternal(flushStream: false);
                }

                int copyLength = Math.Min(buffer.Length - copied, _charLen - _charPos);

                buffer.Slice(copied, copyLength).CopyTo(_charBuffer.AsSpan()[_charPos..]);
                _charPos += copyLength;
                copied += copyLength;
            }

            if (appendNewLine)
            {
                WriteLineInternal();
            }

            if (_autoFlush)
            {
                FlushInternal(true);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteCharSpan(ReadOnlySpan<char> buffer, bool appendNewLine)
        {
            if (buffer.IsEmpty && !appendNewLine)
                return;

            ThrowIfDisposed();

            int copied = 0;
            while (copied < buffer.Length)
            {
                if (_charPos == _charLen)
                {
                    FlushInternal(flushStream: false);
                }

                int n = Math.Min(buffer.Length - copied, (_charLen - _charPos) / Encoding.GetMaxByteCount(1));
                int count = _encoder.GetBytes(buffer.Slice(copied, n), _charBuffer.AsSpan()[_charPos..], flush: false);
                if (count > 0)
                {
                    _charPos += count;
                    copied += n;
                }
                else
                {
                    FlushInternal(flushStream: false);
                }
            }

            _encoder.Reset();

            if (appendNewLine)
            {
                WriteLineInternal();
            }

            if (_autoFlush)
            {
                FlushInternal(true);
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)] // prevent WriteCharSpan from bloating call sites
        public override void WriteLine()
        {
            WriteCharSpan(default, appendNewLine: true);
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // prevent WriteCharSpan from bloating call sites
        public override void WriteLine(string? value)
        {
            WriteCharSpan(value, appendNewLine: true);
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // prevent WriteCharSpan from bloating call sites
        public override void WriteLine(ReadOnlySpan<char> buffer)
        {
            WriteCharSpan(buffer, appendNewLine: true);
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // prevent WriteCharSpan from bloating call sites
        public override void WriteLine(char value)
        {
            WriteCharSpan(stackalloc char[] { value }, appendNewLine: true);
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // prevent WriteSpan from bloating call sites
        public override void WriteLine(char[]? buffer)
        {
            WriteCharSpan(buffer, appendNewLine: true);
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // prevent WriteSpan from bloating call sites
        public override void WriteLine(char[] buffer, int index, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (buffer.Length - index < count)
            {
                throw new ArgumentException("Invalid length");
            }

            WriteCharSpan(buffer.AsSpan(index, count), appendNewLine: true);
        }

        #region Async operations

        private async ValueTask WriteLineAsyncInternal(CancellationToken cancellationToken)
        {
            char[] coreNewLine = CoreNewLine;

            for (int i = 0; i < coreNewLine.Length; i++) // Generally 1 (\n) or 2 (\r\n) iterations
            {
                if (_charPos == _charLen)
                {
                    await FlushAsyncInternal(flushStream: false, cancellationToken).ConfigureAwait(false);
                }

                Debug.Assert(coreNewLine[i] <= 0x7F);

                _charBuffer[_charPos++] = (byte)coreNewLine[i];
            }
        }

        public override Task WriteAsync(char value)
        {
            ThrowIfDisposed();

            var task = WriteCharAsyncInternal(value, appendNewLine: false);
            return task.AsTask();
        }

        private async ValueTask WriteCharAsyncInternal(char value, bool appendNewLine, CancellationToken cancellationToken = default)
        {
            if (value <= 0x7F)
            {
                if (_charPos == _charLen)
                {
                    await FlushAsyncInternal(flushStream: false, cancellationToken).ConfigureAwait(false);
                }

                _charBuffer[_charPos++] = (byte)value;

                if (appendNewLine)
                {
                    await WriteLineAsyncInternal(cancellationToken).ConfigureAwait(false);
                }

                if (_autoFlush)
                {
                    await FlushAsyncInternal(flushStream: true, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                var array = ArrayPool<char>.Shared.Rent(1);
                array[0] = value;
                await WriteAsyncInternal(array, appendNewLine, cancellationToken).ConfigureAwait(false);
                ArrayPool<char>.Shared.Return(array);
            }
        }

        public override Task WriteAsync(string? value)
        {
            if (value == null)
            {
                return Task.CompletedTask;
            }
            return WriteAsync(value.AsMemory());
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (buffer.Length - index < count)
            {
                throw new ArgumentException("Invalid length");
            }
            return WriteAsync(new ReadOnlyMemory<char>(buffer, index, count));
        }

        public ValueTask WriteAsyncSlim(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            if (buffer.IsEmpty)
            {
                return ValueTask.CompletedTask;
            }

            ThrowIfDisposed();

            if (cancellationToken.IsCancellationRequested)
            {
                return ValueTask.FromCanceled(cancellationToken);
            }

            return WriteAsyncInternal(buffer, appendNewLine: false, cancellationToken: cancellationToken);
        }

        public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            return WriteAsyncSlim(buffer, cancellationToken).AsTask();
        }

        private async ValueTask WriteAsyncInternal(ReadOnlyMemory<char> buffer, bool appendNewLine, CancellationToken cancellationToken)
        {
            int copied = 0;
            while (copied < buffer.Length)
            {
                if (_charPos == _charLen)
                {
                    await FlushAsyncInternal(flushStream: false, cancellationToken).ConfigureAwait(false);
                }

                int n = Math.Min(buffer.Length - copied, (_charLen - _charPos) / Encoding.GetMaxByteCount(1));
                int count = _encoder.GetBytes(buffer.Span.Slice(copied, n), _charBuffer.AsSpan()[_charPos..], flush: false);
                if (count > 0)
                {
                    _charPos += count;
                    copied += n;
                }
                else
                {
                    await FlushAsyncInternal(flushStream: false, cancellationToken).ConfigureAwait(false);
                }
            }

            _encoder.Reset();

            if (appendNewLine)
            {
                await WriteLineAsyncInternal(cancellationToken).ConfigureAwait(false);
            }

            if (_autoFlush)
            {
                await FlushAsyncInternal(flushStream: true, cancellationToken).ConfigureAwait(false);
            }
        }

        public override Task WriteLineAsync()
        {
            return WriteLineAsyncSlim().AsTask();
        }

        public ValueTask WriteLineAsyncSlim(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (cancellationToken.IsCancellationRequested)
            {
                return ValueTask.FromCanceled(cancellationToken);
            }

            return WriteAsyncInternal(ReadOnlyMemory<char>.Empty, appendNewLine: true, cancellationToken);
        }

        public override Task WriteLineAsync(char value)
        {
            ThrowIfDisposed();

            var task = WriteCharAsyncInternal(value, appendNewLine: true);
            return task.AsTask();
        }

        public override Task WriteLineAsync(string? value)
        {
            if (value == null)
            {
                return WriteLineAsync();
            }

            return WriteLineAsync(value.AsMemory());
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (buffer.Length - index < count)
            {
                throw new ArgumentException("Invalid length");
            }
            return WriteLineAsync(new ReadOnlyMemory<char>(buffer, index, count), cancellationToken: default);
        }

        public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            return WriteLineAsyncSlim(buffer, cancellationToken).AsTask();
        }

        public ValueTask WriteLineAsyncSlim(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            if (buffer.IsEmpty)
            {
                return WriteLineAsyncSlim(cancellationToken);
            }

            ThrowIfDisposed();

            if (cancellationToken.IsCancellationRequested)
            {
                return ValueTask.FromCanceled(cancellationToken);
            }

            return WriteAsyncInternal(buffer, appendNewLine: true, cancellationToken: cancellationToken);
        }

        public ValueTask FlushAsyncSlim(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            return FlushAsyncInternal(flushStream: true, cancellationToken: cancellationToken);
        }

        public override Task FlushAsync()
        {
            return FlushAsyncSlim().AsTask();
        }

        private ValueTask FlushAsyncInternal(bool flushStream, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new ValueTask(Task.FromCanceled(cancellationToken));
            }

            // Perf boost for Flush on non-dirty writers.
            if (_charPos == 0 && !flushStream)
            {
                return default(ValueTask);
            }

            return Core(flushStream, cancellationToken);

            async ValueTask Core(bool flushStream, CancellationToken cancellationToken)
            {
                if (!_haveWrittenPreamble)
                {
                    _haveWrittenPreamble = true;
                    byte[] preamble = Encoding.GetPreamble();
                    if (preamble.Length > 0)
                    {
                        await _stream.WriteAsync(new ReadOnlyMemory<byte>(preamble), cancellationToken).ConfigureAwait(false);
                    }
                }

                if (_charPos > 0)
                {
                    await _stream.WriteAsync(new ReadOnlyMemory<byte>(_charBuffer, 0, _charPos), cancellationToken).ConfigureAwait(false);
                    _charPos = 0;
                }

                // By definition, calling Flush should flush the stream, but this is
                // only necessary if we passed in true for flushStream.  The Web
                // Services guys have some perf tests where flushing needlessly hurts.
                if (flushStream)
                {
                    await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
        #endregion

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                ThrowObjectDisposedException();
            }

            [DoesNotReturn]
            void ThrowObjectDisposedException() => throw new ObjectDisposedException(GetType().Name);
        }

        public Formats Formats { get; init; } = new Formats();
    }

    public class Formats
    {
        public StandardFormat Boolean { get; set; }
        public StandardFormat Int32 { get; set; }
        public StandardFormat Int64 { get; set; }
        public StandardFormat Double { get; set; }
        public StandardFormat Single { get; set; }
        public StandardFormat Decimal { get; set; }
        public StandardFormat UInt32 { get; set; }
        public StandardFormat UInt64 { get; set; }
        public StandardFormat DateTime { get; set; }
        public StandardFormat DateTimeOffset { get; set; }
        public StandardFormat TimeSpan { get; set; }
        public StandardFormat Guid { get; set; }
        public StandardFormat Half { get; set; }
        public StandardFormat DateOnly { get; set; }
        public StandardFormat TimeOnly { get; set; }
    }
}