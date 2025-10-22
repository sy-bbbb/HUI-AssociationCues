using System;
using NUnit.Framework;
using UnityEngine;
using Unity.XR.CoreUtils.Collections;
using System.Collections.Generic;

namespace Unity.XR.CoreUtils.Editor.Tests
{
    class ReadOnlyListSpanTests
    {
        [Test]
        public void TestCreatingReadOnlyListSpanWithNullList()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var readOnlyListSpan = new ReadOnlyListSpan<int>(null);
            });
        }

        [Test]
        public void TestCreatingReadOnlyListSpanWithEmptyList()
        {
            var readOnlyListSpan = new ReadOnlyListSpan<int>(new List<int>());

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var slice = readOnlyListSpan.Slice(0, 1);
            });

            using var enumerator = readOnlyListSpan.GetEnumerator();
            Assert.AreEqual(false, enumerator.MoveNext());

            Assert.AreEqual(0, readOnlyListSpan.Count);
        }

        [Test]
        public void TestEnumeratorIteratesCorrectlyWithDefaultConstructor()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            var readOnlyListSpan = new ReadOnlyListSpan<int>(list);
            using var enumerator = readOnlyListSpan.GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Current);

            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void TestEnumeratorIteratesCorrectlyWithSliceConstructor()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            var readOnlyListSpan = new ReadOnlyListSpan<int>(list, 1, 1);
            using var enumerator = readOnlyListSpan.GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);

            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void TestEnumeratorIteratesCorrectlyWithSliceMethod()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            var readOnlyListSpan = new ReadOnlyListSpan<int>(list);
            var slice = readOnlyListSpan.Slice(1, 1);
            using var enumerator = slice.GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);

            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void TestEnumeratorThrowsExceptionWhenAccessingCurrentWithoutMoveNext()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            var readOnlyListSpan = new ReadOnlyListSpan<int>(list);
            using var enumerator = readOnlyListSpan.GetEnumerator();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = enumerator.Current;
            });
        }

        [Test]
        public void TestEnumeratorThrowsExceptionWhenAccessingCurrentAfterEnd()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            var readOnlyListSpan = new ReadOnlyListSpan<int>(list);
            using var enumerator = readOnlyListSpan.GetEnumerator();

            enumerator.MoveNext();
            enumerator.MoveNext();
            enumerator.MoveNext();

            Assert.False(enumerator.MoveNext());
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = enumerator.Current;
            });
        }

        [Test]
        public void TestEnumeratorThrowsExceptionWhenAccessingCurrentAfterSliceEnd()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(5);

            var readOnlyListSpan = new ReadOnlyListSpan<int>(list, 1, 2);
            using var enumerator = readOnlyListSpan.GetEnumerator();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = enumerator.Current;
            });

            enumerator.MoveNext();
            enumerator.MoveNext();
            enumerator.MoveNext();

            Assert.False(enumerator.MoveNext());
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = enumerator.Current;
            });
        }

        [Test]
        public void TestEnumeratorResetsToBeginningWhenResetCalled()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(5);

            var readOnlyListSpan = new ReadOnlyListSpan<int>(list, 1, 2);
            using var enumerator = readOnlyListSpan.GetEnumerator();

            enumerator.MoveNext();
            enumerator.MoveNext();
            enumerator.Reset();

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);
        }

        [Test]
        public void TestEnumeratorCanIterateOverEmptyCollection()
        {
            var list = new List<int>();
            var readOnlyListSpan = new ReadOnlyListSpan<int>(list);
            using var enumerator = readOnlyListSpan.GetEnumerator();

            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void TestIndexingIsReletiveToSlicedList()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(5);

            var readOnlyListSpan = new ReadOnlyListSpan<int>(list, 1, 2);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = readOnlyListSpan[-1];
            });

            Assert.AreEqual(2, readOnlyListSpan[0]);
            Assert.AreEqual(3, readOnlyListSpan[1]);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = readOnlyListSpan[2];
            });
        }

        [Test]
        public void TestOneEnumeratorDoesNotAffectAnotherEnumerator()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            var readOnlyListSpan = new ReadOnlyListSpan<int>(list);
            using var enumerator1 = readOnlyListSpan.GetEnumerator();

            enumerator1.MoveNext();
            enumerator1.MoveNext();

            using var enumerator2 = readOnlyListSpan.GetEnumerator();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = enumerator2.Current;
            });

            enumerator2.MoveNext();
            Assert.AreNotEqual(enumerator1.Current, enumerator2.Current);

            Assert.AreEqual(1, enumerator2.Current);
            Assert.AreEqual(2, enumerator1.Current);
        }

        [Test]
        public void TestSlicingAnAlreadySlicedListThrowsExceptionIfOutsideRange()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(5);

            var readOnlyListSpan = new ReadOnlyListSpan<int>(list, 2, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var anotherReadOnlyListSpan = readOnlyListSpan.Slice(1, 3);
            });
        }

        [Test]
        public void TestSlicingArrayToSameSizeAsOriginalList()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            var readOnlyListSpan = new ReadOnlyListSpan<int>(list, 0, 3);
            var anotherReadOnlyLisSpan = readOnlyListSpan.Slice(0, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var andAnotherReadOnlyListSpan = readOnlyListSpan.Slice(0, 4);
            });
        }

        [Test]
        public void TestSliceReturnsNewReadOnlyList()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(5);

            var readOnlyListSpan = new ReadOnlyListSpan<int>(list);
            var slice = readOnlyListSpan.Slice(1, 3);
            Assert.AreNotEqual(readOnlyListSpan.Count, slice.Count);

            var slice2 = slice.Slice(1, 2);
            Assert.AreNotEqual(slice.Count, slice2.Count);

            using var sliceEnumerator = slice.GetEnumerator();
            sliceEnumerator.MoveNext();
            Assert.AreEqual(2, sliceEnumerator.Current);

            using var slice2Enumerator = slice2.GetEnumerator();
            slice2Enumerator.MoveNext();
            Assert.AreEqual(3, slice2Enumerator.Current);
        }

        [Test]
        public void TestConstructorWithStartAndLengthOutsideRangeOfList()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var readOnlyListSpan = new ReadOnlyListSpan<int>(list, -1, 2);
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var anotherReadOnlyListSpan = new ReadOnlyListSpan<int>(list, 0, 4);
            });
        }

        [Test]
        public void ToString_SucceedsWithNullItemsInList()
        {
            var list = new List<object> { 1, null, 2 };
            var readOnlySpan = new ReadOnlyListSpan<object>(list);
            Debug.Log(readOnlySpan.ToString());
            // test passes if no error are logged
        }
    }
}
