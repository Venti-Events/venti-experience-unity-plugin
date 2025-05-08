using Newtonsoft.Json;
using SimpleJSON;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static Venti.Theme.Theme;

namespace Venti.Theme
{
    public class ThemeManager : MonoBehaviour
    {
        [field: SerializeField] public Theme theme { get; private set; }

        public UnityEvent onThemeUpdate;

        // JSON File Path
        public const string themeFolderName = "theme";
        private const string configFileName = "theme-config";

        //public TMPro.TMP_Text testText;

        private void Start()
        {
            LoadFromLocalJson();
        }

        public bool LoadFromLocalJson()
        {
            string jsonStr = FileHandler.ReadFile(configFileName + ".json", themeFolderName);

            if (string.IsNullOrEmpty(jsonStr))
            {
                Debug.LogError("JSON string for loading theme is null or empty");
                return false;
            }

            JSONObject json = JSON.Parse(jsonStr).AsObject;
            return LoadJson(json, false);
        }

        public bool LoadFromWebJson(string jsonStr)
        {
            //JSONObject json = JSON.Parse(jsonStr).AsObject;
            //if (json == null)
            //{
            //    Debug.LogError("JSON for loading experience is null");
            //    return false;
            //}

            //JSONObject configJson = json["data"]["theme"].AsObject;
            //return LoadJson(configJson.ToString(), true);

            if (string.IsNullOrEmpty(jsonStr))
            {
                Debug.LogError("JSON string for loading theme is null or empty");
                return false;
            }

            JSONObject json = JSON.Parse(jsonStr).AsObject;
            if (json == null)
            {
                Debug.LogError("JSON for loading experience is null");
                return false;
            }

            JSONObject configJson = json["data"]["theme"].AsObject;
            return LoadJson(configJson, true);
        }

        bool LoadJson(JSONObject json, bool saveJson = true)
        {
            try
            {
                //theme = JsonConvert.DeserializeObject<ThemeResponse>(jsonStr).data.theme;

                if (json == null)
                    throw new Exception("JSON for loading experience is null");

                // TODO: Check whether versions match

                // Update all fields from json
                if (SetFromJson(json))
                {
                    // TODO: Only call onThemeUpdate if all theme elements have either passed or failed
                    //onThemeUpdate?.Invoke();
                    StartCoroutine(InvokeAfterDelay());

                    if (saveJson)
                        FileHandler.WriteString(json.ToString(), configFileName + ".json", themeFolderName);
                }

                return true;
            }
            catch (JsonException e)
            {
                Debug.LogError($"Failed to load theme JSON: {e.Message}");
                return false;
            }
        }

        IEnumerator InvokeAfterDelay(float delay = 1f)
        {
            yield return new WaitForSeconds(delay);
            onThemeUpdate?.Invoke();
        }


        //[Serializable]
        //private class ThemeResponse
        //{
        //    public bool success;
        //    public ThemeData data;
        //    public string message;
        //}

        //[Serializable]
        //private class ThemeData
        //{
        //    public string id;
        //    public Theme theme;
        //}

