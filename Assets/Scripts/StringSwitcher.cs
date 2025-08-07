using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StringSwitcher : MonoBehaviour
{
    public Text targetText; // 目标 Text 组件
    public string[] strings; // 字符串数组
    public float switchInterval = 5.0f; // 切换间隔时间
    public float fadeDuration = 0.5f; // 淡入淡出持续时间

    private int currentIndex = 0; // 当前字符串索引

    void Start()
    {
        if (strings.Length > 0)
        {
            targetText.text = strings[currentIndex];
            StartCoroutine(SwitchStrings());
        }
    }

    private IEnumerator SwitchStrings()
    {
        while (true)
        {
            yield return StartCoroutine(FadeOut());
            currentIndex = (currentIndex + 1) % strings.Length;
            targetText.text = strings[currentIndex];
            yield return StartCoroutine(FadeIn());
            yield return new WaitForSeconds(switchInterval);
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = targetText.color;
        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            targetText.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        color.a = 1;
        targetText.color = color;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color color = targetText.color;
        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            targetText.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        color.a = 0;
        targetText.color = color;
    }
}
