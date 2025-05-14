using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(DevMenu))]
public class DevMenuEditor : Editor
{
    private ReorderableList reorderableList;

    private void OnEnable()
    {
        // 获取 DevMenu 脚本中的 customShortcuts 数组
        var devMenu = (DevMenu)target;

        // 创建 ReorderableList 并绑定到 customShortcuts 数组
        reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("customShortcuts"), true, true, true, true);
        reorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Custom Shortcuts");
        };

        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 如果不是在游戏模式下，则不显示快捷键设置
        if (Application.isPlaying)
        {
            // 在游戏模式下显示快捷键设置
            reorderableList.DoLayoutList();
        }
        else
        {
            // 非游戏模式下，可能只显示一些基本信息或者只显示脚本
            GUILayout.Label("DevMenu is only active in Play Mode.");
        }

        serializedObject.ApplyModifiedProperties();

        // 绘制默认的 Inspector 元素
        DrawDefaultInspector();
    }
}
