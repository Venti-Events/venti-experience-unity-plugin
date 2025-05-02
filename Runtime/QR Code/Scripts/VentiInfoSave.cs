using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VentiInfoSave : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI appName;

    [SerializeField]
    private TextMeshProUGUI version;

    [SerializeField]
    private TextMeshProUGUI cloudConnectionStatus;

    [Space]

    [SerializeField]
    private SettingsState settingsState;

    [SerializeField]
    private TextMeshProUGUI settingsStateText;

    [Space] 

    [SerializeField]
    private ThemeState themeState;

    [SerializeField]
    private TextMeshProUGUI themeStateText;

    [Space]

    [SerializeField]
    private TextMeshProUGUI themeConfigLastCloudSync;

    [SerializeField]
    private TextMeshProUGUI settingsLastCloudSync;

    [Space]

    [SerializeField]
    private Button clearThemeConfigBtn;

    [SerializeField]
    private Button clearSettingConfigBtn;

    [SerializeField]
    private Button resetAppKeyBtn;
}

public enum SettingsState { Initial, Loading, Ready, Failed}
public enum ThemeState { Initial, Loading, Ready, Failed}
