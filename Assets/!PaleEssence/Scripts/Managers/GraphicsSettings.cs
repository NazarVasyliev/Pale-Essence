using UnityEngine;
using TMPro;

public class GraphicsSettingsDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private void Start()
    {
        string[] qualityNames = QualitySettings.names;

        dropdown.ClearOptions();
        dropdown.AddOptions(new System.Collections.Generic.List<string>(qualityNames));
        dropdown.value = QualitySettings.GetQualityLevel();
        dropdown.RefreshShownValue();
        dropdown.onValueChanged.AddListener(SetQualityLevel);
    }

    private void SetQualityLevel(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
    }
}
