using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Unity.XR.CoreUtils.Collections
{
    /// <summary>
    /// Wraps a list or array to provide a read-only view of some or all elements. Elements are not copied, so if the
    /// underlying collection changes, the `ReadOnlyListSpan` will see the updated elements.
    /// </summary>
    /// <remarks>
    /// It is preferable to use this collection in API designs instead of `IReadOnlyCollection` because
    /// <see cref="GetEnumerator"/> returns a value-type enumerator and does not perform any heap allocations.
    ///
    /// This collection is not thread-safe.
    /// </remarks>
    /// <typeparam name="T">The element type.</typeparam>
    public struct ReadOnlyListSpan<T> : IReadOnlyList<T>, IEquatable<ReadOnlyListSpan<T>>
    {
        static ReadOnlyListSpan<T> s_EmptyList = new();
        Enumerator m_Enumerator;

        /// <summary>
        /// The number of elements in the read-only list.
        /// </summary>
        /// <value>The number of elements.</value>
        public int Count => m_Enumerator.end - m_Enumerator.start;

        /// <summary>
        /// Returns the element at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index.</param>
        public T this[int index]
        {
            get
            {
                index += m_Enumerator.start;
                if (index < m_Enumerator.start || index >= m_Enumerator.end)
                    throw new ArgumentOutOfRangeException();

                return m_Enumerator.list[index];
            }
        }

        /// <summary>
        /// Constructs a new instance of this class that is a read-only wrapper around the specified list.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is <see langword="null"/>.</exception>
        public ReadOnlyListSpan(IReadOnlyList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            m_Enumerator = new Enumerator(list);
        }

        /// <summary>
        /// Constructs a new instance of this class that is a read-only wrapper around a slice of the specified list.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        /// <param name="start">The zero-based index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown if
        /// start or length are outside the bounds of the list.</exception>
        public ReadOnlyListSpan(IReadOnlyList<T> list, int start, int length)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (start < 0 || start + length > list.Count)
                throw new ArgumentOutOfRangeException();

            m_Enumerator = new Enumerator(list, start, start + length);
        }

        /// <summary>
        /// Modifies the existing list to form a slice starting at a specified index for a specified length.
        /// </summary>
        /// <param name="start">The zero-based index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice.</param>
        /// <returns>A new <see cref="ReadOnlyListSpan{T}"/> that is a read only view of a slice of a list.</returns>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown if
        /// start or length are outside the bounds of the current ReadOnlyListSpan.</exception>
        public ReadOnlyListSpan<T> Slice(int start, int length)
        {
            var newStart = m_Enumerator.start + start;
            if (newStart < m_Enumerator.start || newStart + length > m_Enumerator.end)
                throw new ArgumentOutOfRangeException();

            return new ReadOnlyListSpan<T>(m_Enumerator.list, m_Enumerator.start + start, length);
        }

        /// <summary>
        /// Returns an empty read-only list with the specified type argument.
        /// </summary>
        /// <returns>The empty read-only list.</returns>
        public static ReadOnlyListSpan<T> Empty() => s_EmptyList;

        /// <summary>
        /// Returns an enumerator that iterates through the read-only list.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public Enumerator GetEnumerator() => m_Enumerator;

        /// <summary>
        /// Returns an enumerator that iterates through the read-only list.
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > This implementation performs a boxing operation and should be avoided.
        /// > Use the public <see cref="GetEnumerator"/> overload instead.
        /// </remarks>
        /// <returns>The boxed enumerator.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the read-only list.
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > This implementation performs a boxing operation and should be avoided.
        /// > Use the public <see cref="GetEnumerator"/> overload instead.
        /// </remarks>
        /// <returns>The boxed enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>`true` if the current object is equal to the `other` parameter. Otherwise, `false`.</returns>
        /// <remarks>
        /// Two instances compare equal if they are read-only views of the same list, for the same start and end indices.
        /// </remarks>
        public bool Equals(ReadOnlyListSpan<T> other)
        {
            return ReferenceEquals(m_Enumerator.list, other.m_Enumerator.list)
                && m_Enumerator.start == other.m_Enumerator.start
                && m_Enumerator.end == other.m_Enumerator.end;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>`true` if the current object is equal to the `other` parameter. Otherwise, `false`.</returns>
        public override bool Equals(object obj)
            => obj is ReadOnlyListSpan<T> other && Equals(other);

        /// <summary>
        /// Returns `true` if objects are equal by <see cref="Equals(Unity.XR.CoreUtils.Collections.ReadOnlyListSpan{T})"/>.
        /// Otherwise, `false`.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`true` if objects are equal. Otherwise, `false`.</returns>
        public static bool operator ==(ReadOnlyListSpan<T> lhs, ReadOnlyListSpan<T> rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Returns `false` if objects are equal by <see cref="Equals(Unity.XR.CoreUtils.Collections.ReadOnlyListSpan{T})"/>.
        /// Otherwise, `true`.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`false` if objects are equal. Otherwise, `true`.</returns>
        public static bool operator !=(ReadOnlyListSpan<T> lhs, ReadOnlyListSpan<T> rhs) => !(lhs == rhs);

        /// <summary>
        /// Get a hash code for this object.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
            => HashCode.Combine(m_Enumerator.list, m_Enumerator.start, m_Enumerator.end);

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            for (var i = m_Enumerator.start; i < m_Enumerator.end; i++)
            {
                var item = m_Enumerator.list[i];
                sb.AppendLine(item == null ? "  null," : $"  {m_Enumerator.list[i].ToString()},");
            }
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// Provides an enumerator for the elements of `ReadOnlyListSpan`.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            /// <summary>
            /// The inclusive start index of a slice of the list.
            /// </summary>
            public int start { get; }

            /// <summary>
            /// The exclusive end index of a slice of the list.
            /// </summary>
            public int end { get; }

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException">Thrown if the current position is outside the bounds of
            /// the ReadOnlyListSpan.</exception>
            public T Current
            {
                get
                {
                    if (m_CurrentIndex < start || m_CurrentIndex >= end)
                        throw new ArgumentOutOfRangeException();

                    return list[m_CurrentIndex];
                }
            }

            object IEnumerator.Current => Current;
            internal IReadOnlyList<T> list;
            int m_CurrentIndex;

            internal Enumerator(IReadOnlyList<T> list) : this(list, 0, list.Count) { }

            /// <summary>
            /// Provides an enumerator for a slice of the elements of a list beginning with the
            /// `start` index and ending at the `end` index.
            /// </summary>
            /// <param name="list">The list to enumerate.</param>
            /// <param name="start">The zero-based index at which to begin this slice.</param>
            /// <param name="end">The desired zero-based index at which to end the slice.</param>
            internal Enumerator(IReadOnlyList<T> list, int start, int end)
            {
                this.list = list;
                this.start = start;
                this.end = end;
                m_CurrentIndex = this.start - 1;
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns><see langword="true"/> if the next position is within the bounds of the list. Otherwise, <see langword="false"/>.</returns>
            public bool MoveNext()
            {
                m_CurrentIndex += 1;
                return m_CurrentIndex < end;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset() => m_CurrentIndex = start - 1;

            void IDisposable.Dispose() { }
        }
    }
}
