using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class DamageNumberLibrary
{
    sealed class DamageNumberHost : MonoBehaviour
    {
        readonly List<ActiveDamageNumber> activeNumbers = new();
        RectTransform canvasRect;

        void Update()
        {
            float now = Time.time;
            for (int i = activeNumbers.Count - 1; i >= 0; i--)
            {
                if (!activeNumbers[i].Tick(now))
                {
                    activeNumbers[i].Dispose();
                    activeNumbers.RemoveAt(i);
                }
            }
        }

        public void Show(Camera cam, Vector3 worldPosition, float damage)
        {
            if (cam == null || damage <= 0.001f)
                return;

            Vector3 screenPosition = cam.WorldToScreenPoint(worldPosition);
            if (screenPosition.z <= 0f)
                return;

            EnsureCanvas();
            if (canvasRect == null)
                return;

            Vector2 offset = ResolveScreenOffset();
            Vector2 localPosition;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                new Vector2(screenPosition.x, screenPosition.y) + offset,
                null,
                out localPosition))
                return;

            GameObject textObj = new GameObject("DamageNumber");
            textObj.transform.SetParent(canvasRect, false);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.raycastTarget = false;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = ResolveFontSize();
            text.fontStyle = FontStyles.Bold;
            text.color = ResolveColor();
            text.text = FormatDamage(damage);

            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = localPosition;
            rect.sizeDelta = new Vector2(160f, 48f);

            activeNumbers.Add(new ActiveDamageNumber(text, localPosition, ResolveRisePixels(), ResolveDuration()));
        }

        void EnsureCanvas()
        {
            if (canvasRect != null)
                return;

            GameObject canvasObj = new GameObject("DamageNumberCanvas");
            Object.DontDestroyOnLoad(canvasObj);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
            canvasRect = canvas.GetComponent<RectTransform>();
        }
    }

    sealed class ActiveDamageNumber
    {
        readonly TextMeshProUGUI text;
        readonly RectTransform rect;
        readonly Vector2 startPosition;
        readonly float risePixels;
        readonly float startTime;
        readonly float duration;
        readonly Color startColor;

        public ActiveDamageNumber(TextMeshProUGUI text, Vector2 startPosition, float risePixels, float duration)
        {
            this.text = text;
            this.rect = text != null ? text.rectTransform : null;
            this.startPosition = startPosition;
            this.risePixels = risePixels;
            this.duration = Mathf.Max(0.05f, duration);
            this.startColor = text != null ? text.color : Color.white;
            startTime = Time.time;
        }

        public bool Tick(float now)
        {
            if (text == null || rect == null)
                return false;

            float normalized = Mathf.Clamp01((now - startTime) / duration);
            float eased = 1f - ((1f - normalized) * (1f - normalized));
            rect.anchoredPosition = startPosition + Vector2.up * (risePixels * eased);

            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, normalized);
            text.color = color;
            return normalized < 1f;
        }

        public void Dispose()
        {
            if (text != null)
                Object.Destroy(text.gameObject);
        }
    }

    static DamageNumberHost host;

    public static void ShowDamage(Vector3 worldPosition, float damage)
    {
        Camera cam = ResolveCamera();
        if (cam == null)
            return;

        EnsureHost().Show(cam, worldPosition, damage);
    }

    static DamageNumberHost EnsureHost()
    {
        if (host != null)
            return host;

        GameObject obj = new GameObject("DamageNumberLibraryHost");
        Object.DontDestroyOnLoad(obj);
        host = obj.AddComponent<DamageNumberHost>();
        return host;
    }

    static Camera ResolveCamera()
    {
        if (CommonAttackVisualUIManager.Instance != null && CommonAttackVisualUIManager.Instance.mainCamera != null)
            return CommonAttackVisualUIManager.Instance.mainCamera;

        Camera cam = Camera.main;
        if (cam != null)
            return cam;

        return Object.FindFirstObjectByType<Camera>();
    }

    static string FormatDamage(float damage)
    {
        return Mathf.Max(1, Mathf.RoundToInt(damage)).ToString();
    }

    static float ResolveFontSize()
    {
        if (CommonAttackVisualUIManager.Instance != null)
            return Mathf.Max(1f, CommonAttackVisualUIManager.Instance.damageNumberFontSize);

        return 28f;
    }

    static float ResolveDuration()
    {
        if (CommonAttackVisualUIManager.Instance != null)
            return Mathf.Max(0.05f, CommonAttackVisualUIManager.Instance.damageNumberDuration);

        return 0.85f;
    }

    static float ResolveRisePixels()
    {
        if (CommonAttackVisualUIManager.Instance != null)
            return Mathf.Max(0f, CommonAttackVisualUIManager.Instance.damageNumberRisePixels);

        return 48f;
    }

    static Vector2 ResolveScreenOffset()
    {
        if (CommonAttackVisualUIManager.Instance != null)
            return CommonAttackVisualUIManager.Instance.damageNumberScreenOffset;

        return new Vector2(0f, 24f);
    }

    static Color ResolveColor()
    {
        if (CommonAttackVisualUIManager.Instance != null)
            return CommonAttackVisualUIManager.Instance.damageNumberColor;

        return new Color(1f, 0.2f, 0.08f, 1f);
    }
}
