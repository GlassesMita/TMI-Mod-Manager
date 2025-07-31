using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIElementFadeController : MonoBehaviour
{
    public CanvasGroup targetCanvasGroup; // 目标组件的 CanvasGroup
    public float fadeDuration = 0.25f; // 淡入淡出持续时间
    // Start is called before the first frame update
    void Start()
    {
        targetCanvasGroup.alpha = 0;
        targetCanvasGroup.interactable = false;
        targetCanvasGroup.blocksRaycasts = false;
    }

    public void ActivateComponent()
    {
        StartCoroutine(FadeIn());
    }

    public void DeactivateComponent()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            targetCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        targetCanvasGroup.alpha = 1;
        targetCanvasGroup.interactable = true;
        targetCanvasGroup.blocksRaycasts = true;
    }

    // 淡出效果
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            targetCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        targetCanvasGroup.alpha = 0;
        targetCanvasGroup.interactable = false;
        targetCanvasGroup.blocksRaycasts = false;
    }
}
