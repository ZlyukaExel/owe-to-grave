using System.IO;
using UnityEditor;
using UnityEngine;

public class PrefabToImage : EditorWindow
{
    GameObject targetPrefab;

    [MenuItem("Tools/Save Prefab Icon")]
    public static void ShowWindow() => GetWindow<PrefabToImage>("Icon Saver");

    void OnGUI()
    {
        targetPrefab = (GameObject)
            EditorGUILayout.ObjectField("Prefab", targetPrefab, typeof(GameObject), false);

        if (GUILayout.Button("Save Icon as PNG"))
        {
            if (targetPrefab == null)
                return;

            Texture2D texture = AssetPreview.GetAssetPreview(targetPrefab);

            if (texture == null)
            {
                Debug.LogWarning("Prеview is not ready yet, try later");
                return;
            }

            Texture2D readableTexture = new(
                texture.width,
                texture.height,
                TextureFormat.RGBA32,
                false
            );
            Graphics.CopyTexture(texture, readableTexture);

            byte[] bytes = readableTexture.EncodeToPNG();

            string path = Application.dataPath + "/" + targetPrefab.name + "_Icon.png";
            File.WriteAllBytes(path, bytes);

            AssetDatabase.Refresh();
            Debug.Log("Icon saved: " + path);
        }
    }
}
