using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Venti
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Size
    {
        Small,
        Medium,
        Large
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Alignment
    {
        Left,
        Center,
        Right
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Rounding
    {
        None,
        Small,
        Medium,
        Large,
        Full
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RoundingFull
    {
        None,
        Small,
        Medium,
        Large,
        Full
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BorderThickness
    {
        None,
        Small,
        Medium,
        Large
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum FontType
    {
        Google,
        Custom
    }

    //[JsonConverter(typeof(StringEnumConverter))]
    //public enum TypographyType
    //{
    //    Heading,
    //    Body
    //}
}
