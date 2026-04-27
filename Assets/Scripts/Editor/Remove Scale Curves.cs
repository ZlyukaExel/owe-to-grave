using UnityEngine;
using UnityEditor;
using System.Linq;

public class DeleteScaleCurves : EditorWindow
{
    [MenuItem("Tools/Delete Scale Curves")]
    static void Init()
    {
        foreach (var clip in Selection.GetFiltered<AnimationClip>(SelectionMode.Assets))
        {
            var curves = AnimationUtility.GetCurveBindings(clip);
            foreach (var curve in curves)
            {
                string boneName = curve.path.Split('/').Last();
                string property = curve.propertyName;
                if (property.StartsWith("m_LocalPosition") && boneName.Equals("mixamorig:Hips"))
                {
                    EditorCurveBinding binding = curve;
                    AnimationCurve originalCurve = AnimationUtility.GetEditorCurve(clip, binding);

                    if (originalCurve != null)
                    {
                        Keyframe[] keys = originalCurve.keys;
                        for (int i = 0; i < keys.Length; i++)
                        {
                            keys[i] = new Keyframe(keys[i].time, keys[i].value * 100f, keys[i].inTangent, keys[i].outTangent);
                        }

                        AnimationCurve newCurve = new AnimationCurve(keys);
                        newCurve.preWrapMode = originalCurve.preWrapMode;
                        newCurve.postWrapMode = originalCurve.postWrapMode;

                        AnimationUtility.SetEditorCurve(clip, binding, newCurve);
                    }
                }
                else if (property.StartsWith("m_LocalRotation") && boneName.Equals("") ||
                property.StartsWith("m_LocalPosition") || property.StartsWith("m_LocalScale"))
                {
                    AnimationUtility.SetEditorCurve(clip, curve, null);
                }
            }

            var objectReferenceCurves = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            foreach (var curve in objectReferenceCurves)
            {
                AnimationUtility.SetObjectReferenceCurve(clip, curve, null);
            }
        }
    }
}