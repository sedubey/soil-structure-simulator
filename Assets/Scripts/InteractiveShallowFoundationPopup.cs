using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UIButton = UnityEngine.UI.Button;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public class InteractiveShallowFoundationPopup : MonoBehaviour
{
    private const string InteractiveButtonName = "Button (SF Interact)";
    private const string InteractiveNameHint = "Interactive";

    private Transform interactiveWorldButton;
    private UIButton interactiveUiButton;
    private FoundationController targetController;
    private bool uiListenerBound;
    private float nextReferenceSearchTime;

    private GameObject overlayRoot;
    private Text errorText;
    private Font defaultFont;

    private InputField degInput;
    private InputField cInput;
    private InputField yInput;
    private InputField bInput;
    private InputField qInput;
    private InputField cuInput;
    private InputField scInput;

    private readonly List<Behaviour> temporarilyDisabled = new List<Behaviour>();
    private CursorLockMode cachedCursorLockMode;
    private bool cachedCursorVisible;
    private bool hasCachedCursorState;

    // Failure indicator fields
    private Image failureIndicator;
    private float failureIndicatorShowTime;
    private const float FailureIndicatorDuration = 3f;
    private static bool lastAttemptFailed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<InteractiveShallowFoundationPopup>() != null)
        {
            return;
        }

        GameObject popupHost = new GameObject("Interactive Shallow Foundation Popup");
        DontDestroyOnLoad(popupHost);
        popupHost.AddComponent<InteractiveShallowFoundationPopup>();
    }

    public static bool TryOpenPopup()
    {
        InteractiveShallowFoundationPopup popup = FindObjectOfType<InteractiveShallowFoundationPopup>();
        if (popup == null)
        {
            return false;
        }

        popup.OpenPopup();
        return true;
    }

    public static void NotifyCalculationFailed()
    {
        InteractiveShallowFoundationPopup popup = FindObjectOfType<InteractiveShallowFoundationPopup>();
        if (popup != null)
        {
            popup.ShowFailureIndicator();
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        CreatePopupUI();
        ClosePopup();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        ClosePopup();
    }

    private void Start()
    {
        TryWireSceneReferences(true);
    }

    private void Update()
    {
        if (overlayRoot != null && overlayRoot.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideFailureIndicator();
                ClosePopup();
            }

            return;
        }

        // Handle failure indicator timing
        if (failureIndicator != null && failureIndicator.gameObject.activeSelf)
        {
            if (Time.unscaledTime >= failureIndicatorShowTime + FailureIndicatorDuration)
            {
                HideFailureIndicator();
            }
        }

        // Check for failed attempts
        if (lastAttemptFailed && failureIndicator != null && !failureIndicator.gameObject.activeSelf)
        {
            ShowFailureIndicator();
        }

        if (Time.unscaledTime >= nextReferenceSearchTime)
        {
            TryWireSceneReferences(false);
            nextReferenceSearchTime = Time.unscaledTime + 1f;
        }

        if (WasPrimaryPressThisFrame() && DidRaycastHitInteractiveButton())
        {
            HideFailureIndicator();
            OpenPopup();
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (interactiveUiButton != null && uiListenerBound)
        {
            interactiveUiButton.onClick.RemoveListener(OpenPopup);
        }

        interactiveUiButton = null;
        interactiveWorldButton = null;
        targetController = null;
        uiListenerBound = false;

        ClosePopup();
        TryWireSceneReferences(true);
    }

    private void TryWireSceneReferences(bool forceSearch)
    {
        if (targetController == null || forceSearch)
        {
            targetController = FindInteractiveController();
        }

        if (interactiveWorldButton == null || forceSearch)
        {
            interactiveWorldButton = FindInteractiveWorldButton();
        }

        if (interactiveUiButton == null || forceSearch)
        {
            interactiveUiButton = FindInteractiveUiButton();
        }

        if (interactiveUiButton != null && !uiListenerBound)
        {
            interactiveUiButton.onClick.AddListener(OpenPopup);
            uiListenerBound = true;
        }
    }

    private FoundationController FindInteractiveController()
    {
        FoundationController[] controllers = FindObjectsOfType<FoundationController>();
        for (int i = 0; i < controllers.Length; i++)
        {
            Transform root = controllers[i].transform.root;
            if (root != null && root.name.Contains(InteractiveNameHint))
            {
                return controllers[i];
            }
        }

        if (controllers.Length > 0)
        {
            return controllers[0];
        }

        return null;
    }

    private Transform FindInteractiveWorldButton()
    {
        GameObject buttonObject = GameObject.Find(InteractiveButtonName);
        if (buttonObject != null)
        {
            return buttonObject.transform;
        }

        Transform[] allTransforms = FindObjectsOfType<Transform>();
        for (int i = 0; i < allTransforms.Length; i++)
        {
            if (allTransforms[i].name.Contains("SF Interact"))
            {
                return allTransforms[i];
            }
        }

        return null;
    }

    private UIButton FindInteractiveUiButton()
    {
        UIButton[] buttons = FindObjectsOfType<UIButton>();
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].name.Contains("SF Interact"))
            {
                return buttons[i];
            }
        }

        return null;
    }

    private bool DidRaycastHitInteractiveButton()
    {
        if (interactiveWorldButton == null)
        {
            return false;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }

        if (!TryGetPointerPosition(out Vector2 pointerPosition))
        {
            return false;
        }

        Camera raycastCamera = GetRaycastCamera();
        if (raycastCamera == null)
        {
            return false;
        }

        Ray ray = raycastCamera.ScreenPointToRay(pointerPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            return false;
        }

        return IsTransformPartOfInteractiveButton(hit.transform);
    }

    private bool IsTransformPartOfInteractiveButton(Transform clickedTransform)
    {
        Transform current = clickedTransform;
        while (current != null)
        {
            if (current == interactiveWorldButton)
            {
                return true;
            }

            if (current.name == InteractiveButtonName || current.name.Contains("SF Interact"))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private Camera GetRaycastCamera()
    {
        if (Camera.main != null)
        {
            return Camera.main;
        }

        Camera[] cameras = FindObjectsOfType<Camera>();
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i].enabled)
            {
                return cameras[i];
            }
        }

        return null;
    }

    private bool WasPrimaryPressThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetMouseButtonDown(0);
