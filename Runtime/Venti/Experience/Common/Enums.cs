using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Venti.Experience
{
    // Experience Enums
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Resolution
    {
        Mobile,
        Tablet,
        Desktop
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BuildType
    {
        Apk,
        Exe,
        Pwa,
        Web
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Orientation
    {
        Portrait,
        Landscape,
        Both
    }

    // [JsonConverter(typeof(StringEnumConverter))]
    // public enum ExperienceType
    // {
    //     Core,
    //     Immersive
    // }

    // SemVer Enums
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ReleaseStatus
    {
        Alpha,
        Beta,
        Stable
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FieldType
    {
        //-------- Simple --------
        boolean,
        color,
        date,
        time,
        dateTime,
        select,
        tags,
        file,
        number,
        text,
        //-------- Complex --------
        list,
        listRow,
        group
        //Dictionary
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PageType
    {
        fields,
        imageFields,
        pageParent
    }
}