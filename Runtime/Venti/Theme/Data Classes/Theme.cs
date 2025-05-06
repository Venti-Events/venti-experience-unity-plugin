using Newtonsoft.Json;
using SimpleJSON;
using System;
using System.Collections;
using System.Web;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.Networking;
using Venti.Experience;
using static UnityEditorInternal.ReorderableList;
using static Venti.Theme.Theme;

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

        [Serializable]
        public class Header
        {
            public ThemeImage companyLogo;
            public ThemeImage eventLogo;
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
            string[] iconUrls;
            Texture2D[] icons;
        }

        [Serializable]
        public class ThemeColor
        {
            public string primary;
            public Color primaryColorValue;
            public string secondary;
            public Color secondaryColorValue;
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
                public string family;
                public FontVariant variants;
                public float sizeAdjustment;    //???

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
        }

        [Serializable]
        public class Background
        {
            public string color;
            public Color colorValue;
            public string portraitImageUrl;
            public string landscapeImageUrl;
        }

        public bool SetFromJson(JSONObject json, bool useCache)
        {
            // Header
            string newHeaderStr = json["header"].ToString();
            Header newHeader = JsonConvert.DeserializeObject<Header>(newHeaderStr);

            //LoadImage(header.companyLogo.imageUrl, newHeader.companyLogo.imageUrl, (Texture2D tex) => { newHeader.companyLogo.image = tex; });
            //LoadImage(header.eventLogo.imageUrl, newHeader.eventLogo.imageUrl, (Texture2D tex) => { newHeader.eventLogo.image = tex; });
            // TODO: Discard old texture
            header = newHeader;

            // Footer
            string newFooterStr = json["footer"].ToString();
            Footer newFooter = JsonConvert.DeserializeObject<Footer>(newFooterStr);
            // TODO load list of images
            footer = newFooter;

            // ThemeColors
            string newThemeColorsStr = json["themeColors"].ToString();
            themeColors = JsonConvert.DeserializeObject<ThemeColor>(newThemeColorsStr);
            ColorUtility.TryParseHtmlString(themeColors.primary, out themeColors.primaryColorValue);
            ColorUtility.TryParseHtmlString(themeColors.secondary, out themeColors.secondaryColorValue);

            // Typography
            string newTypographyStr = json["typography"].ToString();
            typography = JsonConvert.DeserializeObject<Typography>(newTypographyStr);
            ColorUtility.TryParseHtmlString(typography.typeScales.heading.color, out typography.typeScales.heading.colorValue);
            ColorUtility.TryParseHtmlString(typography.typeScales.subHeading.color, out typography.typeScales.subHeading.colorValue);
            ColorUtility.TryParseHtmlString(typography.typeScales.body.color, out typography.typeScales.body.colorValue);
            ColorUtility.TryParseHtmlString(typography.typeScales.caption.color, out typography.typeScales.caption.colorValue);

            // Typography
            string newButtonsStr = json["typography"].ToString();
            typography = JsonConvert.DeserializeObject<Typography>(newTypographyStr);


            //string decodedUrl = HttpUtility.UrlDecode(url);

            //string oldFileName = null;
            //string newFileName = null;

            //// Extract file names from urls
            //if (valueRaw != null)
            //    oldFileName = valueRaw.Substring(valueRaw.LastIndexOf('/') + 1);
            //newFileName = newValue.Substring(newValue.LastIndexOf('/') + 1);
            ////Debug.Log("Old value: " + valueRaw);
            ////Debug.Log("Old filename: " + oldFileName);

            ////Debug.Log("New value: " + newValue);
            ////Debug.Log("New filename: " + newFileName);

            //valueRaw = newValue;

            //// Delete old file
            //if (oldFileName != null || oldFileName == "")
            //    FileHandler.DeleteFile(oldFileName, ExperienceManager.appFolderName);

            //if (FileHandler.FileExists(newFileName, ExperienceManager.appFolderName))
            //{
            //    // If fetched image exists in cache. e.g. first run
            //    string filePath = FileHandler.GetFilePath(newFileName, ExperienceManager.appFolderName);
            //    StartCoroutine(FetchImage(filePath, newFileName, false));
            //    //Debug.Log("Fetching image from cache: " + filePath);
            //}
            //else
            //{
            //    // Fetch new image from web and save to cache
            //    StartCoroutine(FetchImage(newValue, newFileName, true));
            //    //Debug.Log("Fetching image from web: " + newValue);
            //}

            return true;
        }

        bool LoadImage(string oldUrl, string newUrl, Action<Texture2D> callback)
        {
            // Same images. No need to load
            if (oldUrl == newUrl)
                return false;

            string newUrlDecoded;
            if (newUrl == null)
            {
                Debug.LogWarning("value is null in JSON for theme image");
                return false;
            }
            else
                newUrlDecoded = HttpUtility.UrlDecode(newUrl);

            string oldFileName = null;
            string newFileName = null;

            // Extract file names from urls
            if (oldUrl != null)
                oldFileName = oldUrl.Substring(oldUrl.LastIndexOf('/') + 1);
            newFileName = newUrlDecoded.Substring(newUrlDecoded.LastIndexOf('/') + 1);

            // Delete old file
            if (oldFileName != null || oldFileName == "")
                FileHandler.DeleteFile(oldFileName, ExperienceManager.appFolderName);

            if (FileHandler.FileExists(newFileName, ExperienceManager.appFolderName))
            {
                // If fetched image exists in cache. e.g. first run
                string filePath = FileHandler.GetFilePath(newFileName, ExperienceManager.appFolderName);
                //StartCoroutine(FetchImage(filePath, newFileName, false));
                //Debug.Log("Fetching image from cache: " + filePath);
            }
            else
            {
                // Fetch new image from web and save to cache
                //StartCoroutine(FetchImage(newValue, newFileName, true));
                //Debug.Log("Fetching image from web: " + newValue);
            }

            return true;
        }

        /*IEnumerator FetchImage(string url, string fileName, bool saveToCache)
        {
            //using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                // Request and wait for the desired page
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(webRequest.error);
                }
                else
                {
                    value = DownloadHandlerTexture.GetContent(webRequest);

                    if (saveToCache)
                    {
                        byte[] bytes = webRequest.downloadHandler.data;
                        FileHandler.WriteBytes(bytes, fileName, ExperienceManager.appFolderName);
                        //Debug.Log("Saved image to cache: " + fileName);
                    }

                    onChange.Invoke(value);
                    onChangeWithId.Invoke(id, value);
                }
            }
        }*/
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