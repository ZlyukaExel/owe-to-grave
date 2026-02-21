using UnityEditor;
using UnityEngine;
using TMPro;

public class ReplaceTMPFont : EditorWindow
{
    private TMP_FontAsset newFont;

    [MenuItem("Tools/Replace fonts (TMP)")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceTMPFont>("Replace TMP font");
    }

    private void OnGUI()
    {
        GUILayout.Label("Choose new TMP font", EditorStyles.boldLabel);
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP font", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Apply for all TMP_Text on scene"))
        {
            ReplaceFont();
        }
    }

    private void ReplaceFont()
    {
        TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (TMP_Text text in allTexts)
        {
            text.font = newFont;
            EditorUtility.SetDirty(text);
        }

        Debug.Log($"{allTexts.Length} TMP_Text fonts have been updated");
    }
}