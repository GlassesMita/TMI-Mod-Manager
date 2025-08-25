using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Shortcut
{
    [Serializable]
    public class ShortcutEntry
    {
        [Tooltip("主快捷键")] public KeyCode key;
        [Tooltip("组合键（可选）")]
        public List<KeyCode> comboKeys = new List<KeyCode>();
        [Tooltip("指示器对象")] public GameObject indicator;
        [Tooltip("按下快捷键时触发的事件")] public UnityEvent onKeyDown;
        [Tooltip("是否需要长按")] public bool requireHold = false;
        [Tooltip("长按秒数（仅当需要长按时有效）")] public float holdSeconds = 1f;
    }

    public class ShortcutManager : MonoBehaviour
    {
        [Header("是否显示快捷键指示器")]
        public bool showShortcutIndicator = true;

        [Header("快捷键列表")]
        public List<ShortcutEntry> shortcuts = new List<ShortcutEntry>();

        [Header("长按和组合键全局设置")]
        [Tooltip("组合键是否也需要长按")] public bool comboRequireHold = false;

        // 记录每个快捷键的按下时间
        private Dictionary<ShortcutEntry, float> holdTimers = new Dictionary<ShortcutEntry, float>();

        public void SetShowShortcutIndicator(bool show)
        {
            showShortcutIndicator = show;
            foreach (var entry in shortcuts)
            {
                if (entry.indicator != null)
                    entry.indicator.SetActive(showShortcutIndicator);
            }
        }

        void Start()
        {
            SetShowShortcutIndicator(showShortcutIndicator);
        }

        void Update()
        {
            foreach (var entry in shortcuts)
            {
                if (entry.key == KeyCode.None) continue;

                bool comboPressed = true;
                if (entry.comboKeys != null && entry.comboKeys.Count > 0)
                {
                    foreach (var k in entry.comboKeys)
                    {
                        if (!Input.GetKey(k))
                        {
                            comboPressed = false;
                            break;
                        }
                    }
                }

                bool allPressed = Input.GetKey(entry.key) && comboPressed;
                bool allJustDown = Input.GetKeyDown(entry.key) && comboPressed;

                // 长按逻辑
                bool needHold = entry.requireHold || (comboRequireHold && entry.comboKeys != null && entry.comboKeys.Count > 0);
                if (needHold)
                {
                    if (allJustDown)
                    {
                        holdTimers[entry] = Time.time;
                    }
                    if (allPressed && holdTimers.ContainsKey(entry))
                    {
                        float held = Time.time - holdTimers[entry];
                        if (held >= entry.holdSeconds)
                        {
                            entry.onKeyDown?.Invoke();
                            holdTimers.Remove(entry);
                        }
                    }
                    if (!allPressed && holdTimers.ContainsKey(entry))
                    {
                        holdTimers.Remove(entry);
                    }
                }
                else
                {
                    if (allJustDown)
                    {
                        entry.onKeyDown?.Invoke();
                    }
                }
            }
        }
    }
}
