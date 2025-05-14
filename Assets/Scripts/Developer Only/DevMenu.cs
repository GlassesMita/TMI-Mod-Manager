using UnityEngine;

public class DevMenu : MonoBehaviour
{
    [Tooltip("快捷键的定义\n右侧数量为触发使用的按键数量\n以下按键进行了合并：\n    左 Ctrl/右 Ctrl\n    左 Shift/右 Shift\n    左 Alt/右 Alt（不包含 AltGr 别名）\n    左 Windows 徽标键/右 Windows 徽标键")]
    [Header("Developer Menu Settings")]
    public KeyCode[] customShortcuts = new KeyCode[0]; // 快捷键数组

    // 用于合并按键的特殊处理（如左 Ctrl 和右 Ctrl）
    private KeyCode[] mergedShortcuts;

    // 开发人员菜单（Game Object）

    public GameObject devMenu;

    // 在 Update 中检查快捷键，仅在游戏模式下执行
    private void Update()
    {
        if (Application.isPlaying) // 仅在播放模式下进行检查
        {
            CheckShortcuts();
        }
    }

    // 检查是否按下快捷键
    private void CheckShortcuts()
    {
        foreach (var shortcut in GetMergedShortcuts())
        {
            if (Input.GetKeyDown(shortcut))
            {
                Debug.Log($"Shortcut {shortcut} Triggered!");
                // 在这里添加具体的操作
                devMenu.SetActive(true);
            }
        }
    }

    // 返回合并后的快捷键列表（如将左 Ctrl 和右 Ctrl 合并为一个）
    private KeyCode[] GetMergedShortcuts()
    {
        if (mergedShortcuts == null || mergedShortcuts.Length != customShortcuts.Length)
        {
            mergedShortcuts = new KeyCode[customShortcuts.Length];
            for (int i = 0; i < customShortcuts.Length; i++)
            {
                if (customShortcuts[i] == KeyCode.LeftControl || customShortcuts[i] == KeyCode.RightControl)
                {
                    mergedShortcuts[i] = KeyCode.LeftControl; // 合并左 Ctrl 和右 Ctrl
                }
                else if (customShortcuts[i] == KeyCode.LeftShift || customShortcuts[i] == KeyCode.RightShift)
                {
                    mergedShortcuts[i] = KeyCode.LeftShift; // 合并左 Shift 和右 Shift
                }
                else if (customShortcuts[i] == KeyCode.LeftAlt || customShortcuts[i] == KeyCode.RightAlt)
                {
                    mergedShortcuts[i] = KeyCode.LeftAlt; // 合并左 Alt 和右 Alt，但不包括 AltGr
                }
                else if (customShortcuts[i] == KeyCode.LeftWindows || customShortcuts[i] == KeyCode.RightWindows)
                {
                    mergedShortcuts[i] = KeyCode.LeftWindows; // 合并左 Windows 和右 Windows
                }
                else
                {
                    mergedShortcuts[i] = customShortcuts[i]; // 其他按键不变
                }

            }
        }
        return mergedShortcuts;
    }
}
