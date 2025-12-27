// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// HashCode polyfill for .NET Standard 2.0

#if NETSTANDARD2_0

using System.Collections.Generic;
using System.ComponentModel;

namespace System
{
    /// <summary>
    /// Combines hash codes in an order-dependent manner.
    /// </summary>
    internal struct HashCode
    {
        private int _hashCode;

        public static int Combine<T1>(T1 value1)
        {
            return EqualityComparer<T1>.Default.GetHashCode(value1!);
        }

        public static int Combine<T1, T2>(T1 value1, T2 value2)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + EqualityComparer<T1>.Default.GetHashCode(value1!);
                hash = hash * 31 + EqualityComparer<T2>.Default.GetHashCode(value2!);
                return hash;
            }
        }

        public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + EqualityComparer<T1>.Default.GetHashCode(value1!);
                hash = hash * 31 + EqualityComparer<T2>.Default.GetHashCode(value2!);
                hash = hash * 31 + EqualityComparer<T3>.Default.GetHashCode(value3!);
                return hash;
            }
        }

        public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + EqualityComparer<T1>.Default.GetHashCode(value1!);
                hash = hash * 31 + EqualityComparer<T2>.Default.GetHashCode(value2!);
                hash = hash * 31 + EqualityComparer<T3>.Default.GetHashCode(value3!);
                hash = hash * 31 + EqualityComparer<T4>.Default.GetHashCode(value4!);
                return hash;
            }
        }

        public void Add<T>(T value)
        {
            unchecked
            {
                _hashCode = _hashCode * 31 + EqualityComparer<T>.Default.GetHashCode(value!);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => ToHashCode();

        public int ToHashCode() => _hashCode == 0 ? 17 : _hashCode;
    }
}

#endif
