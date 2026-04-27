using UnityEngine;
using UnityEditor;
using System;

public class CopyRotationEditor : EditorWindow
{
    private static Transform sourceTransform;
    private static Quaternion storedRotation = Quaternion.identity;

    [MenuItem("Tools/Rotation Tools/Copy World Rotation %&c", false, 1)]
    public static void CopyWorldRotation()
    {
        if (Selection.transforms.Length == 1)
        {
            sourceTransform = Selection.activeTransform;
            storedRotation = sourceTransform.rotation;
            Debug.Log($"Скопировано мировое вращение из {sourceTransform.name}");
        }
        else
        {
            EditorUtility.DisplayDialog("Ошибка", "Выберите один объект для копирования вращения.", "OK");
        }
    }

    [MenuItem("Tools/Rotation Tools/Paste World Rotation %&v", false, 2)]
    public static void PasteWorldRotation()
    {
        if (sourceTransform == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Сначала скопируйте вращение с другого объекта.", "OK");
            return;
        }

        foreach (Transform target in Selection.transforms)
        {
            Undo.RecordObject(target, "Paste World Rotation");
            target.rotation = storedRotation;
        }

        Debug.Log($"Вращение применено к {Selection.transforms.Length} объектам");
    }

    [MenuItem("Tools/Rotation Tools/Copy World Rotation", true)]
    [MenuItem("Tools/Rotation Tools/Paste World Rotation", true)]
    public static bool ValidateSelection()
    {
        return !EditorApplication.isPlaying;
    }

    [MenuItem("CONTEXT/Transform/Copy World Rotation", false, 1)]
    public static void ContextCopyRotation(MenuCommand command)
    {
        sourceTransform = (Transform)command.context;
        storedRotation = sourceTransform.rotation;
        Debug.Log($"Скопировано мировое вращение из {sourceTransform.name}");
    }

    [MenuItem("CONTEXT/Transform/Paste World Rotation", false, 2)]
    public static void ContextPasteRotation(MenuCommand command)
    {
        Transform target = (Transform)command.context;

        if (sourceTransform == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Сначала скопируйте вращение с другого объекта.", "OK");
            return;
        }

        Undo.RecordObject(target, "Paste World Rotation");
        target.rotation = storedRotation;

        Debug.Log($"Вращение применено к {target.name}");
    }
}