﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SpecFlow.Internal.Json.Tests
{
    [TestClass]
    public class DateTimeTests
    {
        [TestMethod]
        public void SerializeDateTimeNow()
        {
            var now = DateTime.Now;
            var expected = $"\"{now:O}\"";
            var serialized = now.ToJson();
            
            Assert.AreEqual(expected, serialized);
        }

        [TestMethod]
        public void SerializeDateTimeUtcNow()
        {
            var now = DateTime.UtcNow;
            var expected = $"\"{now:O}\"";
            var serialized = now.ToJson();
            
            Assert.AreEqual(expected, serialized);
        }

        [TestMethod]
        public void DeserializeDateTimeNow()
        {
            var dto = DateTime.Now;
            var serialized = dto.ToJson();
            var deSerialized = serialized.FromJson<DateTime>();

            Assert.AreEqual(dto, deSerialized);
        }

        [TestMethod]
        public void DeserializeDateTimeUtcNow()
        {
            var dto = DateTime.UtcNow;
            var serialized = dto.ToJson();
            var deSerialized = serialized.FromJson<DateTime>();

            Assert.AreEqual(dto, deSerialized);
        }

        [TestMethod]
        public void TestDateTimeFormats()
        {
            Assert.AreEqual("2021".ToJson().FromJson<DateTime>(), new DateTime(2021,1,1));
            Assert.AreEqual("2021-06".ToJson().FromJson<DateTime>(), new DateTime(2021,6,1));
            Assert.AreEqual("2021-06-19".ToJson().FromJson<DateTime>(), new DateTime(2021,6,19));
            Assert.AreEqual("2021-02-16T14:07:24.3912313Z".ToJson().FromJson<DateTime>(), new DateTime(2021, 2, 16, 14, 7, 24).AddTicks(3912313));

            Assert.AreEqual(new DateTime(2021, 6, 1).ToJson().FromJson<DateTime>(), new DateTime(2021, 6, 1));
            Assert.AreEqual(new DateTime(2021, 6, 1, 8, 10, 30).ToJson().FromJson<DateTime>(), new DateTime(2021, 6, 1, 8, 10, 30));
        }

        public class DateTimeTest : IEquatable<DateTimeTest>
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }

            public bool Equals(DateTimeTest other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Start.Equals(other.Start) && End.Equals(other.End);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((DateTimeTest) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Start.GetHashCode() * 397) ^ End.GetHashCode();
                }
            }
        }

        [TestMethod]
        public void SerializeObjectWithDateTimes()
        {
            var defaultValues = "{\"Start\":\"0001-01-01T00:00:00\",\"End\":\"0001-01-01T00:00:00\"}";
            Assert.AreEqual(defaultValues, new DateTimeTest().ToJson());

            var start = DateTime.Now;
            var onlyStart = $"{{\"Start\":\"{start:O}\",\"End\":\"0001-01-01T00:00:00\"}}";
            Assert.AreEqual(onlyStart, new DateTimeTest() { Start = start }.ToJson());

            var end = DateTime.Now.AddHours(-1);
            var onlyEnd = $"{{\"Start\":\"0001-01-01T00:00:00\",\"End\":\"{end:O}\"}}";
            Assert.AreEqual(onlyEnd, new DateTimeTest() { End = end }.ToJson());

            var bothValues = $"{{\"Start\":\"{start:O}\",\"End\":\"{end:O}\"}}";
            Assert.AreEqual(bothValues, new DateTimeTest() { Start = start, End = end }.ToJson());
        }

        [TestMethod]
        public void DeserializeObjectWithDateTimes()
        {
            Assert.AreEqual(new DateTimeTest(), new DateTimeTest().ToJson().FromJson<DateTimeTest>());

            var oneProp = new DateTimeTest() { Start = DateTime.Now };
            Assert.AreEqual(oneProp, oneProp.ToJson().FromJson<DateTimeTest>());

            var twoProps = new DateTimeTest() { Start = DateTime.Now, End = DateTime.Now.AddHours(-1) };
            Assert.AreEqual(twoProps, twoProps.ToJson().FromJson<DateTimeTest>());
        }

        public class NullableDateTimeTest : IEquatable<NullableDateTimeTest>
        {
            public DateTime? Start { get; set; }
            public DateTime? End { get; set; }

            public bool Equals(NullableDateTimeTest other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Nullable.Equals(Start, other.Start) && Nullable.Equals(End, other.End);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((NullableDateTimeTest) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Start.GetHashCode() * 397) ^ End.GetHashCode();
                }
            }
        }

        [TestMethod]
        public void SerializeObjectWithNullableDateTimes()
        {
            var defaultValues = "{}";
            Assert.AreEqual(defaultValues, new NullableDateTimeTest().ToJson());

            var bothNulls = "{\"Start\":null,\"End\":null}";
            Assert.AreEqual(bothNulls, new NullableDateTimeTest().ToJson(new JsonSerializerSettings(false)));

            var start = DateTime.Now;
            var endIsNull = $"{{\"Start\":\"{start:O}\",\"End\":null}}";
            Assert.AreEqual(endIsNull, new NullableDateTimeTest() { Start = start }.ToJson(new JsonSerializerSettings(false)));

            var end = DateTime.Now.AddHours(-1);
            var startIsNull = $"{{\"Start\":null,\"End\":\"{end:O}\"}}";
            Assert.AreEqual(startIsNull, new NullableDateTimeTest() { End = end }.ToJson(new JsonSerializerSettings(false)));

            var noNulls = $"{{\"Start\":\"{start:O}\",\"End\":\"{end:O}\"}}";
            Assert.AreEqual(noNulls, new NullableDateTimeTest() { Start = start, End = end }.ToJson());
        }

        [TestMethod]
        public void DeserializeObjectWithNullableDateTimes()
        {
            Assert.AreEqual(new NullableDateTimeTest(), new NullableDateTimeTest().ToJson().FromJson<NullableDateTimeTest>());
            Assert.AreEqual(new NullableDateTimeTest(), new NullableDateTimeTest().ToJson(new JsonSerializerSettings(false)).FromJson<NullableDateTimeTest>());

            var oneNull = new NullableDateTimeTest() { Start = DateTime.Now };
            Assert.AreEqual(oneNull, oneNull.ToJson(new JsonSerializerSettings(false)).FromJson<NullableDateTimeTest>());

            var twoNulls = new NullableDateTimeTest() { Start = DateTime.Now, End = DateTime.Now.AddHours(-1) };
            Assert.AreEqual(twoNulls, twoNulls.ToJson().FromJson<NullableDateTimeTest>());
        }

        [TestMethod]
        public void SerializeTimeSpan()
        {
            var timeSpan = TimeSpan.FromSeconds(10);
            var expected = $"\"{timeSpan}\"";
            var serialized = timeSpan.ToJson();

            Assert.AreEqual(expected, serialized);
        }

        [TestMethod]
        public void DeserializeTimeSpan()
        {
            var dto = TimeSpan.FromSeconds(10);
            var serialized = dto.ToJson();
            var deSerialized = serialized.FromJson<TimeSpan>();

            Assert.AreEqual(dto, deSerialized);
        }

        public class TimeSpanTest : IEquatable<TimeSpanTest>
        {
            public TimeSpan? Duration { get; set; }

            public bool Equals(TimeSpanTest other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Nullable.Equals(Duration, other.Duration);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((TimeSpanTest) obj);
            }

            public override int GetHashCode()
            {
                return Duration.GetHashCode();
            }
        }

        [TestMethod]
        public void SerializeNullableTimeSpan()
        {
            var nullValue = $"{{\"Duration\":null}}";
            Assert.AreEqual(nullValue, new TimeSpanTest().ToJson(new JsonSerializerSettings(ignoreNullValues: false)));

            var witValue = $"{{\"Duration\":\"00:00:00.0780782\"}}";
            Assert.AreEqual(witValue, new TimeSpanTest { Duration = TimeSpan.FromTicks(780782) }.ToJson());
        }

        [TestMethod]
        public void DeserializeNullableTimeSpan()
        {
            Assert.AreEqual(new TimeSpanTest(), new TimeSpanTest().ToJson(new JsonSerializerSettings(ignoreNullValues: false)).FromJson<TimeSpanTest>());

            var timeSpan = TimeSpan.FromSeconds(1);
            Assert.AreEqual(new TimeSpanTest() { Duration = timeSpan }, new TimeSpanTest { Duration = timeSpan }.ToJson().FromJson<TimeSpanTest>());
        }
    }
}
