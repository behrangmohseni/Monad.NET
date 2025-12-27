// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Index and Range polyfills for .NET Standard 2.0

#if NETSTANDARD2_0

using System.Runtime.CompilerServices;

namespace System
{
    internal readonly struct Index : IEquatable<Index>
    {
        private readonly int _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Index(int value, bool fromEnd = false)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative");
            _value = fromEnd ? ~value : value;
        }

        private Index(int value) => _value = value;

        public static Index Start => new Index(0);
        public static Index End => new Index(~0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Index FromStart(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative");
            return new Index(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Index FromEnd(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative");
            return new Index(~value);
        }

        public int Value => _value < 0 ? ~_value : _value;
        public bool IsFromEnd => _value < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffset(int length)
        {
            int offset = _value;
            if (IsFromEnd)
                offset += length + 1;
            return offset;
        }

        public override bool Equals(object? value) => value is Index index && _value == index._value;
        public bool Equals(Index other) => _value == other._value;
        public override int GetHashCode() => _value;
        public static implicit operator Index(int value) => FromStart(value);
        public override string ToString() => IsFromEnd ? "^" + ((uint)Value).ToString() : ((uint)Value).ToString();
    }

    internal readonly struct Range : IEquatable<Range>
    {
        public Index Start { get; }
        public Index End { get; }

        public Range(Index start, Index end)
        {
            Start = start;
            End = end;
        }

        public override bool Equals(object? value) => value is Range r && r.Start.Equals(Start) && r.End.Equals(End);
        public bool Equals(Range other) => other.Start.Equals(Start) && other.End.Equals(End);
        public override int GetHashCode() => Start.GetHashCode() * 31 + End.GetHashCode();
        public override string ToString() => Start.ToString() + ".." + End.ToString();

        public static Range StartAt(Index start) => new Range(start, Index.End);
        public static Range EndAt(Index end) => new Range(Index.Start, end);
        public static Range All => new Range(Index.Start, Index.End);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (int Offset, int Length) GetOffsetAndLength(int length)
        {
            int start = Start.GetOffset(length);
            int end = End.GetOffset(length);
            if ((uint)end > (uint)length || (uint)start > (uint)end)
                throw new ArgumentOutOfRangeException(nameof(length));
            return (start, end - start);
        }
    }
}

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Reserved for use by the compiler for record types.
    /// </summary>
    internal static class IsExternalInit { }
}

#endif
