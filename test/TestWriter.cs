﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CS0649 // not production code
#pragma warning disable CS0414 // not production code

namespace SpecFlow.Internal.Json.Tests
{
    [TestClass]
    public class TestWriter
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

        [TestMethod]
        public void TestValues()
        {
            Assert.AreEqual("\"\u94b1\u4e0d\u591f!\"", "\u94b1\u4e0d\u591f!".ToJson());
            Assert.AreEqual("123", 123.ToJson());
            Assert.AreEqual("true", true.ToJson());
            Assert.AreEqual("false", false.ToJson());
            Assert.AreEqual("[1,2,3]", new int[] { 1, 2, 3 }.ToJson());
            Assert.AreEqual("[1,2,3]", new List<int> { 1, 2, 3 }.ToJson());
            Assert.AreEqual("\"Green\"", Color.Green.ToJson());
            Assert.AreEqual("\"Green\"", ((Color)1).ToJson());
            Assert.AreEqual("\"10\"", ((Color)10).ToJson());
            Assert.AreEqual("\"Bold\"", Style.Bold.ToJson());
            Assert.AreEqual("\"Bold, Italic\"", (Style.Bold | Style.Italic).ToJson());
            Assert.AreEqual("\"19\"", (Style.Bold | Style.Italic | (Style)16).ToJson());
        }

        [TestMethod]
        public void TestDicts()
        {
            Assert.AreEqual("{\"foo\":\"bar\"}", new Dictionary<string, string> { { "foo", "bar" } }.ToJson());
            Assert.AreEqual("{\"foo\":123}", new Dictionary<string, int> { { "foo", 123 } }.ToJson());
        }

        class SimpleObject
        {
            public SimpleObject A;
            public List<int> B;
            public string C { get; set; }

            // Should not serialize
            private int D = 333;
            public static int E = 555;
            internal int F = 777;
            protected int G = 999;
            public const int H = 111;
        }

        struct SimpleStruct
        {
            public SimpleObject A;
        }

        class InheritedObject : SimpleObject
        {
            public int X;
        }

        [TestMethod]
        public void TestObjects()
        {
            Assert.AreEqual("{\"A\":{},\"B\":[1,2,3],\"C\":\"Test\"}", new SimpleObject { A = new SimpleObject(), B = new List<int> { 1, 2, 3 }, C = "Test" }.ToJson());
            Assert.AreEqual("{\"A\":{\"A\":{},\"B\":[1,2,3],\"C\":\"Test\"}}", new SimpleStruct { A = new SimpleObject { A = new SimpleObject(), B = new List<int> { 1, 2, 3 }, C = "Test" } }.ToJson());
            Assert.AreEqual("{\"X\":9,\"A\":{},\"B\":[1,2,3],\"C\":\"Test\"}", new InheritedObject { A = new SimpleObject(), B = new List<int> { 1, 2, 3 }, C = "Test", X = 9 }.ToJson());
        }

        public struct NastyStruct
        {
            public int R, G, B;
            public NastyStruct(byte r, byte g, byte b)
            {
                R = r; G = g; B = b;
            }
            public static NastyStruct Nasty = new NastyStruct(0, 0, 0);
        }

        [TestMethod]
        public void TestNastyStruct()
        {
            Assert.AreEqual("{\"R\":1,\"G\":2,\"B\":3}", new NastyStruct(1,2,3).ToJson());
        }

        [TestMethod]
        public void TestEscaping()
        {
            Assert.AreEqual("{\"hello\":\"world\\n \\\\ \\\" \\b \\r \\u0000\u263A\"}", new Dictionary<string,string>{
                {"hello", "world\n \\ \" \b \r \0\u263A"}
            }.ToJson());
        }

        class IgnoreDataMemberObject
        {
            [IgnoreDataMember]
            public int A;
            public int B;
            [IgnoreDataMember]
            public int C { get; set; }
            public int D { get; set; }
        }

        [TestMethod]
        public void TestIgnoreDataMemberObject()
        {
            Assert.AreEqual("{\"B\":20,\"D\":40}", new IgnoreDataMemberObject { A = 10, B = 20, C = 30, D = 40 }.ToJson());
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
            Assert.AreEqual("{\"a\":10,\"B\":20,\"c\":30,\"D\":40}", new DataMemberObject { A = 10, B = 20, C = 30, D = 40 }.ToJson());
        }

        public class EnumClass
        {
            public Color Colors;
            public Style Style;
        }

        [TestMethod]
        public void TestEnumMember()
        {
            Assert.AreEqual("{\"Colors\":\"Green\",\"Style\":\"Bold\"}", new EnumClass { Colors = Color.Green, Style = Style.Bold }.ToJson());
            Assert.AreEqual("{\"Colors\":\"Green\",\"Style\":\"Bold, Underline\"}", new EnumClass { Colors = Color.Green, Style = Style.Bold | Style.Underline }.ToJson());
            Assert.AreEqual("{\"Colors\":\"Blue\",\"Style\":\"Italic, Underline\"}", new EnumClass { Colors = (Color)2, Style = (Style)6 }.ToJson());
            Assert.AreEqual("{\"Colors\":\"Blue\",\"Style\":\"Underline\"}", new EnumClass { Colors = (Color)2, Style = (Style)4 }.ToJson());
            Assert.AreEqual("{\"Colors\":\"10\",\"Style\":\"17\"}", new EnumClass { Colors = (Color)10, Style = (Style)17 }.ToJson());
        }

