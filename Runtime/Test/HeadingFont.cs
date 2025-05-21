using UnityEngine;
using TMPro;

namespace Venti.Theme
{
    [RequireComponent(typeof(TMP_Text))]
    public class HeadingFont : MonoBehaviour
    {
        private void OnEnable()
        {
            //SettingsManager.Instance.themeManager.onThemeChange.AddListener(OnUpdate);
            //OnUpdate();
        }
        public void OnUpdate()
        {
            TMP_Text text = GetComponent<TMP_Text>();
            text.font = ThemeManager.Instance.theme.typography.headingFont.fontAsset;
            text.color = ThemeManager.Instance.theme.typography.typeScales.heading.colorValue;
            text.ForceMeshUpdate();
        }
    }

}