#else
        return false;
#endif
    }

    private bool TryGetPointerPosition(out Vector2 pointerPosition)
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            pointerPosition = Mouse.current.position.ReadValue();
            return true;
        }

        if (Touchscreen.current != null)
        {
            pointerPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        pointerPosition = Input.mousePosition;
        return true;
#else
        pointerPosition = Vector2.zero;
        return false;
#endif
    }

    private void OpenPopup()
    {
        if (overlayRoot != null && overlayRoot.activeSelf)
        {
            return;
        }

        TryWireSceneReferences(true);
        EnsureEventSystem();

        if (targetController != null)
        {
            PopulateFieldsFromController(targetController);
        }

        SetError(string.Empty);

        if (overlayRoot != null)
        {
            overlayRoot.SetActive(true);
        }

        EnterPopupInteractionMode();
        if (degInput != null)
        {
            degInput.Select();
            degInput.ActivateInputField();
        }
    }

    private void ClosePopup()
    {
        ExitPopupInteractionMode();
        HideFailureIndicator();

        if (overlayRoot != null)
        {
            overlayRoot.SetActive(false);
        }
    }

    private void ApplyInputs()
    {
        TryWireSceneReferences(true);

        if (targetController == null)
        {
            SetError("Interactive foundation controller was not found in the scene.");
            return;
        }

        if (!TryParseInt(degInput, "deg", out int degValue))
        {
            return;
        }

        if (!TryParseDouble(cInput, "c", out double cValue))
        {
            return;
        }

        if (!TryParseDouble(yInput, "y", out double yValue))
        {
            return;
        }

        if (!TryParseDouble(bInput, "b", out double bValue))
        {
            return;
        }

        if (!TryParseDouble(qInput, "q", out double qValue))
        {
            return;
        }

        if (!TryParseDouble(cuInput, "Cu", out double cuValue))
        {
            return;
        }

        if (!TryParseDouble(scInput, "Sc", out double scValue))
        {
            return;
        }

        targetController.ApplyParametersAndRun(
            degValue,
            cValue,
            yValue,
            bValue,
            qValue,
            cuValue,
            scValue);

        ClosePopup();
    }

    private bool TryParseDouble(InputField field, string fieldName, out double value)
    {
        string raw = field != null ? field.text.Trim() : string.Empty;

        if (string.IsNullOrEmpty(raw))
        {
            value = 0;
            SetError(fieldName + " cannot be empty.");
            return false;
        }

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
            double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
        {
            return true;
        }

        SetError("Invalid " + fieldName + ". Use a numeric value.");
        return false;
    }

    private bool TryParseInt(InputField field, string fieldName, out int value)
    {
        string raw = field != null ? field.text.Trim() : string.Empty;

        if (string.IsNullOrEmpty(raw))
        {
            value = 0;
            SetError(fieldName + " cannot be empty.");
            return false;
        }

        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ||
            int.TryParse(raw, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
        {
            return true;
        }

        SetError("Invalid " + fieldName + ". Use an integer value.");
        return false;
    }

    private void SetError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
        }
    }

    private void PopulateFieldsFromController(FoundationController controller)
    {
        degInput.text = controller.deg.ToString(CultureInfo.InvariantCulture);
        cInput.text = controller.c.ToString(CultureInfo.InvariantCulture);
        yInput.text = controller.y.ToString(CultureInfo.InvariantCulture);
        bInput.text = controller.b.ToString(CultureInfo.InvariantCulture);
        qInput.text = controller.q.ToString(CultureInfo.InvariantCulture);
        cuInput.text = controller.Cu.ToString(CultureInfo.InvariantCulture);
        scInput.text = controller.Sc.ToString(CultureInfo.InvariantCulture);
    }

    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem != null)
        {
#if ENABLE_INPUT_SYSTEM
            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null &&
                eventSystem.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#else
            if (eventSystem.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
        eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
    }

    private void EnterPopupInteractionMode()
    {
        if (!hasCachedCursorState)
        {
            cachedCursorLockMode = Cursor.lockState;
            cachedCursorVisible = Cursor.visible;
            hasCachedCursorState = true;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        temporarilyDisabled.Clear();
        DisableBehaviourType<MouseMovement>();
        DisableBehaviourType<PlayerMovement>();
        DisableBehaviourType<SimplePlayerMotor>();
    }

    private void ExitPopupInteractionMode()
    {
        for (int i = 0; i < temporarilyDisabled.Count; i++)
        {
            Behaviour behaviour = temporarilyDisabled[i];
            if (behaviour != null)
            {
                behaviour.enabled = true;
            }
        }

        temporarilyDisabled.Clear();

        if (hasCachedCursorState)
        {
            Cursor.lockState = cachedCursorLockMode;
            Cursor.visible = cachedCursorVisible;
            hasCachedCursorState = false;
        }
    }

    private void DisableBehaviourType<T>() where T : Behaviour
    {
        T[] behaviours = FindObjectsOfType<T>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (!behaviours[i].enabled)
            {
                continue;
            }

            behaviours[i].enabled = false;
            temporarilyDisabled.Add(behaviours[i]);
        }
    }

    private void ShowFailureIndicator()
    {
        if (failureIndicator != null)
        {
            failureIndicator.gameObject.SetActive(true);
            failureIndicatorShowTime = Time.unscaledTime;
            lastAttemptFailed = true;
        }
    }

    private void HideFailureIndicator()
    {
        if (failureIndicator != null)
        {
            failureIndicator.gameObject.SetActive(false);
            lastAttemptFailed = false;
        }
    }

    private void CreatePopupUI()
    {
        GameObject canvasObject = new GameObject("SF Popup Canvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        // Create failure indicator before the overlay
        CreateFailureIndicator(canvas);

        overlayRoot = new GameObject("Popup Overlay");
        overlayRoot.transform.SetParent(canvasObject.transform, false);
        Image overlayImage = overlayRoot.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.65f);
        Stretch(overlayRoot.GetComponent<RectTransform>());

        GameObject panel = new GameObject("Popup Panel");
        panel.transform.SetParent(overlayRoot.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.96f, 0.96f, 0.96f, 1f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        float panelWidth = Mathf.Min(Screen.width * 0.9f, 760f);
        float panelHeight = Mathf.Min(Screen.height * 0.95f, 640f);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        panelRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup panelLayout = panel.AddComponent<VerticalLayoutGroup>();
        panelLayout.padding = new RectOffset(18, 18, 18, 18);
        panelLayout.spacing = 6f;
        panelLayout.childControlHeight = true;
        panelLayout.childControlWidth = true;
        panelLayout.childForceExpandHeight = false;
        panelLayout.childForceExpandWidth = true;

        Text title = CreateLabel(
            panel.transform,
            "Title",
            "Interactive Shallow Foundation Parameters",
            20,
            TextAnchor.MiddleLeft,
            new Color(0.12f, 0.12f, 0.12f, 1f));
        title.gameObject.AddComponent<LayoutElement>().minHeight = 30f;

        Text subtitle = CreateLabel(
            panel.transform,
            "Subtitle",
            "Enter values and press Apply to run the scenario.",
            13,
            TextAnchor.MiddleLeft,
            new Color(0.2f, 0.2f, 0.2f, 1f));
        subtitle.gameObject.AddComponent<LayoutElement>().minHeight = 22f;

        degInput = CreateFieldRow(panel.transform, "deg (friction angle)", "26");
        degInput.contentType = InputField.ContentType.IntegerNumber;
        cInput = CreateFieldRow(panel.transform, "c (cohesion)", "0");
        yInput = CreateFieldRow(panel.transform, "y (unit weight)", "19");
        bInput = CreateFieldRow(panel.transform, "b (foundation width)", "2");
        qInput = CreateFieldRow(panel.transform, "q (applied pressure)", "1000");
        cuInput = CreateFieldRow(panel.transform, "Cu (undrained shear)", "40");
        scInput = CreateFieldRow(panel.transform, "Sc (shape factor)", "1.3");

        errorText = CreateLabel(
            panel.transform,
            "Error",
            string.Empty,
            14,
            TextAnchor.MiddleLeft,
            new Color(0.75f, 0.15f, 0.15f, 1f));
        errorText.gameObject.AddComponent<LayoutElement>().minHeight = 22f;

        GameObject buttonRow = new GameObject("Button Row");
        buttonRow.transform.SetParent(panel.transform, false);
        HorizontalLayoutGroup buttonLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 12f;
        buttonLayout.childAlignment = TextAnchor.MiddleRight;
        buttonLayout.childControlHeight = false;
        buttonLayout.childControlWidth = false;
        buttonLayout.childForceExpandHeight = false;
        buttonLayout.childForceExpandWidth = false;
        buttonRow.AddComponent<LayoutElement>().minHeight = 42f;

        UIButton applyButton = CreateButton(
            buttonRow.transform,
            "Apply",
            new Color(0.12f, 0.42f, 0.84f, 1f),
            Color.white);
        applyButton.onClick.AddListener(ApplyInputs);

        UIButton cancelButton = CreateButton(
            buttonRow.transform,
            "Cancel",
            new Color(0.34f, 0.34f, 0.34f, 1f),
            Color.white);
        cancelButton.onClick.AddListener(ClosePopup);
    }

    private void CreateFailureIndicator(Canvas canvas)
    {
        GameObject indicatorObject = new GameObject("Failure Indicator");
        indicatorObject.transform.SetParent(canvas.transform, false);

        failureIndicator = indicatorObject.AddComponent<Image>();
        failureIndicator.color = new Color(0.9f, 0.2f, 0.2f, 0.9f);

        RectTransform indicatorRect = indicatorObject.GetComponent<RectTransform>();
        indicatorRect.anchorMin = new Vector2(1f, 1f);
        indicatorRect.anchorMax = new Vector2(1f, 1f);
        indicatorRect.pivot = new Vector2(1f, 1f);
        indicatorRect.anchoredPosition = new Vector2(-20f, -20f);
        indicatorRect.sizeDelta = new Vector2(60f, 60f);

        GameObject textObject = new GameObject("Exclamation Text");
        textObject.transform.SetParent(indicatorObject.transform, false);

        Text exclamationText = textObject.AddComponent<Text>();
        exclamationText.text = "!";
        exclamationText.font = GetDefaultFont();
        exclamationText.fontSize = 48;
        exclamationText.fontStyle = FontStyle.Bold;
        exclamationText.alignment = TextAnchor.MiddleCenter;
        exclamationText.color = Color.white;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        Stretch(textRect);

        indicatorObject.SetActive(false);
    }

    private InputField CreateFieldRow(Transform parent, string labelText, string placeholder)
    {
        GameObject row = new GameObject(labelText + " Row");
        row.transform.SetParent(parent, false);

        HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 8f;
        rowLayout.childAlignment = TextAnchor.MiddleLeft;
        rowLayout.childControlHeight = true;
        rowLayout.childControlWidth = true;
        rowLayout.childForceExpandHeight = false;
        rowLayout.childForceExpandWidth = false;
        row.AddComponent<LayoutElement>().minHeight = 32f;

        Text label = CreateLabel(
            row.transform,
            "Label",
            labelText,
            13,
            TextAnchor.MiddleLeft,
            new Color(0.15f, 0.15f, 0.15f, 1f));
        LayoutElement labelLayout = label.gameObject.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 250f;
        labelLayout.minWidth = 180f;
        labelLayout.minHeight = 30f;

        InputField input = CreateInputField(row.transform, placeholder);
        LayoutElement inputLayout = input.gameObject.AddComponent<LayoutElement>();
        inputLayout.preferredWidth = 230f;
        inputLayout.flexibleWidth = 1f;
        inputLayout.minWidth = 160f;
        inputLayout.minHeight = 30f;

        return input;
    }

    private InputField CreateInputField(Transform parent, string placeholderText)
    {
        GameObject inputObject = new GameObject("Input");
        inputObject.transform.SetParent(parent, false);

        Image background = inputObject.AddComponent<Image>();
        background.color = Color.white;
        Outline outline = inputObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        outline.effectDistance = new Vector2(1f, -1f);

        InputField inputField = inputObject.AddComponent<InputField>();
        inputField.lineType = InputField.LineType.SingleLine;
        inputField.contentType = InputField.ContentType.DecimalNumber;

        Text text = CreateLabel(
            inputObject.transform,
            "Text",
            string.Empty,
            13,
            TextAnchor.MiddleLeft,
            new Color(0.12f, 0.12f, 0.12f, 1f));
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 4f);
        textRect.offsetMax = new Vector2(-8f, -4f);

        Text placeholder = CreateLabel(
            inputObject.transform,
            "Placeholder",
            placeholderText,
            13,
            TextAnchor.MiddleLeft,
            new Color(0.55f, 0.55f, 0.55f, 0.85f));
        placeholder.fontStyle = FontStyle.Italic;
        RectTransform placeholderRect = placeholder.rectTransform;
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(8f, 4f);
        placeholderRect.offsetMax = new Vector2(-8f, -4f);

        inputField.textComponent = text;
        inputField.placeholder = placeholder;

        return inputField;
    }

    private UIButton CreateButton(Transform parent, string buttonText, Color backgroundColor, Color textColor)
    {
        GameObject buttonObject = new GameObject(buttonText + " Button");
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = backgroundColor;

        UIButton button = buttonObject.AddComponent<UIButton>();
        button.targetGraphic = image;

        LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
        layout.preferredWidth = 130f;
        layout.minWidth = 120f;
        layout.minHeight = 36f;

        Text text = CreateLabel(
            buttonObject.transform,
            "Text",
            buttonText,
            14,
            TextAnchor.MiddleCenter,
            textColor);
        Stretch(text.rectTransform);

        return button;
    }

    private Text CreateLabel(
        Transform parent,
        string objectName,
        string value,
        int fontSize,
        TextAnchor alignment,
        Color color)
    {
        GameObject labelObject = new GameObject(objectName);
        labelObject.transform.SetParent(parent, false);

        Text label = labelObject.AddComponent<Text>();
        label.font = GetDefaultFont();
        label.text = value;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = color;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;

        return label;
    }

    private Font GetDefaultFont()
    {
        if (defaultFont != null)
        {
            return defaultFont;
        }

        defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null)
        {
            defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return defaultFont;
    }

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}