        public class PrimitiveObject
        {
            public bool Bool;
            public byte Byte;
            public sbyte SByte;
            public short Short;
            public ushort UShort;
            public int Int;
            public uint UInt;
            public long Long;
            public ulong ULong;
            public char Char;
            public float Single;
            public double Double;
            public decimal Decimal;
        }

        [TestMethod]
        public void TestPrimitives()
        {
            Assert.AreEqual(
                "{\"Bool\":true,\"Byte\":17,\"SByte\":-17,\"Short\":-123,\"UShort\":123,\"Int\":-56,\"UInt\":56,\"Long\":-34,\"ULong\":34,\"Char\":\"C\",\"Single\":4.3,\"Double\":5.6,\"Decimal\":10.1}",
                new PrimitiveObject
                {
                    Bool = true,
                    Byte = 17,
                    SByte = -17,
                    Short = -123,
                    UShort = 123,
                    Int = -56,
                    UInt = 56,
                    Long = -34,
                    ULong = 34,
                    Char = 'C',
                    Single = 4.3f,
                    Double = 5.6,
                    Decimal = 10.1M
                }.ToJson());
        }
        
        public enum ByteEnum : byte
        {
            Cyan = 170,
            Gold = 255
        }

        public enum LongEnum : long
        {
            White = 9223372036854775800,
            Black = 9223372036854775801
        }

        public enum ShortEnum : short
        {
            Orange = -10,
            Pink = -32768,
            Indigo = 32767
        }

        [TestMethod]
        public void TestEnumUnderlyingValues()
        {
            var settings = new JsonSerializerSettings(useEnumUnderlyingValues: true);
            
            Assert.AreEqual("{\"Colors\":0}", new { Colors = Color.Red }.ToJson(settings));
            Assert.AreEqual("{\"Colors\":1}", new { Colors = Color.Green }.ToJson(settings));
            Assert.AreEqual("{\"Colors\":2}", new { Colors = Color.Blue }.ToJson(settings));
            Assert.AreEqual("{\"Colors\":3}", new { Colors = Color.Yellow }.ToJson(settings));

            Assert.AreEqual("{\"Colors\":170}", new { Colors = ByteEnum.Cyan }.ToJson(settings));
            Assert.AreEqual("{\"Colors\":255}", new { Colors = ByteEnum.Gold }.ToJson(settings));

            Assert.AreEqual("{\"Colors\":9223372036854775800}", new { Colors = LongEnum.White }.ToJson(settings));
            Assert.AreEqual("{\"Colors\":9223372036854775801}", new { Colors = LongEnum.Black }.ToJson(settings));

            Assert.AreEqual("{\"Colors\":-10}", new { Colors = ShortEnum.Orange }.ToJson(settings));
            Assert.AreEqual("{\"Colors\":-32768}", new { Colors = ShortEnum.Pink }.ToJson(settings));
            Assert.AreEqual("{\"Colors\":32767}", new { Colors = ShortEnum.Indigo }.ToJson(settings));
        }

        [TestMethod]
        public void TestIgnoreNullValues()
        {
            Assert.AreEqual("{}", new { Address = (string)null }.ToJson());
            Assert.AreEqual("{\"FirstName\":\"Bob\"}", new { FirstName = "Bob", Address = (string)null }.ToJson());

            var settings = new JsonSerializerSettings(ignoreNullValues: false);
            Assert.AreEqual("{\"Address\":null}", new { Address = (string)null }.ToJson(settings));
            Assert.AreEqual("{\"FirstName\":\"Bob\",\"Address\":null}", new { FirstName = "Bob", Address = (string)null }.ToJson(settings));
        }

        public class TestGuidClass
        {
            public Guid Id { get; set; }
            public Guid? PersonId { get; set; }
        }

        [TestMethod]
        public void TestGuid()
        {
            Assert.AreEqual("{\"Id\":\"00000000-0000-0000-0000-000000000000\"}", new TestGuidClass().ToJson());
            Assert.AreEqual("{\"Id\":\"00000000-0000-0000-0000-000000000000\",\"PersonId\":null}", new TestGuidClass().ToJson(new JsonSerializerSettings(false)));

            var id = Guid.NewGuid();
            Assert.AreEqual($"{{\"Id\":\"{id}\"}}", new TestGuidClass() { Id = id }.ToJson());
            Assert.AreEqual($"{{\"Id\":\"{id}\",\"PersonId\":null}}", new TestGuidClass() { Id = id }.ToJson(new JsonSerializerSettings(false)));

            var personId = Guid.NewGuid();
            Assert.AreEqual($"{{\"Id\":\"{id}\",\"PersonId\":\"{personId}\"}}", new TestGuidClass() { Id = id, PersonId = personId}.ToJson());
        }

    }
}

#pragma warning restore CS0414
#pragma warning restore CS0649