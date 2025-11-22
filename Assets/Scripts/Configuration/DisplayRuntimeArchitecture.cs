using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering; // 用于鼠标事件接口

public class DisplayRuntimeArchitecture : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Text architectureText;
    // 可配置鼠标移入时的Alpha值（0-1之间）
    [Range(0f, 1f)] public float hoverAlpha = 1f;
    [Tooltip("淡入/淡出持续时间（秒）")]
    public float fadeDuration = 0.2f;

    [Tooltip("启用平滑淡入淡出；关闭则立即切换 alpha")]
    public bool enableFade = true;

    // 当前运行的淡入/淡出协程引用
    private Coroutine fadeCoroutine;

    public byte defaultAlpha = 200;

    public byte hoverAlphaValue = 255;

    void Start()
    {
#if UNITY_EDITOR
        architectureText.text = "Editor Env";
#else
        if (Environment.Is64BitProcess)
            architectureText.text = "x64 Build";
        else
            architectureText.text = "x86 Build";
#endif

        // 初始状态设置为几乎完全透明（5/255）
        SetTextAlpha(defaultAlpha);
    }

    // 鼠标移入时调用
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (enableFade)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeAlpha(255, fadeDuration));
        }
        else
        {
            if (fadeCoroutine != null) { StopCoroutine(fadeCoroutine); fadeCoroutine = null; }
            SetTextAlpha(hoverAlphaValue);
        }
    }

    // 鼠标移出时调用
    public void OnPointerExit(PointerEventData eventData)
    {
        if (enableFade)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeAlpha(5, fadeDuration));
        }
        else
        {
            if (fadeCoroutine != null) { StopCoroutine(fadeCoroutine); fadeCoroutine = null; }
            SetTextAlpha(defaultAlpha);
        }
    }

    // 封装设置Alpha的方法
    private void SetTextAlpha(byte alpha)
    {
        if (architectureText != null)
        {
            Color32 current = architectureText.color;
            current.a = alpha;
            architectureText.color = current;
        }
    }

    private System.Collections.IEnumerator FadeAlpha(byte targetAlpha, float duration)
    {
        if (architectureText == null)
            yield break;

        Color32 start = architectureText.color;
        byte startA = start.a;
        if (duration <= 0f)
        {
            SetTextAlpha(targetAlpha);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Lerp between startA and targetAlpha
            byte curA = (byte)Mathf.RoundToInt(Mathf.Lerp(startA, targetAlpha, t));
            SetTextAlpha(curA);
            yield return null;
        }

        SetTextAlpha(targetAlpha);
        fadeCoroutine = null;
    }
}