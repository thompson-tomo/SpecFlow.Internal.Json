namespace SpecFlow.Internal.Json
{
    public class JsonSerializerSettings
    {
        public bool IgnoreNullValues { get; set; }

        public bool UseEnumUnderlyingValues { get; set; }

        public JsonSerializerSettings(bool ignoreNullValues = true, bool useEnumUnderlyingValues = false)
        {
            IgnoreNullValues = ignoreNullValues;
            UseEnumUnderlyingValues = useEnumUnderlyingValues;
        }
    }
}
