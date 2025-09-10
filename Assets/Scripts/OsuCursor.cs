using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OsuCursor : MonoBehaviour
{
    public Sprite cursorSprite;
    public Sprite cursorAdditiveSprite;
    // 基准高度（像素），默认 40，可由配置面板的 Slider 在运行时修改到 60
    public float baseHeight = 40f;

    private RectTransform cursorRect;
    private Image cursorImage;
    private RectTransform additiveRect;
    private Image additiveImage;
    private Canvas canvas;
    private CanvasGroup cursorGroup;
    private bool isDragging = false;
    private Vector2 dragStartPos;
    private float rotateDegrees = 0f;
    private float dragThreshold = 30f;
    private bool pointerVisible = true;
    private Coroutine scaleCoroutine;
    private Coroutine additiveAlphaCoroutine;
    private Vector2 cursorSize = new Vector2(30, 30);

    void Start()
    {
        // 创建 Canvas
    GameObject canvasObj = new GameObject("OsuCursorCanvas");
    canvas = canvasObj.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    // 强制覆盖排序，确保指针总在最上层（比 Dropdown 的弹出项更高）
    canvas.overrideSorting = true;
    canvas.sortingOrder = 32767;
    // 设置到常用 UI 层
    canvas.sortingLayerName = "UI";
    canvasObj.AddComponent<CanvasScaler>();
    canvasObj.AddComponent<GraphicRaycaster>();

        // 主指针
        GameObject cursorObj = new GameObject("OsuCursor");
    cursorObj.transform.SetParent(canvas.transform);
    // 确保 cursorObj 在画布的最后一个子对象（渲染优先）
    cursorObj.transform.SetAsLastSibling();
        cursorImage = cursorObj.AddComponent<Image>();
        cursorImage.sprite = cursorSprite;
        cursorImage.raycastTarget = false;
        cursorRect = cursorObj.GetComponent<RectTransform>();
        cursorGroup = cursorObj.AddComponent<CanvasGroup>();

    // 动态设置原始比例，并根据 baseHeight 等比缩放
    UpdateSizes();
    // 指针左上角为焦点（修正：Unity 默认 pivot.y=0 为下，pivot.y=1 为上）
    cursorRect.pivot = new Vector2(0, 1);

        // Additive 指针
        GameObject additiveObj = new GameObject("OsuCursorAdditive");
        additiveObj.transform.SetParent(cursorObj.transform);
        additiveImage = additiveObj.AddComponent<Image>();
        additiveImage.sprite = cursorAdditiveSprite;
        additiveImage.raycastTarget = false;
        additiveRect = additiveObj.GetComponent<RectTransform>();
        additiveRect.sizeDelta = cursorSize;
        additiveRect.anchoredPosition = Vector2.zero;
        additiveImage.color = new Color(1, 1, 1, 0);

    // 编辑器模式下显示系统指针，便于调试
    #if UNITY_EDITOR
    Cursor.visible = true;
    pointerVisible = false;
    #else
    Cursor.visible = false;
    pointerVisible = true;
    #endif

    // 监听窗口焦点
    Application.focusChanged += OnAppFocusChanged;
    }

    // 公共方法：设置基准高度（像素），并更新指针尺寸
    public void SetBaseHeight(float height)
    {
        baseHeight = Mathf.Clamp(height, 1f, 1000f);
        UpdateSizes();
    }

    // 根据 baseHeight 和精灵原始宽高比计算实际显示尺寸，并应用到主指针和 additive
    private void UpdateSizes()
    {
        if (cursorSprite != null)
        {
            float aspect = (float)cursorSprite.rect.width / cursorSprite.rect.height;
            // baseHeight 为 height，按 aspect 计算宽度，保持等比
            if (aspect >= 1f)
                cursorSize = new Vector2(baseHeight, baseHeight / aspect);
            else
                cursorSize = new Vector2(baseHeight * aspect, baseHeight);
        }
        else
        {
            cursorSize = new Vector2(baseHeight, baseHeight);
        }

        if (cursorRect != null)
        {
            cursorRect.sizeDelta = cursorSize;
            // 保证 pivot 保持左上角
            cursorRect.pivot = new Vector2(0, 1);
        }
        if (additiveRect != null)
        {
            additiveRect.sizeDelta = cursorSize;
            additiveRect.anchoredPosition = Vector2.zero;
        }
    }

    void Update()
    {
    Vector2 mousePos = Input.mousePosition;
    // 使左上角对齐鼠标
    cursorRect.position = mousePos;

        // 鼠标进入/离开窗口时动态隐藏/显示
        if (!Application.isFocused)
        {
            SetCursorVisible(false);
            return;
        }
        else if (!pointerVisible)
        {
            SetCursorVisible(true);
        }

        // 按下鼠标左键
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragStartPos = mousePos;
            rotateDegrees = 0f;
            StartScaleAnim(0.9f, 0.37f); // 0.12+0.25
            StartAdditiveAlphaAnim(1f, 0.15f);
        }
        // 松开鼠标左键
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            StartScaleAnim(1f, 0.43f, true); // 0.18+0.25
            StartAdditiveAlphaAnim(0f, 0.18f);
            StartCoroutine(RotateBackEaseOut(0.25f));
        }
        // 拖拽旋转
        if (isDragging)
        {
            Vector2 delta = mousePos - dragStartPos;
            if (delta.sqrMagnitude > dragThreshold * dragThreshold)
            {
                // 参考 JS，旋转中心为指针中心，旋转角度有偏移
                float degrees = Mathf.Atan2(-delta.x, delta.y) * Mathf.Rad2Deg + 24.3f;
                // 渐变插值旋转，动画更平滑
                rotateDegrees = Mathf.LerpAngle(rotateDegrees, degrees, Time.deltaTime * 16f);
                cursorRect.rotation = Quaternion.Euler(0, 0, rotateDegrees);
            }
        }
    }

    void SetCursorVisible(bool visible)
    {
        if (pointerVisible == visible) return;
        pointerVisible = visible;
        cursorGroup.alpha = visible ? 1f : 0f;
        Cursor.visible = !visible;
    }

    void OnAppFocusChanged(bool hasFocus)
    {
        SetCursorVisible(hasFocus);
    }

    void OnDestroy()
    {
    #if UNITY_EDITOR
    Cursor.visible = true;
    #else
    Cursor.visible = true;
    #endif
    Application.focusChanged -= OnAppFocusChanged;
    if (canvas != null) Destroy(canvas.gameObject);
    }

    // 平滑缩放动画
    void StartScaleAnim(float target, float duration, bool elastic = false)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleAnim(target, duration, elastic));
    }
    IEnumerator ScaleAnim(float target, float duration, bool elastic)
    {
        float t = 0f;
        Vector3 start = cursorRect.localScale;
        Vector3 end = Vector3.one * target;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            if (elastic)
                p = Mathf.Sin(-13f * (p + 1) * Mathf.PI / 2) * Mathf.Pow(2, -10 * p) + 1; // 弹性
            cursorRect.localScale = Vector3.LerpUnclamped(start, end, p);
            yield return null;
        }
        cursorRect.localScale = end;
    }

    // Additive 透明度动画
    void StartAdditiveAlphaAnim(float target, float duration)
    {
        if (additiveAlphaCoroutine != null) StopCoroutine(additiveAlphaCoroutine);
        additiveAlphaCoroutine = StartCoroutine(AdditiveAlphaAnim(target, duration));
    }
    IEnumerator AdditiveAlphaAnim(float target, float duration)
    {
        float t = 0f;
        float start = additiveImage.color.a;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            float a = Mathf.Lerp(start, target, p);
            additiveImage.color = new Color(1, 1, 1, a);
            yield return null;
        }
        additiveImage.color = new Color(1, 1, 1, target);
    }

    // 回弹旋转动画（ease out，速度逐渐减小到0）
    IEnumerator RotateBackEaseOut(float duration)
    {
        float t = 0f;
        float start = rotateDegrees;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            // easeOutCubic: 1-(1-p)^3
            float ease = 1f - Mathf.Pow(1f - p, 3f);
            float deg = Mathf.LerpUnclamped(start, 0f, ease);
            cursorRect.rotation = Quaternion.Euler(0, 0, deg);
            yield return null;
        }
        cursorRect.rotation = Quaternion.identity;
        rotateDegrees = 0f;
    }
}