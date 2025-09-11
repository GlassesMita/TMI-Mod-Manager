using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class ScreenTextReader : MonoBehaviour
{
    // 全局开关（静态，便于其他组件检查）
    public static bool Enabled { get; private set; } = false;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    // 使用反射创建 COM 对象 SAPI.SpVoice，以避免对 Interop 引用的编译时依赖
    private static object spVoice;
    private static object chosenVoiceToken;
    private const int SVSFlagsAsync = 1;
    private const int SVSFPurgeBeforeSpeak = 2;
#endif
    // 缓存平台是否支持 TTS（避免重复尝试）
    private static bool speechSupported = true;

    // 启用/禁用功能（即时生效）
    public static void SetEnabled(bool en)
    {
        Enabled = en;
    }

    // 朗读文本（线程同步调用）
    public static void Speak(string text)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(text)) return;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        try
        {
            // 处理文本中的特殊字符（主要转义双引号和换行）
            string processedText = text.Replace("\"", "'").Replace(Environment.NewLine, " ");
            // 构建VBScript命令
            string vbScript = $"CreateObject(\"\"SAPI.SpVoice\"\").Speak(\"\"{processedText}\"\")";
            // 构建mshta命令
            string command = $"mshta vbscript:Execute(\"{vbScript}\")(window.close)";

            // 执行命令
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{command}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            });
        }
        catch (Exception ex)
        {
            Debug.LogWarning("ScreenTextReader Speak error: " + ex.Message);
            Debug.Log("ScreenTextReader (fallback): " + text);
        }
#else
        Debug.Log("ScreenTextReader would speak: " + text);
#endif
    }

    // 检查当前是否支持系统 TTS
    public static bool IsSpeechSupported()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return speechSupported;
#else
        return false;
#endif
    }

    // 可选：显式释放（如果需要）
    public static void DisposeSynthesizer()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (spVoice != null)
        {
            try { spVoice.GetType().InvokeMember("Speak", System.Reflection.BindingFlags.InvokeMethod, null, spVoice, new object[] { string.Empty, SVSFPurgeBeforeSpeak }); } catch { }
            try { spVoice = null; } catch { }
            chosenVoiceToken = null;
        }
#endif
    }

    // 额外控制方法：暂停、继续、停止
    public static void Pause()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    try { if (spVoice != null) spVoice.GetType().InvokeMember("Pause", System.Reflection.BindingFlags.InvokeMethod, null, spVoice, null); } catch { }
#endif
    }

    public static void Resume()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    try { if (spVoice != null) spVoice.GetType().InvokeMember("Resume", System.Reflection.BindingFlags.InvokeMethod, null, spVoice, null); } catch { }
#endif
    }

    public static void StopPlaying()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    try { if (spVoice != null) spVoice.GetType().InvokeMember("Speak", System.Reflection.BindingFlags.InvokeMethod, null, spVoice, new object[] { string.Empty, SVSFPurgeBeforeSpeak }); } catch { }
#endif
    }
}