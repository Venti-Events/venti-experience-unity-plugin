using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Venti
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Size
    {
        sm, //Small,
        md, //Medium,
        lg  //Large
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Alignment
    {
        left,   //Left,
        center, //Center,
        right   //Right
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Rounding
    {
        none,   //None,
        sm,     //Small,
        md,     //Medium,
        lg,     //Large,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RoundingFull
    {
        none,   //None,
        sm,     //Small,
        md,     //Medium,
        lg,     //Large,
        full    //Full
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BorderThickness
    {
        none,   //None,
        sm,     //Small,
        md,     //Medium,
        lg,     //Large
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum FontType
    {
        google, //Google,
        custom  //Custom
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TypographyType
    {
        heading,    //Heading,
        body        //Body
    }
}
