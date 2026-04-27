using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class RotationAnimationEditor : EditorWindow
{
    private AnimationClip animationClip;
    private GameObject targetObject;
    private Vector3 worldRotation = Vector3.zero;

    [MenuItem("Window/Rotation Animation Editor")]
    public static void ShowWindow()
    {
        GetWindow<RotationAnimationEditor>("Rotation Animator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Set up rotation animation", EditorStyles.boldLabel);

        animationClip = EditorGUILayout.ObjectField("Animation Clip", animationClip, typeof(AnimationClip), false) as AnimationClip;
        targetObject = EditorGUILayout.ObjectField("Target GameObject", targetObject, typeof(GameObject), true) as GameObject;
        worldRotation = EditorGUILayout.Vector3Field("World Rotation", worldRotation);

        if (GUILayout.Button("Apply Rotation to Animation"))
        {
            ApplyRotationToAnimation();
        }
    }

    private void ApplyRotationToAnimation()
    {
        if (animationClip == null)
        {
            Debug.LogError("Animation Clip is not assigned.");
            return;
        }

        if (targetObject == null)
        {
            Debug.LogError("Target GameObject is not assigned.");
            return;
        }

        // Получаем длительность клипа в секундах
        float duration = animationClip.length;

        // Определяем частоту кадров (обычно 60 FPS)
        int frameRate = 60;
        float timeStep = 1.0f / frameRate;

        AnimationCurve rotX = new AnimationCurve();
        AnimationCurve rotY = new AnimationCurve();
        AnimationCurve rotZ = new AnimationCurve();

        Quaternion targetWorldRotation = Quaternion.Euler(worldRotation);

        // Создаем временную позу объекта, чтобы получить его локальное вращение в заданном мировом
        GameObject tempParent = new GameObject("TempParent");
        tempParent.transform.position = Vector3.zero;
        tempParent.transform.rotation = Quaternion.identity;

        GameObject tempChild = new GameObject("TempChild");
        tempChild.transform.parent = tempParent.transform;
        tempChild.transform.localRotation = Quaternion.identity;

        // Устанавливаем целевое мировое вращение
        tempChild.transform.rotation = targetWorldRotation;

        // Получаем локальное вращение, которое нужно применить к объекту
        Quaternion localRotation = tempChild.transform.localRotation;

        Object.DestroyImmediate(tempChild);
        Object.DestroyImmediate(tempParent);

        // Проходим по каждому кадру и добавляем ключевые фреймы
        for (float time = 0; time <= duration; time += timeStep)
        {
            // Вращение будет постоянным
            rotX.AddKey(time, localRotation.eulerAngles.x);
            rotY.AddKey(time, localRotation.eulerAngles.y);
            rotZ.AddKey(time, localRotation.eulerAngles.z);
        }

        // Путь к Transform объекта в анимации
        string path = AnimationUtility.CalculateTransformPath(targetObject.transform, null);

        // Удаляем старые кривые, если они есть
        AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding
        {
            type = typeof(Transform),
            path = path,
            propertyName = "m_LocalRotation.x"
        }, null);

        AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding
        {
            type = typeof(Transform),
            path = path,
            propertyName = "m_LocalRotation.y"
        }, null);

        AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding
        {
            type = typeof(Transform),
            path = path,
            propertyName = "m_LocalRotation.z"
        }, null);

        // Устанавливаем новые кривые
        AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding
        {
            type = typeof(Transform),
            path = path,
            propertyName = "m_LocalRotation.x"
        }, rotX);

        AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding
        {
            type = typeof(Transform),
            path = path,
            propertyName = "m_LocalRotation.y"
        }, rotY);

        AnimationUtility.SetEditorCurve(animationClip, new EditorCurveBinding
        {
            type = typeof(Transform),
            path = path,
            propertyName = "m_LocalRotation.z"
        }, rotZ);

        Debug.Log($"Applied rotation animation to {targetObject.name} in clip {animationClip.name}");
    }
}