        public bool SetFromJson(JSONObject json)
        {
            Debug.Log("Setting Theme from JSON");

            theme.SetFromJson(json);

            /*
            // Header
            if (theme.header.hash != json["header"]["hash"].ToString())
            {
                string newHeaderStr = json["header"].ToString();
                Header newHeader = JsonConvert.DeserializeObject<Header>(newHeaderStr);

                // TODO: Dispose old texture
                newHeader.companyLogo.image = theme.header.companyLogo.image;
                CacheManager.Instance.GetImage(theme.header.companyLogo.imageUrl, newHeader.companyLogo.imageUrl, themeFolderName, (Texture2D tex) =>
                {
                    if (tex != null)
                    {
                        Destroy(theme.header.companyLogo.image);
                        theme.header.companyLogo.image = tex;
                    }
                });

                newHeader.eventLogo.image = theme.header.eventLogo.image;
                CacheManager.Instance.GetImage(theme.header.eventLogo.imageUrl, newHeader.eventLogo.imageUrl, themeFolderName, (Texture2D tex) =>
                {
                    if (tex != null)
                    {
                        Destroy(theme.header.eventLogo.image);
                        theme.header.eventLogo.image = tex;
                    }
                });
                theme.header = newHeader;
            }

            // Footer
            if (theme.footer.hash != json["footer"]["hash"].ToString())
            {
                string newFooterStr = json["footer"].ToString();
                Footer newFooter = JsonConvert.DeserializeObject<Footer>(newFooterStr);
                // TODO load list of images
                theme.footer = newFooter;
            }

            // ThemeColors
            if (theme.themeColors.hash != json["themeColors"]["hash"].ToString())
            {
                string newThemeColorsStr = json["themeColors"].ToString();
                theme.themeColors = JsonConvert.DeserializeObject<ThemeColor>(newThemeColorsStr);
                ColorUtility.TryParseHtmlString(theme.themeColors.primary, out theme.themeColors.primaryColorValue);
                ColorUtility.TryParseHtmlString(theme.themeColors.secondary, out theme.themeColors.secondaryColorValue);
            }

            // Typography
            if (theme.typography.hash != json["typography"]["hash"].ToString())
            {
                string newTypographyStr = json["typography"].ToString();
                Typography newTypography = JsonConvert.DeserializeObject<Typography>(newTypographyStr);
                ColorUtility.TryParseHtmlString(newTypography.typeScales.heading.color, out newTypography.typeScales.heading.colorValue);
                ColorUtility.TryParseHtmlString(newTypography.typeScales.subHeading.color, out newTypography.typeScales.subHeading.colorValue);
                ColorUtility.TryParseHtmlString(newTypography.typeScales.body.color, out newTypography.typeScales.body.colorValue);
                ColorUtility.TryParseHtmlString(newTypography.typeScales.caption.color, out newTypography.typeScales.caption.colorValue);

                newTypography.headingFont.fontAsset = theme.typography.headingFont.fontAsset;
                CacheManager.Instance.GetFont(theme.typography.headingFont.variants.regular, newTypography.headingFont.variants.regular, themeFolderName, (TMPro.TMP_FontAsset font) =>
                {
                    if (font != null)
                    {
                        Destroy(theme.typography.headingFont.fontAsset);
                        theme.typography.headingFont.fontAsset = font;

                        //testText.font = font;
                        //testText.color = theme.typography.typeScales.heading.colorValue;
                        //testText.ForceMeshUpdate();
                    }
                });
                theme.typography = newTypography;
            }

            // Buttons
            if (theme.buttons.hash != json["buttons"]["hash"].ToString())
            {
                string newButtonsStr = json["buttons"].ToString();
                theme.buttons = JsonConvert.DeserializeObject<ThemeButton>(newButtonsStr);
                ColorUtility.TryParseHtmlString(theme.buttons.primary.textColor, out theme.buttons.primary.textColorValue);
                ColorUtility.TryParseHtmlString(theme.buttons.secondary.textColor, out theme.buttons.secondary.textColorValue);
            }

            // Surfaces
            if (theme.surfaces.hash != json["surfaces"]["hash"].ToString())
            {
                string newSurfacesStr = json["surfaces"].ToString();
                theme.surfaces = JsonConvert.DeserializeObject<Surface>(newSurfacesStr);
                ColorUtility.TryParseHtmlString(theme.surfaces.color, out theme.surfaces.colorValue);
                ColorUtility.TryParseHtmlString(theme.surfaces.borderColor, out theme.surfaces.borderColorValue);
            }

            // Background
            if (theme.background.hash != json["background"]["hash"].ToString())
            {
                string newBackgroundStr = json["background"].ToString();
                Background newBackground = JsonConvert.DeserializeObject<Background>(newBackgroundStr);
                ColorUtility.TryParseHtmlString(newBackground.color, out newBackground.colorValue);
                newBackground.landscapeImage = theme.background.landscapeImage;

                // TODO: Dispose old texture
                newBackground.portraitImage = theme.background.portraitImage;
                CacheManager.Instance.GetImage(theme.background.portraitImageUrl, newBackground.portraitImageUrl, themeFolderName, (Texture2D tex) => { newBackground.portraitImage = tex; });

                newBackground.landscapeImage = theme.background.landscapeImage;
                CacheManager.Instance.GetImage(theme.background.landscapeImageUrl, newBackground.landscapeImageUrl, themeFolderName, (Texture2D tex) => { newBackground.landscapeImage = tex; });

                theme.background = newBackground;
            }*/

            return true;
        }
    }
}