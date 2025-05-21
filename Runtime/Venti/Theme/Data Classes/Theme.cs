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

        // private Action<string> valueLoadStartEvent;
        // private Action<string> valueLoadEndEvent;

        public bool SetFromJson(JSONObject json)
        {
            if (json == null)
                throw new Exception("JSON is null for theme");

            if (json["hash"] == null)
                throw new Exception("No hash for theme");

            if (json["hash"] == hash)
                return false;

            header.SetFromJson(json["header"].AsObject);
            footer.SetFromJson(json["footer"].AsObject);
            themeColors.SetFromJson(json["themeColors"].AsObject);
            typography.SetFromJson(json["typography"].AsObject);
            buttons.SetFromJson(json["buttons"].AsObject);
            surfaces.SetFromJson(json["surfaces"].AsObject);
            background.SetFromJson(json["background"].AsObject);
            hash = json["hash"];

            return true;
        }

        // public void SetAsyncLoadEvents(Action<string> onValueLoadStart, Action<string> onValueLoadEnd)
        // {
        //     valueLoadStartEvent = onValueLoadStart;
        //     valueLoadEndEvent = onValueLoadEnd;
        // }

        // protected void OnAsyncValueLoadStart(string value)
        // {
        //     valueLoadStartEvent?.Invoke(value);
        // }

        // protected void OnAsyncValueLoadEnd(string value)
        // {
        //     valueLoadEndEvent?.Invoke(value);
        // }

        [Serializable]
        public class Header
        {
            public ThemeImage companyLogo;
            public ThemeImage eventLogo;
            [SerializeField][ReadOnly] private string hash;

            public bool SetFromJson(JSONObject json)
            {
                if (json == null)
                    throw new Exception("JSON is null for header");

                if (json["hash"] == null)
                    throw new Exception("No hash for header");

                if (json["hash"] == hash)
                    return false;

                companyLogo.SetFromJson(json["companyLogo"].AsObject);
                eventLogo.SetFromJson(json["eventLogo"].AsObject);
                hash = json["hash"];

                return true;
            }
        }

        [Serializable]
        public class ThemeImage
        {
            public string imageUrl;
            public Texture2D image;
            public Size size;
            public Alignment alignment;

            public bool SetFromJson(JSONObject json)
            {
                if (json == null)
                {
                    Debug.LogError("JSON is null for theme image");
                    return false;
                }

                //size = (Size)Enum.Parse(typeof(Size), json["size"].Value);
                Enum.TryParse<Size>(json["size"].Value, true, out size);
                Enum.TryParse<Alignment>(json["alignment"].Value, true, out alignment);
                imageUrl = json["imageUrl"];

                ThemeManager.Instance?.OnAssetLoadStart(imageUrl);
                CacheManager.Instance?.GetAsset(imageUrl, CachedAssetType.Image, (texture) =>
                {
                    if (texture == null)
                        return;
                    image = texture as Texture2D;
                    ThemeManager.Instance?.OnAssetLoadEnd(imageUrl);
                });

                return true;
            }
        }

        [Serializable]
        public class Footer
        {
            //public string[] iconUrls;
            //public Texture2D[] icons;
            [SerializeField][ReadOnly] private string hash;

            public bool SetFromJson(JSONObject json)
            {
                if (json == null)
                    throw new Exception("JSON is null for header");

                if (json["hash"] == null)
                    throw new Exception("No hash for header");

                if (json["hash"] == hash)
                    return false;

                //companyLogo.SetFromJson(json["companyLogo"].AsObject);
                //eventLogo.SetFromJson(json["eventLogo"].AsObject);
                hash = json["hash"];

                return true;
            }
        }

        [Serializable]
        public class ThemeColor
        {
            public string primary;
            public Color primaryColorValue;
            public string secondary;
            public Color secondaryColorValue;
            [SerializeField][ReadOnly] private string hash;

            public bool SetFromJson(JSONObject json)
            {
                if (json == null)
                    throw new Exception("JSON is null for header");

                if (json["hash"] == null)
                    throw new Exception("No hash for header");

                if (json["hash"] == hash)
                    return false;

                primary = json["primary"];
                ColorUtility.TryParseHtmlString(primary, out primaryColorValue);
                secondary = json["secondary"];
                ColorUtility.TryParseHtmlString(secondary, out secondaryColorValue);
                hash = json["hash"];

                return true;
            }
        }

        [Serializable]
        public class Typography
        {
            public ThemeFont headingFont;
            public ThemeFont bodyFont;
            public TypeScale typeScales;
            [SerializeField][ReadOnly] private string hash;

            public bool SetFromJson(JSONObject json)
            {
                if (json == null)
                    throw new Exception("JSON is null for header");

                if (json["hash"] == null)
                    throw new Exception("No hash for header");

                if (json["hash"] == hash)
                    return false;

                //companyLogo.SetFromJson(json["companyLogo"].AsObject);
                //eventLogo.SetFromJson(json["eventLogo"].AsObject);
                headingFont.SetFromJson(json["headingFont"].AsObject, "regular");
                bodyFont.SetFromJson(json["bodyFont"].AsObject, "regular");
                typeScales.SetFromJson(json["typeScales"].AsObject);
                hash = json["hash"];

                return true;
            }

            [Serializable]
            public class ThemeFont
            {
                public FontType type;
                public string family;
                public FontVariant variants;
                public float sizeAdjustment;    //???
                public TMPro.TMP_FontAsset fontAsset;

                public bool SetFromJson(JSONObject json, string variantPreference)
                {
                    if (json == null)
                    {
                        Debug.LogError("JSON is null for theme font");
                        return false;
                    }

                    //type = (FontType)Enum.Parse(typeof(FontType), json["type"].Value);
                    Enum.TryParse<FontType>(json["type"].Value, true, out type);
                    family = json["family"];
                    sizeAdjustment = json["sizeAdjustment"].AsFloat;
                    variants.SetFromJson(json["variants"].AsObject);

                    string newFontUrl = json["variants"][variantPreference];
                    if (newFontUrl == null)
                    {
                        if (variants.regular != null)
                            newFontUrl = variants.regular;
                        else if (variants.bold != null)
                            newFontUrl = variants.bold;
                        else if (variants.semiBold != null)
                            newFontUrl = variants.semiBold;
                        else if (variants.light != null)
                            newFontUrl = variants.light;
                        else if (variants.italic != null)
                            newFontUrl = variants.italic;
                        else if (variants.boldItalic != null)
                            newFontUrl = variants.boldItalic;
                    }

                    if (newFontUrl == null)
                    {
                        Debug.LogError("No variant found for font");
                        return false;
                    }

                    ThemeManager.Instance?.OnAssetLoadStart(newFontUrl);
                    CacheManager.Instance?.GetAsset(newFontUrl, CachedAssetType.Font, (font) =>
                    {
                        if (font == null)
                            return;
                        fontAsset = font as TMPro.TMP_FontAsset;
                        ThemeManager.Instance?.OnAssetLoadEnd(newFontUrl);
                    });

                    return true;
                }
            }

            [Serializable]
            public class FontVariant
            {
                public string bold;
                public string semiBold;
                public string regular;
                public string light;
                public string italic;
                public string boldItalic;

                public bool SetFromJson(JSONObject json)
                {
                    if (json == null)
                    {
                        Debug.LogError("JSON is null for theme font variant");
                        return false;
                    }

                    bold = json["bold"];
                    semiBold = json["semiBold"];
                    regular = json["regular"];
                    light = json["light"];
                    italic = json["italic"];
                    boldItalic = json["boldItalic"];

                    return true;
                }
            }
        }

        [Serializable]
        public class TypeScale
        {
            public TypeScaleStruct heading;
            public TypeScaleStruct subHeading;
            public TypeScaleStruct body;
            public TypeScaleStruct caption;

            public bool SetFromJson(JSONObject json)
            {
                if (json == null)
                {
                    Debug.LogError("JSON is null for theme type scale");
                    return false;
                }

                heading.SetFromJson(json["heading"].AsObject);
                subHeading.SetFromJson(json["subHeading"].AsObject);
                body.SetFromJson(json["body"].AsObject);
                caption.SetFromJson(json["caption"].AsObject);

                return true;
            }

            [Serializable]
            public class TypeScaleStruct
            {
                public TypographyType font = TypographyType.body;
                public string color;
                public Color colorValue;

                public bool SetFromJson(JSONObject json)
                {
                    if (json == null)
                    {
                        Debug.LogError("JSON is null for theme type scale");
                        return false;
                    }

                    Enum.TryParse<TypographyType>(json["font"].Value, true, out font);
                    color = json["color"];
                    ColorUtility.TryParseHtmlString(color, out colorValue);

                    return true;
                }
            }
        }
    }

    [Serializable]
    public class ThemeButton
    {
        public ButtonStruct primary;
        public ButtonStruct secondary;
        [SerializeField][ReadOnly] private string hash;

        public bool SetFromJson(JSONObject json)
        {
            if (json == null)
                throw new Exception("JSON is null for header");

            if (json["hash"] == null)
                throw new Exception("No hash for header");

            if (json["hash"] == hash)
                return false;

            primary.SetFromJson(json["primary"].AsObject);
            secondary.SetFromJson(json["secondary"].AsObject);
            hash = json["hash"];

            return true;
        }
    }

    [Serializable]
    public class ButtonStruct
    {
        public string textColor;
        public Color textColorValue;
        public RoundingFull rounded;
        public BorderThickness borderThickness;

        public bool SetFromJson(JSONObject json)
        {
            if (json == null)
            {
                Debug.LogError("JSON is null for theme button");
                return false;
            }

            textColor = json["textColor"];
            ColorUtility.TryParseHtmlString(textColor, out textColorValue);
            Enum.TryParse<RoundingFull>(json["rounded"].Value, true, out rounded);
            Enum.TryParse<BorderThickness>(json["borderThickness"].Value, true, out borderThickness);

            return true;
        }
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
        [SerializeField][ReadOnly] private string hash;

        public bool SetFromJson(JSONObject json)
        {
            if (json == null)
                throw new Exception("JSON is null for header");

            if (json["hash"] == null)
                throw new Exception("No hash for header");

            if (json["hash"] == hash)
                return false;

            //primary.SetFromJson(json["primary"].AsObject);
            //secondary.SetFromJson(json["secondary"].AsObject);
            color = json["color"];
            ColorUtility.TryParseHtmlString(color, out colorValue);
            Enum.TryParse<Rounding>(json["rounded"].Value, true, out rounded);
            Enum.TryParse<BorderThickness>(json["borderThickness"].Value, true, out borderThickness);
            borderColor = json["borderColor"];
            ColorUtility.TryParseHtmlString(borderColor, out borderColorValue);
            hash = json["hash"];

            return true;
        }
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
        [SerializeField][ReadOnly] private string hash;

        public bool SetFromJson(JSONObject json)
        {
            if (json == null)
                throw new Exception("JSON is null for header");

            if (json["hash"] == null)
                throw new Exception("No hash for header");

            if (json["hash"] == hash)
                return false;

            color = json["color"];
            portraitImageUrl = json["portraitImageUrl"].Value;
            landscapeImageUrl = json["landscapeImageUrl"].Value;
            hash = json["hash"];

            ColorUtility.TryParseHtmlString(color, out colorValue);

            ThemeManager.Instance?.OnAssetLoadStart(portraitImageUrl);
            CacheManager.Instance?.GetAsset(portraitImageUrl, CachedAssetType.Image, (texture) =>
            {
                if (texture == null)
                    return;
                portraitImage = texture as Texture2D;
                ThemeManager.Instance?.OnAssetLoadEnd(portraitImageUrl);
            });

            ThemeManager.Instance?.OnAssetLoadStart(landscapeImageUrl);
            CacheManager.Instance?.GetAsset(landscapeImageUrl, CachedAssetType.Image, (texture) =>
            {
                if (texture == null)
                    return;
                landscapeImage = texture as Texture2D;
                ThemeManager.Instance?.OnAssetLoadEnd(landscapeImageUrl);
            });

            return true;
        }
    }
}