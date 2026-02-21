using UnityEditor;
using UnityEngine;

public class ApplyAnimationRotation : MonoBehaviour
{
    [MenuItem("Tools/Apply First Frame Rotation")]
    static void ApplyRotationFromFirstFrame()
    {
        GameObject go = Selection.activeGameObject;
        if (!go)
        {
            Debug.LogWarning("No GameObject selected.");
            return;
        }

        Animator animator = go.GetComponent<Animator>();
        if (!animator || !animator.runtimeAnimatorController)
        {
            Debug.LogWarning("Selected GameObject must have an Animator with a Controller.");
            return;
        }

        // Получаем первый клип из контроллера (можно улучшить — выбрать конкретный)
        AnimationClip[] clips = AnimationUtility.GetAnimationClips(animator.gameObject);
        if (clips.Length == 0)
        {
            Debug.LogWarning("No animation clips found on this Animator.");
            return;
        }

        AnimationClip clip = clips[0]; // Берём первый клип. Можно улучшить выбор.
        Debug.Log($"Processing clip: {clip.name}");

        // Создаём временный объект для воспроизведения анимации
        GameObject tempGo = Instantiate(go);
        Animator tempAnimator = tempGo.GetComponent<Animator>();
        AnimationClip tempClip = Instantiate(clip);

        // Удаляем Animator, чтобы не мешал
        DestroyImmediate(tempAnimator);

        // Проигрываем анимацию на 0 времени (первый кадр)
        Animation tempAnim = tempGo.GetComponent<Animation>();
        if (!tempAnim)
            tempAnim = tempGo.AddComponent<Animation>();
        tempAnim.AddClip(tempClip, "tempClip");
        tempAnim.Play("tempClip");
        tempAnim.Sample(); // Применяем состояние на time=0
        tempAnim.Stop();

        // Теперь копируем rotation всех Transform'ов из tempGo в оригинал
        CopyRotations(go.transform, tempGo.transform);

        // Уничтожаем временный объект
        DestroyImmediate(tempGo);

        Debug.Log("Applied first frame rotations from animation clip.");
    }

    static void CopyRotations(Transform original, Transform temp)
    {
        if (original == null || temp == null)
            return;

        // Копируем rotation
        original.rotation = temp.rotation;

        // Рекурсивно проходим по детям
        for (int i = 0; i < original.childCount; i++)
        {
            if (i < temp.childCount)
            {
                CopyRotations(original.GetChild(i), temp.GetChild(i));
            }
        }
    }
}
