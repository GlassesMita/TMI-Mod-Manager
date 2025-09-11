using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

// 附加到需要支持右键朗读的 UI 元素上
public class AccessibleOnRightClick : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("如果不为空则朗读该文本，否则尝试读取 targetText 或自身子 Text 组件的内容")]
    [TextArea]
    public string overrideText;  // 确保此类中只定义一次

    [Tooltip("可选：明确指定要朗读的 Text 对象（优先于 overrideText 为空时使用）")]
    public Text targetText;  // 确保此类中只定义一次

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null) return;
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (!ScreenTextReader.Enabled) return;

        string toSpeak = null;
        if (!string.IsNullOrEmpty(overrideText))
            toSpeak = overrideText;
        else if (targetText != null)
            toSpeak = targetText.text;
        else
        {
            var txt = GetComponentInChildren<Text>();
            if (txt != null) toSpeak = txt.text;
        }

        if (!string.IsNullOrEmpty(toSpeak))
            ScreenTextReader.Speak(toSpeak);
    }
}