using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AnimationProcessorWindow : EditorWindow
{
    public List<AnimationClip> clips = new();

    [Header("Path Modification")]
    public int parentsToRemove = 0;
    public string prefixToAdd = "";

    [Header("Filters")]
    public string removeIfContains = "";
    public bool removePosition = false;
    public bool removeRotation = false;
    public bool removeScale = true;

    [MenuItem("Tools/Animation Processor")]
    public static void ShowWindow()
    {
        GetWindow<AnimationProcessorWindow>("Anim Processor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Path Settings", EditorStyles.boldLabel);
        parentsToRemove = EditorGUILayout.IntField("Parents To Remove", parentsToRemove);
        prefixToAdd = EditorGUILayout.TextField("Prefix To Add", prefixToAdd);
        removeIfContains = EditorGUILayout.TextField("Remove If Path Contains", removeIfContains);

        EditorGUILayout.Space();
        GUILayout.Label("Curve Type Filters", EditorStyles.boldLabel);
        removePosition = EditorGUILayout.Toggle("Remove Position", removePosition);
        removeRotation = EditorGUILayout.Toggle("Remove Rotation", removeRotation);
        removeScale = EditorGUILayout.Toggle("Remove Scale", removeScale);

        EditorGUILayout.Space();

        // Отрисовка списка клипов
        ScriptableObject target = this;
        SerializedObject so = new(target);
        SerializedProperty clipsProperty = so.FindProperty("clips");
        EditorGUILayout.PropertyField(clipsProperty, new GUIContent("Animation Clips"), true);
        so.ApplyModifiedProperties();

        if (GUILayout.Button("Process Animations"))
        {
            ProcessAnimations();
        }
    }

    private void ProcessAnimations()
    {
        if (clips == null || clips.Count == 0)
            return;

        foreach (var clip in clips)
        {
            if (clip == null)
                continue;

            Undo.RecordObject(clip, "Process Animation Curves");
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

            foreach (var binding in bindings)
            {
                bool shouldRemove = false;

                // 1. Проверка на содержание строки в пути
                if (
                    !string.IsNullOrEmpty(removeIfContains)
                    && binding.path.Contains(removeIfContains)
                )
                {
                    shouldRemove = true;
                }

                // 2. Проверка на тип кривой
                if (!shouldRemove)
                {
                    if (removePosition && binding.propertyName.Contains("m_LocalPosition"))
                        shouldRemove = true;
                    if (removeRotation && binding.propertyName.Contains("m_LocalRotation"))
                        shouldRemove = true;
                    if (removeScale && binding.propertyName.Contains("m_LocalScale"))
                        shouldRemove = true;
                }

                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);

                // Удаляем текущую привязку
                AnimationUtility.SetEditorCurve(clip, binding, null);

                if (!shouldRemove)
                {
                    // 3. Модификация пути
                    string newPath = binding.path;

                    // Убираем родителей
                    if (parentsToRemove > 0)
                    {
                        string[] parts = newPath.Split('/');
                        int skip = Mathf.Min(parentsToRemove, parts.Length);
                        newPath = string.Join("/", parts.Skip(skip));
                    }

                    // Добавляем префикс
                    if (!string.IsNullOrEmpty(prefixToAdd))
                    {
                        string cleanPrefix = prefixToAdd.TrimEnd('/');
                        newPath = string.IsNullOrEmpty(newPath)
                            ? cleanPrefix
                            : $"{cleanPrefix}/{newPath}";
                    }

                    EditorCurveBinding newBinding = binding;
                    newBinding.path = newPath;
                    AnimationUtility.SetEditorCurve(clip, newBinding, curve);
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Animations processed! Curves filtered and paths updated.");
    }
}
