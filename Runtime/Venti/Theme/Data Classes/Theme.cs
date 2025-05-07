using System;
using UnityEngine;
using SimpleJSON;

namespace Venti.Theme
{
    [Serializable]
    public class Theme
    {
        public Header header;
        public Footer footer;
        public ThemeColor themeColors;
        public Typography typography;
        public ThemeButton buttons;
        public Surface surfaces;
        public Background background;
        [SerializeField][ReadOnly] private string hash;

        //public bool SetFromJson(JSONObject json)
        //{
        //    if (json == null)
        //        throw new Exception("JSON is null for theme");

        //    if (json["hash"] == null)
        //        throw new Exception("No hash for theme");

        //    if (json["hash"] == hash)
        //        return false;

        //    header.SetFromJson(json["header"]);
        //    footer.SetFromJson(json["footer"]);
        //    themeColors.SetFromJson(json["themeColors"]);
        //    typography.SetFromJson(json["typography"]);
        //    buttons.SetFromJson(json["buttons"]);
        //    surfaces.SetFromJson(json["surfaces"]);
        //    background.SetFromJson(json["background"]);
        //    hash = json["hash"];

        //    return true;
        //}

        [Serializable]
        public class Header
        {
            public ThemeImage companyLogo;
            public ThemeImage eventLogo;
            public string hash;
            //[SerializeField][ReadOnly] private string hash;

            //public bool SetFromJson(JSONObject json)
            //{
            //    if (json == null)
            //        throw new Exception("JSON is null for header");

            //    if (json["hash"] == null)
            //        throw new Exception("No hash for header");

            //    if (json["hash"] == hash)
            //        return false;

            //    companyLogo.SetFromJson(json["companyLogo"]);
            //    eventLogo.SetFromJson(json["eventLogo"]);
            //    hash = json["hash"];

            //    return true;
            //}
        }

        [Serializable]
        public class ThemeImage
        {
            public string imageUrl;
            public Texture2D image;
            public Size size;
            public Alignment alignment;
        }

        [Serializable]
        public class Footer
        {
            //public string[] iconUrls;
            //public Texture2D[] icons;
            public string hash;
        }

        [Serializable]
        public class ThemeColor
        {
            public string primary;
            public Color primaryColorValue;
            public string secondary;
            public Color secondaryColorValue;
            public string hash;
        }

        [Serializable]
        public class Typography
        {
            public ThemeFont headingFont;
            public ThemeFont bodyFont;
            public TypeScale typeScales;
            public string hash;

            [Serializable]
            public class ThemeFont
            {
                public FontType type;
                public string family;
                public FontVariant variants;
                public float sizeAdjustment;    //???
                public TMPro.TMP_FontAsset fontAsset;

                [Serializable]
                public class FontVariant
                {
                    public string bold;
                    public string semiBold;
                    public string regular;
                    public string light;
                    public string italic;
                    public string boldItalic;
                }
            }

            [Serializable]
            public class TypeScale
            {
                public TypeScaleStruct heading;
                public TypeScaleStruct subHeading;
                public TypeScaleStruct body;
                public TypeScaleStruct caption;

                [Serializable]
                public class TypeScaleStruct
                {
                    public TypographyType font = TypographyType.body;
                    public string color;
                    public Color colorValue;
                }
            }

        }

        [Serializable]
        public class ThemeButton
        {
            public ButtonStruct primary;
            public ButtonStruct secondary;
            public string hash;
        }

        [Serializable]
        public class ButtonStruct
        {
            public string textColor;
            public Color textColorValue;
            public RoundingFull rounded;
            public BorderThickness borderThickness;
        }

        [Serializable]
        public class Surface
        {
            public string color;
            public Color colorValue;
            public Rounding rounded;
            public BorderThickness borderThickness;
            public string borderColor;
            public Color borderColorValue;
            public string hash;
        }

        [Serializable]
        public class Background
        {
            public string color;
            public Color colorValue;
            public string portraitImageUrl;
            public Texture2D portraitImage;
            public string landscapeImageUrl;
            public Texture2D landscapeImage;
            public string hash;
        }
    }

    //[Serializable]
    //public class Theme
    //{
    //    public ThemeImage companyLogo;
    //    public ThemeImage eventLogo;
    //    public Footer footer;
    //    public ThemeColor themeColors;
    //    public Typography typography;
    //    public ThemeButton buttons;
    //    public Surface surfaces;
    //    public Background background;

    //    [Serializable]
    //    public class ThemeImage
    //    {
    //        public string imageUrl;
    //        public Size size;
    //        public Alignment alignment;
    //    }

    //    [Serializable]
    //    public class Footer
    //    {
    //        public FooterText text;
    //        public ThemeImage image;

    //        [Serializable]
    //        public class FooterText
    //        {
    //            public string text;
    //            public Alignment alignment;
    //        }
    //    }

    //    [Serializable]
    //    public class ThemeColor
    //    {
    //        public Color primary;
    //        public Color secondary;
    //    }

    //    [Serializable]
    //    public class Typography
    //    {
    //        public ThemeFont headingFont;
    //        public ThemeFont bodyFont;
    //        public TypeScale typeScales;

    //        [Serializable]
    //        public class ThemeFont
    //        {
    //            public FontType type;
    //            public FontVariant variants;
    //            //public float sizeAdjustment;

    //            [Serializable]
    //            public class FontVariant
    //            {
    //                public string bold;
    //                public string semiBold;
    //                public string regular;
    //                public string light;
    //                public string italic;
    //                public string boldItalic;
    //            }
    //        }

    //        [Serializable]
    //        public class TypeScale
    //        {
    //            public TypeScaleStruct heading;
    //            public TypeScaleStruct subHeading;
    //            public TypeScaleStruct body;
    //            public TypeScaleStruct caption;
    //            //public TypeScaleStruct primaryButton;
    //            //public TypeScaleStruct secondaryButton;

    //            [Serializable]
    //            public class TypeScaleStruct
    //            {
    //                //public TypographyType font;
    //                public Color color;
    //            }
    //        }

    //    }

    //    [Serializable]
    //    public class ThemeButton
    //    {
    //        public ButtonStruct primary;
    //        public ButtonStruct secondary;
    //    }

    //    [Serializable]
    //    public class ButtonStruct
    //    {
    //        public Color textColor;
    //        public RoundingFull rounded;
    //        public BorderThickness borderThickness;
    //        public Color borderColor;
    //    }

    //    [Serializable]
    //    public class Surface
    //    {
    //        public Color color;
    //        public Rounding rounded;
    //        public BorderThickness borderThickness;
    //        public Color borderColor;
    //    }

    //    [Serializable]
    //    public class Background
    //    {
    //        public Color color;
    //        public string portraitImageUrl;
    //        public string landscapeImageUrl;
    //    }
    //}
}