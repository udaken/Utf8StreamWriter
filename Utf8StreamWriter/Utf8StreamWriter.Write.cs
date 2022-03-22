using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers.Text;
using System.Runtime.CompilerServices;

namespace IO.Specialized
{
    partial class Utf8StreamWriter
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Write(Boolean value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: false, Formats.Boolean);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void WriteLine(Boolean value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: true, Formats.Boolean);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Write(Int32 value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: false, Formats.Int32);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void WriteLine(Int32 value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: true, Formats.Int32);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Write(Int64 value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: false, Formats.Int64);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void WriteLine(Int64 value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: true, Formats.Int64);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Write(Single value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: false, Formats.Single);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void WriteLine(Single value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: true, Formats.Single);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Write(Double value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: false, Formats.Double);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void WriteLine(Double value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: true, Formats.Double);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Write(Decimal value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: false, Formats.Decimal);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void WriteLine(Decimal value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: true, Formats.Decimal);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Write(UInt32 value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: false, Formats.UInt32);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void WriteLine(UInt32 value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: true, Formats.UInt32);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Write(UInt64 value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: false, Formats.UInt64);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void WriteLine(UInt64 value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: true, Formats.UInt64);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Write(DateTime value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: false, Formats.DateTime);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteLine(DateTime value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: true, Formats.DateTime);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Write(DateTimeOffset value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: false, Formats.DateTimeOffset);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteLine(DateTimeOffset value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: true, Formats.DateTimeOffset);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Write(TimeSpan value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: false, Formats.TimeSpan);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteLine(TimeSpan value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: true, Formats.TimeSpan);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Write(Guid value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: false, Formats.Guid);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteLine(Guid value)
        {
           WritePrimitive(value, Utf8Formatter.TryFormat, appendNewLine: true, Formats.Guid);
        }

        public void Write(Half value)
        {
            WritePrimitive((float)value, Utf8Formatter.TryFormat, appendNewLine: false, Formats.Half);
        }
        
        public void WriteLine(Half value)
        {
            WritePrimitive((float)value, Utf8Formatter.TryFormat, appendNewLine: true, Formats.Half);
        }
        
        public void Write(DateOnly value)
        {
            WritePrimitive(value.ToDateTime(default), Utf8Formatter.TryFormat, appendNewLine: false, Formats.Half);
        }
        
        public void WriteLine(DateOnly value)
        {
            WritePrimitive(value.ToDateTime(default), Utf8Formatter.TryFormat, appendNewLine: true, Formats.Half);
        }
        
        public void Write(TimeOnly value)
        {
            WritePrimitive(value.ToTimeSpan(), Utf8Formatter.TryFormat, appendNewLine: false, Formats.Half);
        }
        
        public void WriteLine(TimeOnly value)
        {
            WritePrimitive(value.ToTimeSpan(), Utf8Formatter.TryFormat, appendNewLine: true, Formats.Half);
        }


        public override void Write(object? value)
        {
            if(false) {}
            else if (value is bool bool_) { Write( bool_ ); }
            else if (value is int int_) { Write( int_ ); }
            else if (value is long long_) { Write( long_ ); }
            else if (value is float float_) { Write( float_ ); }
            else if (value is double double_) { Write( double_ ); }
            else if (value is decimal decimal_) { Write( decimal_ ); }
            else if (value is uint uint_) { Write( uint_ ); }
            else if (value is ulong ulong_) { Write( ulong_ ); }
            else if (value is DateTime DateTime_) { Write( DateTime_ ); }
            else if (value is DateTimeOffset DateTimeOffset_) { Write( DateTimeOffset_ ); }
            else if (value is TimeSpan TimeSpan_) { Write( TimeSpan_ ); }
            else if (value is Guid Guid_) { Write( Guid_ ); }
            else if (value is DateOnly DateOnly_) { Write( DateOnly_ ); }
            else if (value is TimeOnly TimeOnly_) { Write( TimeOnly_ ); }
            else if (value is Half Half_) { Write( Half_ ); }
            else if (value is string string_) { Write( string_ ); }
            else if (value is ISpanFormattable formattable)
            {
                WriteFormattableInternal(formattable, false);
            }
            else
            {
                base.Write(value);
            }
        }
        
        public override void WriteLine(object? value)
        {
            if(false) {}
            else if (value is bool bool_) { WriteLine( bool_ ); }
            else if (value is int int_) { WriteLine( int_ ); }
            else if (value is long long_) { WriteLine( long_ ); }
            else if (value is float float_) { WriteLine( float_ ); }
            else if (value is double double_) { WriteLine( double_ ); }
            else if (value is decimal decimal_) { WriteLine( decimal_ ); }
            else if (value is uint uint_) { WriteLine( uint_ ); }
            else if (value is ulong ulong_) { WriteLine( ulong_ ); }
            else if (value is DateTime DateTime_) { WriteLine( DateTime_ ); }
            else if (value is DateTimeOffset DateTimeOffset_) { WriteLine( DateTimeOffset_ ); }
            else if (value is TimeSpan TimeSpan_) { WriteLine( TimeSpan_ ); }
            else if (value is Guid Guid_) { WriteLine( Guid_ ); }
            else if (value is DateOnly DateOnly_) { WriteLine( DateOnly_ ); }
            else if (value is TimeOnly TimeOnly_) { WriteLine( TimeOnly_ ); }
            else if (value is Half Half_) { WriteLine( Half_ ); }
            else if (value is string string_) { WriteLine( string_ ); }
            else if (value is ISpanFormattable formattable)
            {
                WriteFormattableInternal(formattable, true);
            }
            else
            {
                base.WriteLine(value);
            }
        }

    }

}
