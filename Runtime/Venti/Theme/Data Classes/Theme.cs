using System;
using UnityEngine;

namespace Venti.Theme
{
    [Serializable]
    public class Theme
    {
        public ThemeImage companyLogo;
        public ThemeImage eventLogo;
        public Footer footer;
        public ThemeColor themeColors;
        public Typography typography;
        public ThemeButton buttons;
        public Surface surfaces;
        public Background background;

        [Serializable]
        public class ThemeImage
        {
            public string imageUrl;
            public Size size;
            public Alignment alignment;
        }

        [Serializable]
        public class Footer
        {
            public FooterText text;
            public ThemeImage image;

            [Serializable]
            public class FooterText
            {
                public string text;
                public Alignment alignment;
            }
        }

        [Serializable]
        public class ThemeColor
        {
            public Color primary;
            public Color secondary;
        }

        [Serializable]
        public class Typography
        {
            public ThemeFont headingFont;
            public ThemeFont bodyFont;
            public TypeScale typeScales;

            [Serializable]
            public class ThemeFont
            {
                public FontType type;
                public FontVariant variants;
                //public float sizeAdjustment;

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
                //public TypeScaleStruct primaryButton;
                //public TypeScaleStruct secondaryButton;

                [Serializable]
                public class TypeScaleStruct
                {
                    //public TypographyType font;
                    public Color color;
                }
            }

        }

        [Serializable]
        public class ThemeButton
        {
            public ButtonStruct primary;
            public ButtonStruct secondary;
        }

        [Serializable]
        public class ButtonStruct
        {
            public Color textColor;
            public RoundingFull rounded;
            public BorderThickness borderThickness;
            public Color borderColor;
        }

        [Serializable]
        public class Surface
        {
            public Color color;
            public Rounding rounded;
            public BorderThickness borderThickness;
            public Color borderColor;
        }

        [Serializable]
        public class Background
        {
            public Color color;
            public string portraitImageUrl;
            public string landscapeImageUrl;
        }
    }
}