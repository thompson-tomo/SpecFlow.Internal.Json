﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CS0649 // not production code
#pragma warning disable CS0414 // not production code

namespace SpecFlow.Internal.Json.Tests
{
    [TestClass]
    public class TestParser
    {
        public enum Color
        {
            Red,
            Green,
            Blue,
            Yellow
        }

        [Flags]
        public enum Style
        {
            None = 0,
            Bold = 1,
            Italic = 2,
            Underline = 4,
            Strikethrough = 8
        }

        static void Test<T>(T expected, string json)
        {
            T value = json.FromJson<T>();
            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void TestValues()
        {
            Test(12345, "12345");
            Test(12345L, "12345");
            Test(12345UL, "12345");
            Test(12.532f, "12.532");
            Test(12.532m, "12.532");
            Test(12.532d, "12.532");
            Test("hello", "\"hello\"");
            Test("hello there", "\"hello there\"");
            Test("hello\nthere", "\"hello\nthere\"");
            Test("hello\"there", "\"hello\\\"there\"");
            Test(true, "true");
            Test(false, "false");
            Test<object>(null, "sfdoijsdfoij");
            Test(Color.Green, "\"Green\"");
            Test(Color.Blue, "2");
            Test(Color.Blue, "\"2\"");
            Test(Color.Red, "\"sfdoijsdfoij\"");
            Test(Style.Bold | Style.Italic, "\"Bold, Italic\"");
            Test(Style.Bold | Style.Italic, "3");
            Test("\u94b1\u4e0d\u591f!", "\"\u94b1\u4e0d\u591f!\"");
            Test("\u94b1\u4e0d\u591f!", "\"\\u94b1\\u4e0d\\u591f!\"");
        }

        static void ArrayTest<T>(T[] expected, string json)
        {
            var value = (T[])json.FromJson<T[]>();
            CollectionAssert.AreEqual(expected, value);
        }

        [TestMethod]
        public void TestArrayOfValues()
        {
            ArrayTest(new string[] { "one", "two", "three" }, "[\"one\",\"two\",\"three\"]");
            ArrayTest(new int[] { 1, 2, 3 }, "[1,2,3]");
            ArrayTest(new bool[] { true, false, true }, "     [true    ,    false,true     ]   ");
            ArrayTest(new object[] { null, null }, "[null,null]");
            ArrayTest(new float[] { 0.24f, 1.2f }, "[0.24,1.2]");
            ArrayTest(new double[] { 0.15, 0.19 }, "[0.15, 0.19]");
            ArrayTest<object>(null, "[garbled");
        }

        static void ListTest<T>(List<T> expected, string json)
        {
            var value = json.FromJson<List<T>>();
            CollectionAssert.AreEqual(expected, value);
        }

        [TestMethod]
        public void TestListOfValues()
        {
            ListTest(new List<string> { "one", "two", "three" }, "[\"one\",\"two\",\"three\"]");
            ListTest(new List<int> { 1, 2, 3 }, "[1,2,3]");
            ListTest(new List<bool> { true, false, true }, "     [true    ,    false,true     ]   ");
            ListTest(new List<object> { null, null }, "[null,null]");
            ListTest(new List<float> { 0.24f, 1.2f }, "[0.24,1.2]");
            ListTest(new List<double> { 0.15, 0.19 }, "[0.15, 0.19]");
            ListTest<object>(null, "[garbled");
        }

        [TestMethod]
        public void TestRecursiveLists()
        {
            var expected = new List<List<int>> { new List<int> { 1, 2 }, new List<int> { 3, 4 } };
            var actual = "[[1,2],[3,4]]".FromJson<List<List<int>>>();
            Assert.AreEqual(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
                CollectionAssert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void TestRecursiveArrays()
        {
            var expected = new int[][] { new int[] { 1, 2 }, new int[] { 3, 4 } };
            var actual = "[[1,2],[3,4]]".FromJson<int[][]>();
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
                CollectionAssert.AreEqual(expected[i], actual[i]);
        }

        static void DictTest<K, V>(Dictionary<K, V> expected, string json)
        {
            var value = json.FromJson<Dictionary<K, V>>();
            Assert.AreEqual(expected.Count, value.Count);
            foreach (var pair in expected)
            {
                Assert.IsTrue(value.ContainsKey(pair.Key));
                Assert.AreEqual(pair.Value, value[pair.Key]);
            }
        }

        [TestMethod]
        public void TestDictionary()
        {
            DictTest(new Dictionary<string, int> { { "foo", 5 }, { "bar", 10 }, { "baz", 128 } }, "{\"foo\":5,\"bar\":10,\"baz\":128}");
            Assert.IsNull("{0:5}".FromJson<Dictionary<int, int>>());

            DictTest(new Dictionary<string, float> { { "foo", 5f }, { "bar", 10f }, { "baz", 128f } }, "{\"foo\":5,\"bar\":10,\"baz\":128}");
            DictTest(new Dictionary<string, string> { { "foo", "\"" }, { "bar", "hello" }, { "baz", "," } }, "{\"foo\":\"\\\"\",\"bar\":\"hello\",\"baz\":\",\"}");
        }

        [TestMethod]
        public void TestRecursiveDictionary()
        {
            var result = "{\"foo\":{ \"bar\":\"\\\"{,,:[]}\" }}".FromJson<Dictionary<string, Dictionary<string, string>>>();
            Assert.AreEqual("\"{,,:[]}", result["foo"]["bar"]);
        }

        class SimpleObject
        {
            public int A;
            public float B;
            public string C { get; set; }
            public List<int> D { get; set; }
        }

        [TestMethod]
        public void TestSimpleObject()
        {
            SimpleObject value = "{\"A\":123,\"b\":456,\"C\":\"789\",\"D\":[10,11,12]}".FromJson<SimpleObject>();
            Assert.IsNotNull(value);
            Assert.AreEqual(123, value.A);
            Assert.AreEqual(456f, value.B);
            Assert.AreEqual("789", value.C);
            CollectionAssert.AreEqual(new List<int> { 10, 11, 12 }, value.D);

            value = "dfpoksdafoijsdfij".FromJson<SimpleObject>();
            Assert.IsNull(value);
        }

        struct SimpleStruct
        {
            public SimpleObject Obj;
        }

        [TestMethod]
        public void TestSimpleStruct()
        {
            SimpleStruct value = "{\"obj\":{\"A\":12345}}".FromJson<SimpleStruct>();
            Assert.IsNotNull(value.Obj);
            Assert.AreEqual(value.Obj.A, 12345);
        }

        struct TinyStruct
        {
            public int Value;
        }

        [TestMethod]
        public void TestListOfStructs()
        {
            var values = "[{\"Value\":1},{\"Value\":2},{\"Value\":3}]".FromJson<List<TinyStruct>>();
            for (int i = 0; i < values.Count; i++)
                Assert.AreEqual(i + 1, values[i].Value);
        }

        class TestObject2
        {
            public TestObject2 A;
            public List<TestObject2> B;
            public SimpleStruct C;
        }

        [TestMethod]
        public void TestDeepObject()
        {
            var value = "{\"A\":{\"A\":{\"A\":{}}}}".FromJson<TestObject2>();
            Assert.IsNotNull(value);
            Assert.IsNotNull(value.A);
            Assert.IsNotNull(value.A.A);
            Assert.IsNotNull(value.A.A.A);

            value = "{\"B\":[{},null,{\"A\":{}}]}".FromJson<TestObject2>();
            Assert.IsNotNull(value);
            Assert.IsNotNull(value.B);
            Assert.IsNotNull(value.B[0]);
            Assert.IsNull(value.B[1]);
            Assert.IsNotNull(value.B[2].A);

            value = "{\"C\":{\"Obj\":{\"A\":5}}}".FromJson<TestObject2>();
            Assert.IsNotNull(value);
            Assert.IsNotNull(value.C.Obj);
            Assert.AreEqual(5, value.C.Obj.A);
        }

        class TestObject3
        {
            public int A, B, C, D, E, F;
            public TestObject3 Z { get; set; }
        }

        [TestMethod]
        public void PerformanceTest()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            const int numTests = 100000;
            for (int i = 0; i < numTests; i++)
            {
                builder.Append("{\"Z\":{\"F\":10}}");
                if (i < numTests - 1)
                    builder.Append(",");
            }
            builder.Append("]");

            string json = builder.ToString();
            var result = json.FromJson<List<TestObject3>>();
            for (int i = 0; i < result.Count; i++)
                Assert.AreEqual(10, result[i].Z.F);
        }

        [TestMethod]
        public void CorruptionTest()
        {
            "{{{{{{[[[]]][[,,,,]],],],]]][[nulldsfoijsfd[[]]]]]]]]]}}}}}{{{{{{{{{D{FD{FD{F{{{{{}}}XXJJJI%&:,,,,,".FromJson<object>();
            "[[,[,,[,:::[[[[[[[".FromJson<List<List<int>>>();
            "{::,[][][],::::,}".FromJson<Dictionary<string, object>>();
        }

        [TestMethod]
        public void DynamicParserTest()
        {
            List<object> list = (List<object>)("[0,1,2,3]".FromJson<object>());
            Assert.IsTrue(list.Count == 4 && (int)list[3] == 3);
            Dictionary<string, object> obj = (Dictionary<string, object>)("{\"Foo\":\"Bar\"}".FromJson<object>());
            Assert.IsTrue((string)obj["Foo"] == "Bar");

            string testJson = "{\"A\":123,\"B\":456,\"C\":\"789\",\"D\":[10,11,12]}";
            var expected = new Dictionary<string, object>
            {
                {"A", 123},
                {"B", 456},
                {"C", "789"},
                {"D", new List<object>() {10, 11, 12} }
            };

            var actual = testJson.FromJson<Dictionary<string, object>>();

            var dictEqual = DictionariesEqual(expected, actual);
            Assert.IsTrue(dictEqual);
        }

        private bool DictionariesEqual(Dictionary<string, object> dict1, Dictionary<string, object> dict2)
        {
            foreach (var pair in dict1)
            {
                if (!dict2.ContainsKey(pair.Key)) throw new Exception($"missing key: {pair.Key}");

                var dict2Value = dict2[pair.Key];

                if (pair.Value.GetType() == typeof(Dictionary<string, object>))
                {
                    DictionariesEqual(pair.Value as Dictionary<string, object>, dict2Value as Dictionary<string, object>);
                }
                else if (pair.Value.GetType() == typeof(List<object>))
                {
                    if (!(pair.Value as List<object>).SequenceEqual(dict2Value as List<object>))
                        throw new Exception($"values not equal. Value1: {pair.Value} - Value2: {dict2Value}");
                }
                else
                {
                    if (!pair.Value.Equals(dict2Value))
                        throw new Exception($"values not equal. Value1: {pair.Value} - Value2: {dict2Value}");
                }
            }
            return true;
        }

        public struct NastyStruct
        {
            public byte R, G, B;
            public NastyStruct(byte r, byte g, byte b)
            {
                R = r; G = g; B = b;
            }
            public static NastyStruct Nasty = new NastyStruct(0, 0, 0);
        }

        [TestMethod]
        public void TestNastyStruct()
        {
            NastyStruct s = "{\"R\":234,\"G\":123,\"B\":11}".FromJson<NastyStruct>();
            Assert.AreEqual(234, s.R);
            Assert.AreEqual(123, s.G);
            Assert.AreEqual(11, s.B);
        }

        [TestMethod]
        public void TestEscaping()
        {
            var orig = new Dictionary<string, string> { { "hello", "world\n \" \\ \b \r \\0\u263A" } };
            var parsed = "{\"hello\":\"world\\n \\\" \\\\ \\b \\r \\0\\u263a\"}".FromJson<Dictionary<string, string>>();
            Assert.AreEqual(orig["hello"], parsed["hello"]);
        }

        [TestMethod]
        public void TestMultithread()
        {
            // Lots of threads
            for (int i = 0; i < 100; i++)
            {
                new Thread(() =>
                {
                    // Each threads has enough work to potentially hit a race condition
                    for (int j = 0; j < 10000; j++)
                    {
                        TestValues();
                        TestArrayOfValues();
                        TestListOfValues();
                        TestRecursiveLists();
                        TestRecursiveArrays();
                        TestDictionary();
                        TestRecursiveDictionary();
                        TestSimpleObject();
                        TestSimpleStruct();
                        TestListOfStructs();
                        TestDeepObject();
                        CorruptionTest();
                        DynamicParserTest();
                        TestNastyStruct();
                        TestEscaping();
                    }
                }).Start();
            }
        }

        class IgnoreDataMemberObject
        {
            public int A;
            [IgnoreDataMember]
            public int B;

            public int C { get; set; }
            [IgnoreDataMember]
            public int D { get; set; }
        }

        [TestMethod]
        public void TestIgnoreDataMember()
        {
            IgnoreDataMemberObject value = "{\"A\":123,\"B\":456,\"Ignored\":10,\"C\":789,\"D\":14}".FromJson<IgnoreDataMemberObject>();
            Assert.IsNotNull(value);
            Assert.AreEqual(123, value.A);
            Assert.AreEqual(0, value.B);
            Assert.AreEqual(789, value.C);
            Assert.AreEqual(0, value.D);
        }

        class DataMemberObject
        {
            [DataMember(Name = "a")]
            public int A;
            [DataMember()]
            public int B;

            [DataMember(Name = "c")]
            public int C { get; set; }
            public int D { get; set; }
        }

        [TestMethod]
        public void TestDataMemberObject()
        {
            DataMemberObject value = "{\"a\":123,\"B\":456,\"c\":789,\"D\":14}".FromJson<DataMemberObject>();
            Assert.IsNotNull(value);
            Assert.AreEqual(123, value.A);
            Assert.AreEqual(456, value.B);
            Assert.AreEqual(789, value.C);
            Assert.AreEqual(14, value.D);
        }

        public class EnumClass
        {
            public Color Colors;
            public Style Style;
        }

        [TestMethod]
        public void TestEnumMember()
        {
            EnumClass value = "{\"Colors\":\"Green\",\"Style\":\"Bold, Underline\"}".FromJson<EnumClass>();
            Assert.IsNotNull(value);
            Assert.AreEqual(Color.Green, value.Colors);
            Assert.AreEqual(Style.Bold | Style.Underline, value.Style);

            value = "{\"Colors\":3,\"Style\":10}".FromJson<EnumClass>();
            Assert.IsNotNull(value);
            Assert.AreEqual(Color.Yellow, value.Colors);
            Assert.AreEqual(Style.Italic | Style.Strikethrough, value.Style);

            value = "{\"Colors\":\"3\",\"Style\":\"10\"}".FromJson<EnumClass>();
            Assert.IsNotNull(value);
            Assert.AreEqual(Color.Yellow, value.Colors);
            Assert.AreEqual(Style.Italic | Style.Strikethrough, value.Style);

            value = "{\"Colors\":\"sfdoijsdfoij\",\"Style\":\"sfdoijsdfoij\"}".FromJson<EnumClass>();
            Assert.IsNotNull(value);
            Assert.AreEqual(Color.Red, value.Colors);
            Assert.AreEqual(Style.None, value.Style);
        }

        [TestMethod]
        public void TestDuplicateKeys()
        {
            var parsed = @"{""hello"": ""world"", ""goodbye"": ""heaven"", ""hello"": ""hell""}".FromJson<Dictionary<string, object>>();
            /*
             * We expect the parser to process the (valid) JSON above containing a duplicated key. The dictionary ensures that there is
             * only one entry with the duplicate key.
             */
            Assert.IsTrue(parsed.ContainsKey("hello"), "The dictionary is missing the duplicated key");
            /*
             * We also expect the other keys in the JSON to be processed as normal
             */
            Assert.IsTrue(parsed.ContainsKey("goodbye"), "The dictionary is missing the non-duplicated key");
            /*
             * The parser should store the last occurring value for the given key
             */
            Assert.AreEqual(parsed["hello"], "hell", "The parser stored an incorrect value for the duplicated key");
        }

        [TestMethod]
        public void TestDuplicateKeysInAnonymousObject()
        {
            var parsed = @"{""hello"": ""world"", ""goodbye"": ""heaven"", ""hello"": ""hell""}".FromJson<object>();
            var dictionary = (Dictionary<string, object>)parsed;
            /*
             * We expect the parser to process the (valid) JSON above containing a duplicated key. The dictionary ensures that there is
             * only one entry with the duplicate key.
             */
            Assert.IsTrue(dictionary.ContainsKey("hello"), "The dictionary is missing the duplicated key");
            /*
             * We also expect the other keys in the JSON to be processed as normal
             */
            Assert.IsTrue(dictionary.ContainsKey("goodbye"), "The dictionary is missing the non-duplicated key");
            /*
             * The parser should store the last occurring value for the given key
             */
            Assert.AreEqual(dictionary["hello"], "hell", "The parser stored an incorrect value for the duplicated key");
        }

        public class UnderlyingEnumClass
        {
            public Color Colors;
        }

        [TestMethod]
        [DataRow(Color.Red)]
        [DataRow(Color.Green)]
        [DataRow(Color.Blue)]
        [DataRow(Color.Yellow)]
        public void TestEnumUnderlyingValues(Color color)
        {
            var expected = $"{{\"Colors\":{(int)color}}}";
            var underlyingEnumClass = expected.FromJson<UnderlyingEnumClass>();
            Assert.AreEqual<Color>(underlyingEnumClass.Colors, color);
        }

        [TestMethod]
        public void TestNull()
        {
            var expected = "{\"C\":null}";
            var simpleObject = expected.FromJson<SimpleObject>();
            Assert.AreEqual(null, simpleObject.C);
        }

        public class TestClassWithCtor : IEquatable<TestClassWithCtor>
        {
            public List<int> Numbers { get; set; }
            public string Text { get; set; } = "Add two numbers";
            public int TargetYear { get; set; }
            public List<string> Errors { get; set; }

            public string TextTwo = "AddTwoMembers";

            public TestClassWithCtor()
            {
                Numbers = new List<int>();
                TargetYear = DateTime.Now.Year;
            }

            public bool Equals(TestClassWithCtor other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return TextTwo == other.TextTwo &&
                       Numbers.SequenceEqual(other.Numbers) && 
                       Text == other.Text && 
                       TargetYear == other.TargetYear &&
                       ((Errors == null && other.Errors == null) ||
                        (Errors.SequenceEqual(other.Errors)));
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((TestClassWithCtor) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (TextTwo != null ? TextTwo.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Numbers != null ? Numbers.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Text != null ? Text.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ TargetYear;
                    hashCode = (hashCode * 397) ^ (Errors != null ? Errors.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
        
        [TestMethod]
        public void TestObjectWithCtor()
        {
            var json = "{}";

            var expected = new TestClassWithCtor();
            var actual = json.FromJson<TestClassWithCtor>();

            Assert.AreEqual(expected, actual);
        }


        public class TestClassWithDefaultValues : IEquatable<TestClassWithDefaultValues>
        {
            [DefaultValue(12)]
            public int TestValue { get; set; }

            [DefaultValue(null)]
            public string NullValue { get; set; }

            [DefaultValue(Color.Blue)]
            public Color MyColor { get; set; }

            [DefaultValue("0:0:0.1")]
            public TimeSpan Start { get; set; }

            [DefaultValue(3)]
            public Color YourColor { get; set; }

            [DefaultValue("2")]
            public Color OurColor { get; set; }


            public bool Equals(TestClassWithDefaultValues other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return TestValue == other.TestValue && 
                       NullValue == other.NullValue && 
                       MyColor == other.MyColor && 
                       Start.Equals(other.Start) && 
                       YourColor == other.YourColor && 
                       OurColor == other.OurColor;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((TestClassWithDefaultValues) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = TestValue;
                    hashCode = (hashCode * 397) ^ (NullValue != null ? NullValue.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (int) MyColor;
                    hashCode = (hashCode * 397) ^ Start.GetHashCode();
                    hashCode = (hashCode * 397) ^ (int) YourColor;
                    hashCode = (hashCode * 397) ^ (int) OurColor;
                    return hashCode;
                }
            }
        }
        
        [TestMethod]
        public void TestObjectWithDefaultValueAttributes()
        {
            var json = "{}";
            
            var actual = json.FromJson<TestClassWithDefaultValues>();

            Assert.AreEqual(12, actual.TestValue);
            Assert.AreEqual(null, actual.NullValue);
            Assert.AreEqual(Color.Blue, actual.MyColor);
            Assert.AreEqual(Color.Yellow, actual.YourColor);
            Assert.AreEqual(Color.Blue, actual.OurColor);
            Assert.AreEqual(TimeSpan.Parse("0:0:0.1"), actual.Start);
        }

        [TestMethod]
        public void TestObjectWithDefaultValueAttributeButValueFromJson()
        {
            var valueInJson = 107;
            var json = $"{{\"TestValue\":{valueInJson}}}";
            
            var actual = json.FromJson<TestClassWithDefaultValues>();

            Assert.AreEqual(valueInJson, actual.TestValue);
        }

        public class GuidTest : IEquatable<GuidTest>
        {
            public Guid Id { get; set; }

            public bool Equals(GuidTest other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id.Equals(other.Id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((GuidTest) obj);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        [TestMethod]
        public void TestObjectWithGuid()
        {
            Assert.AreEqual(new GuidTest(), "{\"Id\":\"00000000-0000-0000-0000-000000000000\"}".FromJson<GuidTest>());
            
            var valueInJson = Guid.NewGuid();
            var json = $"{{\"Id\":\"{valueInJson}\"}}";
            var actual = json.FromJson<GuidTest>();
            Assert.AreEqual(valueInJson, actual.Id);
        }

        public class NullableGuidTest : IEquatable<NullableGuidTest>
        {
            public Guid? Id { get; set; }

            public bool Equals(NullableGuidTest other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Nullable.Equals(Id, other.Id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((NullableGuidTest) obj);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        [TestMethod]
        public void TestObjectWithNullableGuid()
        {
            Assert.AreEqual(new NullableGuidTest(), "{}".FromJson<NullableGuidTest>());
            Assert.AreEqual(new NullableGuidTest(), "{\"Id\":null}".FromJson<NullableGuidTest>());
            
            var valueInJson = Guid.NewGuid();
            var json = $"{{\"Id\":\"{valueInJson}\"}}";
            var actual = json.FromJson<NullableGuidTest>();
            Assert.AreEqual(valueInJson, actual.Id);
        }

        public class NoSetterClass
        {
            public string Text { get; } = "No touchy!";
        }

        [TestMethod]
        public void TestObjectWithoutSetMethod()
        {
            var newText = "Touch!";
            var actual = $"{{\"Text\":\"{newText}\"}}".FromJson<NoSetterClass>();

            Assert.AreNotEqual(actual.Text, newText);
            Assert.AreEqual(new NoSetterClass().Text, actual.Text);
        }
    }
}

#pragma warning restore CS0414
#pragma warning restore CS0649