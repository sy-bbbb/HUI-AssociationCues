using System.Collections.Generic;
using NUnit.Framework;
using Unity.XR.CoreUtils.Collections;
using UnityEngine;

namespace Unity.XR.CoreUtils.Editor.Tests
{
    class ReadOnlyListTests
    {
        [Test]
        public void ToString_SucceedsWithNullItemsInList()
        {
            var list = new List<object> { 1, null, 2 };
            var listReadOnly = new ReadOnlyList<object>(list);
            Debug.Log(listReadOnly.ToString());
            // Test passes if no errors are logged
        }

        [Test]
        public void EqualityOps_BasicTests()
        {
            ReadOnlyList<int> null1 = null;
            ReadOnlyList<int> null2 = null;
            Assert.IsTrue(null1 == null2);
            Assert.IsFalse(null1 != null2);

            var myList1 = new List<int>();
            var myList2 = new List<int>();
            var readOnly1 = new ReadOnlyList<int>(myList1);
            var readOnly2 = new ReadOnlyList<int>(myList2);
            var readOnly1copy = new ReadOnlyList<int>(myList1);

            Assert.IsTrue(readOnly1 == readOnly1copy);
            Assert.IsFalse(readOnly1 != readOnly1copy);
            Assert.IsTrue(readOnly1 != readOnly2);
            Assert.IsFalse(readOnly1 == readOnly2);
        }
    }
}
