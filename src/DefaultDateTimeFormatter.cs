using System;
using System.Text;

namespace SpecFlow.Internal.Json
{
    // https://github.com/neuecc/Utf8Json/blob/master/src/Utf8Json/Formatters/DateTimeFormatter.cs
    // ISO8601DateTimeFormatter
    public class DefaultDateTimeFormatter
    {
        public static void SerializeDateTime(StringBuilder stringBuilder, DateTime value)
        {
            var year = value.Year;
            var month = value.Month;
            var day = value.Day;
            var hour = value.Hour;
            var minute = value.Minute;
            var second = value.Second;
            var nanosec = value.Ticks % TimeSpan.TicksPerSecond;

            stringBuilder.Append('\"');

            if (year < 10)
            {
                stringBuilder.Append("000");
            }
            else if (year < 100)
            {
                stringBuilder.Append("00");
            }
            else if (year < 1000)
            {
                stringBuilder.Append('0');
            }
            stringBuilder.Append(year);
            stringBuilder.Append('-');

            if (month < 10)
            {
                stringBuilder.Append('0');
            }
            stringBuilder.Append(month);
            stringBuilder.Append('-');

            if (day < 10)
            {
                stringBuilder.Append('0');
            }
            stringBuilder.Append(day);

            stringBuilder.Append('T');

            if (hour < 10)
            {
                stringBuilder.Append('0');
            }
            stringBuilder.Append(hour);
            stringBuilder.Append(':');

            if (minute < 10)
            {
                stringBuilder.Append('0');
            }
            stringBuilder.Append(minute);
            stringBuilder.Append(':');

            if (second < 10)
            {
                stringBuilder.Append('0');
            }
            stringBuilder.Append(second);

            if (nanosec != 0)
            {
                stringBuilder.Append('.');

                if (nanosec < 10)
                {
                    stringBuilder.Append("000000");
                }
                else if (nanosec < 100)
                {
                    stringBuilder.Append("00000");
                }
                else if (nanosec < 1000)
                {
                    stringBuilder.Append("0000");
                }
                else if (nanosec < 10000)
                {
                    stringBuilder.Append("000");
                }
                else if (nanosec < 100000)
                {
                    stringBuilder.Append("00");
                }
                else if (nanosec < 1000000)
                {
                    stringBuilder.Append('0');
                }

                stringBuilder.Append(nanosec);
            }

            switch (value.Kind)
            {
                case DateTimeKind.Local:
                    // should not use `BaseUtcOffset` - https://stackoverflow.com/questions/10019267/is-there-a-generic-timezoneinfo-for-central-europe
                    var localOffset = TimeZoneInfo.Local.GetUtcOffset(value);
                    var minus = (localOffset < TimeSpan.Zero);
                    if (minus) localOffset = localOffset.Negate();
                    var h = localOffset.Hours;
                    var m = localOffset.Minutes;
                    stringBuilder.Append(minus ? '-' : '+');
                    if (h < 10)
                    {
                        stringBuilder.Append('0');
                    }
                    stringBuilder.Append(h);
                    stringBuilder.Append(':');
                    if (m < 10)
                    {
                        stringBuilder.Append('0');
                    }
                    stringBuilder.Append(m);
                    break;
                case DateTimeKind.Utc:
                    stringBuilder.Append('Z');
                    break;
                case DateTimeKind.Unspecified:
                default:
                    break;
            }

            stringBuilder.Append('\"');
        }

        public static DateTime DeserializeDateTime(string dateTimeString)
        {
            dateTimeString = dateTimeString.Replace("\"", "");
            var array = Encoding.UTF8.GetBytes(dateTimeString);
            var i = 0;
            var len = array.Length;
            var to = array.Length;

            // YYYY
            if (len == 4)
            {
                var y = (array[i++] - (byte)'0') * 1000 + (array[i++] - (byte)'0') * 100 + (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
                return new DateTime(y, 1, 1);
            }

            // YYYY-MM
            if (len == 7)
            {
                var y = (array[i++] - (byte)'0') * 1000 + (array[i++] - (byte)'0') * 100 + (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
                if (array[i++] != (byte)'-') goto ERROR;
                var m = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
                return new DateTime(y, m, 1);
            }

            // YYYY-MM-DD
            if (len == 10)
            {
                var y = (array[i++] - (byte)'0') * 1000 + (array[i++] - (byte)'0') * 100 + (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
                if (array[i++] != (byte)'-') goto ERROR;
                var m = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
                if (array[i++] != (byte)'-') goto ERROR;
                var d = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
                return new DateTime(y, m, d);
            }

            // range-first section requires 19
            if (len < 19) goto ERROR;

            var year = (array[i++] - (byte)'0') * 1000 + (array[i++] - (byte)'0') * 100 + (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
            if (array[i++] != (byte)'-') goto ERROR;
            var month = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
            if (array[i++] != (byte)'-') goto ERROR;
            var day = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');

            if (array[i++] != (byte)'T') goto ERROR;

            var hour = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
            if (array[i++] != (byte)':') goto ERROR;
            var minute = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
            if (array[i++] != (byte)':') goto ERROR;
            var second = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');

            int ticks = 0;
            if (i < to && array[i] == '.')
            {
                i++;

                // *7.
                if (!(i < to) || !IsNumber(array[i])) goto END_TICKS;
                ticks += (array[i] - (byte)'0') * 1000000;
                i++;

                if (!(i < to) || !IsNumber(array[i])) goto END_TICKS;
                ticks += (array[i] - (byte)'0') * 100000;
                i++;

                if (!(i < to) || !IsNumber(array[i])) goto END_TICKS;
                ticks += (array[i] - (byte)'0') * 10000;
                i++;

                if (!(i < to) || !IsNumber(array[i])) goto END_TICKS;
                ticks += (array[i] - (byte)'0') * 1000;
                i++;

                if (!(i < to) || !IsNumber(array[i])) goto END_TICKS;
                ticks += (array[i] - (byte)'0') * 100;
                i++;

                if (!(i < to) || !IsNumber(array[i])) goto END_TICKS;
                ticks += (array[i] - (byte)'0') * 10;
                i++;

                if (!(i < to) || !IsNumber(array[i])) goto END_TICKS;
                ticks += (array[i] - (byte)'0') * 1;
                i++;

                // others, lack of precision
                while (i < to && IsNumber(array[i]))
                {
                    i++;
                }
            }

            END_TICKS:
            var kind = DateTimeKind.Unspecified;
            if (i < to && array[i] == 'Z')
            {
                kind = DateTimeKind.Utc;
            }
            else if (i < to && (array[i] == '-' || array[i] == '+'))
            {
                if (!(i + 5 < to)) goto ERROR;

                kind = DateTimeKind.Local;
                var minus = array[i++] == '-';

                var h = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');
                i++;
                var m = (array[i++] - (byte)'0') * 10 + (array[i++] - (byte)'0');

                var offset = new TimeSpan(h, m, 0);
                if (minus) offset = offset.Negate();

                return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).AddTicks(ticks).Subtract(offset).ToLocalTime();
            }

            return new DateTime(year, month, day, hour, minute, second, kind).AddTicks(ticks);

            ERROR:
            throw new InvalidOperationException($"invalid datetime format. value: {dateTimeString}");
        }

        public static bool IsNumber(byte c)
        {
            return (byte)'0' <= c && c <= (byte)'9';
        }
    }
}
