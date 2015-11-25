using System;
using System.Collections.Generic;

using FluentAssertions;

using logviewer.core;

using Xunit;

namespace logviewer.tests
{
    public class TstFixedSizeDictionary
    {
        public static IEnumerable<object[]> ValuePairs => new[]
        {
            new object[] { 2, DateTime.Today },
            new object[] { 2, DateTime.MinValue }
        };

        public static IEnumerable<object[]> ReferencePairs => new[]
        {
            new object[] { 2, "test" },
            new object[] { 2, null }
        };

        private static void KeysTest<T>(int key, T value)
        {
            var instance = new FixedSizeDictionary<T>(10);
            instance.Add(key, value);
            instance.Keys.ShouldBeEquivalentTo(new[] { key });
        }
        private static void ValuesTest<T>(int key, T value)
        {
            var instance = new FixedSizeDictionary<T>(10);
            instance.Add(key, value);
            instance.Values.ShouldBeEquivalentTo(new[] { value });
        }

        [Theory, MemberData("ValuePairs")]
        public void ValueTypeKeys(int key, DateTime value)
        {
            KeysTest(key, value);
        }

        [Theory, MemberData("ReferencePairs")]
        public void ReferenceTypeKeys(int key, string value)
        {
            KeysTest(key, value);
        }

        [Theory, MemberData("ValuePairs")]
        public void ValueTypeValues(int key, DateTime value)
        {
            ValuesTest(key, value);
        }

        [Theory, MemberData("ReferencePairs")]
        public void ReferenceTypeValues(int key, string value)
        {
            ValuesTest(key, value);
        }
    }
}
