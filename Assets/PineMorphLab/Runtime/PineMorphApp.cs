using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AdieLab.PineMorphLab
{
    public sealed class PineMorphApp : MonoBehaviour
    {
        private static readonly Color Ink = new Color(0.035f, 0.075f, 0.075f, 0.98f);
        private static readonly Color Panel = new Color(0.94f, 0.955f, 0.945f, 0.99f);
        private static readonly Color Teal = new Color(0.00f, 0.44f, 0.40f, 1f);
        private static readonly Color Amber = new Color(0.60f, 0.30f, 0.00f, 1f);
        private static readonly Color Coral = new Color(0.71f, 0.14f, 0.09f, 1f);
        private static readonly Color CoralBright = new Color(0.98f, 0.38f, 0.34f, 1f);
        private static readonly Color Muted = new Color(0.22f, 0.30f, 0.29f, 1f);
        private static readonly Color ActiveLayer = new Color(0.18f, 0.63f, 0.47f, 1f);
        private static readonly Color PassiveLayer = new Color(0.74f, 0.39f, 0.19f, 1f);

        private static class UiLayout
        {
            public const float LeftX = 20f;
            public const float LeftWidth = 350f;
            public const float CenterX = 388f;
            public const float CenterWidth = 750f;
            public const float RightX = 1154f;
            public const float RightWidth = 426f;
            public const float WorkspaceTop = 80f;
            public const float ResultsTop = 632f;
        }

        private static class UiType
        {
            public const int Section = 18;
            public const int Body = 17;
            public const int Control = 15;
            public const int Value = 16;
            public const int Meta = 13;
            public const int Stage = 18;
            public const int Viewport = 14;
            public const int MetricLabel = 12;
            public const int MetricValue = 20;
            public const int Chart = 14;
        }

        private readonly List<PineMorphInput> testedInputs = new List<PineMorphInput>();
        private readonly List<PineMorphResult> testedResults = new List<PineMorphResult>();
        private readonly List<string> predictions = new List<string>();

        private Font font;
        private PineMorphRibbon ribbon;
        private Slider thicknessSlider;
        private Slider stiffnessSlider;
        private Slider fiberSlider;
        private Text thicknessValue;
        private Text stiffnessValue;
        private Text fiberValue;
        private Text stageText;
        private Text resultTitle;
        private Text resultBody;
        private Text angleValue;
        private Text responseValue;
        private Text stressValue;
        private Text traceText;
        private Text trialHistory;
        private Button runButton;
        private Button continueButton;
        private Button exportButton;
        private Button[] predictionButtons;
        private PineMorphChart chart;
        private GameObject tutorialOverlay;
        private Text tutorialProgress;
        private Text tutorialTitle;
        private Text tutorialBody;
        private Button tutorialBack;
        private Button tutorialNext;
        private GameObject assessmentPanel;
        private Text assessmentScore;

        private int trialIndex;
        private int predictionIndex = -1;
        private int tutorialIndex;
        private readonly List<bool> predictionMatches = new List<bool>();
        private int correctPredictions;
        private int finalRevisionAttempts;
        private int assessmentSelections;
        private int correctCerSelections;
        private bool transferCorrect;
        private bool running;
        private bool applyingPreset;
        private bool compactLayout;
        private bool initialGuidanceActive = true;
        private int guidedPractice = -1;
        private bool cameraRotated;
        private bool cameraZoomed;
        private bool objectInspected;
        private PineMorphInspectable selectedInspectable;

        public int CurrentTrial => trialIndex;
        public int CompletedTrialCount => testedResults.Count;
        public bool TutorialVisible => tutorialOverlay != null && tutorialOverlay.activeSelf;
        public string SelectedObjectLabel => selectedInspectable?.Label ?? string.Empty;

        private static readonly string[] PredictionLabels =
        {
            "UNDER-OPENS", "BALANCED", "OVER-OPENS", "TOO SLOW", "OVER-STRESS"
        };

        private static readonly string[] TutorialTitles =
        {
            "Explore the biological mechanism",
            "Balance the layer thicknesses",
            "Change the stiffness ratio",
            "Rotate the reinforcing fibers",
            "Predict, test, and explain"
        };

        private static readonly string[] TutorialConcepts =
        {
            "Pine-cone scales move without muscles. Two bonded tissues swell by different amounts as humidity changes.",
            "Layer thickness changes both laminate curvature and the distance moisture must diffuse through the active layer.",
            "The modulus ratio redistributes axial force and bending moment between the bonded layers.",
            "Anisotropic fibers transform hygroscopic expansion along the actuator axis.",
            "A viable actuator must reach the target angle without responding too slowly or exceeding the stress limit."
        };

        private static readonly string[] TutorialActions =
        {
            "Drag to rotate, use the wheel to zoom, and click a layer or the pine cone.",
            "Move ACTIVE LAYER FRACTION to connect geometry with curvature and response time.",
            "Move STIFFNESS RATIO to connect load sharing with the stress prediction.",
            "Move FIBER ANGLE to connect orientation with transformed expansion.",
            "Choose an outcome before RUN TEST, then use all three metrics to explain the result."
        };

        private static readonly string[] TutorialEvidence =
        {
            "Differential free strain produces curvature through force and moment equilibrium.",
            "Response time grows with the square of active-layer thickness.",
            "Comparable layer stiffness often produces stronger curvature than extreme mismatch.",
            "Transverse expansion is larger than longitudinal expansion in this instructional material model.",
            "Target: 45-75 deg, response <= 180 s, stress <= 3.5 MPa."
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindAnyObjectByType<PineMorphApp>() == null)
            {
                new GameObject("PineMorph Lab").AddComponent<PineMorphApp>();
            }
        }

        private void Awake()
        {
            compactLayout = ShouldUseCompactLayout();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            CreateWorld();
            CreateInterface();
            ApplyTrialPreset(0);
            ShowTutorial(0);
            RecordEvent("session_started", "");
        }

        private void Update()
        {
            if (TutorialVisible && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseTutorial();
            }
            HandleViewportSelection();
        }

        private void CreateWorld()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraObject.tag = "MainCamera";
                camera = cameraObject.GetComponent<Camera>();
            }

            camera.backgroundColor = new Color(0.115f, 0.15f, 0.15f, 1f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.fieldOfView = 38f;
            camera.rect = new Rect(UiLayout.CenterX / 1600f, 268f / 900f,
                UiLayout.CenterWidth / 1600f, 432f / 900f);
            PineMorphOrbitCamera orbitCamera = camera.gameObject.AddComponent<PineMorphOrbitCamera>();
            orbitCamera.Initialize(this);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.24f, 0.28f, 0.27f);

            Light mainLight = new GameObject("Key Light", typeof(Light)).GetComponent<Light>();
            mainLight.type = LightType.Directional;
            mainLight.intensity = 1.45f;
            mainLight.color = new Color(1f, 0.94f, 0.84f);
            mainLight.transform.rotation = Quaternion.Euler(45f, -32f, 0f);
            mainLight.shadows = LightShadows.Soft;
            Light fill = new GameObject("Fill Light", typeof(Light)).GetComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.45f;
            fill.color = new Color(0.58f, 0.78f, 0.84f);
            fill.transform.rotation = Quaternion.Euler(25f, 145f, 0f);

            Light rim = new GameObject("Specimen Rim Light", typeof(Light)).GetComponent<Light>();
            rim.type = LightType.Spot;
            rim.range = 9f;
            rim.spotAngle = 54f;
            rim.intensity = 3.6f;
            rim.color = new Color(0.48f, 0.83f, 0.64f);
            rim.transform.position = new Vector3(3.2f, 4.1f, 3.8f);
            rim.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0.7f, 1.5f) - rim.transform.position);

            Material floorMaterial = MaterialFor(new Color(0.23f, 0.29f, 0.28f), 0.12f, 0.45f);
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Laboratory Plinth";
            floor.transform.position = new Vector3(0f, -0.50f, 2.1f);
            floor.transform.localScale = new Vector3(7.3f, 0.26f, 6.5f);
            floor.GetComponent<Renderer>().material = floorMaterial;

            Material frameMaterial = MaterialFor(new Color(0.07f, 0.10f, 0.10f), 0.62f, 0.42f);
            CreateWorldPanel("Chamber Backdrop", new Vector3(0f, 1.55f, 4.75f), new Vector3(6.7f, 3.5f, 0.12f), frameMaterial);
            CreateWorldPanel("Left Chamber Rail", new Vector3(-3.18f, 1.0f, 2.15f), new Vector3(0.12f, 3.0f, 5.1f), frameMaterial);
            CreateWorldPanel("Right Chamber Rail", new Vector3(3.18f, 1.0f, 2.15f), new Vector3(0.12f, 3.0f, 5.1f), frameMaterial);

            PineMorphAssetLoader.InstantiateModel("Models/HumidityStage", "Humidity Test Stage", null,
                new Vector3(0f, -0.52f, 1.95f), Quaternion.identity, new Vector3(1.04f, 1.04f, 1.04f));

            GameObject ribbonObject = new GameObject("Hygromorphic Bilayer");
            ribbonObject.transform.position = new Vector3(0f, 0.02f, -0.55f);
            ribbon = ribbonObject.AddComponent<PineMorphRibbon>();
            ribbon.Initialize(MaterialFor(ActiveLayer, 0.05f, 0.62f),
                MaterialFor(PassiveLayer, 0.02f, 0.48f));

            for (int i = -1; i <= 1; i += 2)
            {
                PineMorphAssetLoader.InstantiateModel("Models/PrecisionClamp",
                    i < 0 ? "Precision Clamp Left" : "Precision Clamp Right", null,
                    new Vector3(i * 0.96f, -0.10f, -0.48f), Quaternion.Euler(0f, i < 0 ? 0f : 180f, 0f),
                    new Vector3(0.60f, 0.60f, 0.60f));
            }

            CreatePineConeReference();
        }

        private static void CreateWorldPanel(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = name;
            panel.transform.position = position;
            panel.transform.localScale = scale;
            panel.GetComponent<Renderer>().material = material;
        }

        private void CreatePineConeReference()
        {
            GameObject cone = PineMorphAssetLoader.InstantiateModel("Models/PineConeReference",
                "Pine Cone Reference", null, new Vector3(-2.30f, -0.26f, 2.30f),
                Quaternion.Euler(0f, 18f, 0f), new Vector3(1.02f, 1.02f, 1.02f));
            BoxCollider coneCollider = cone.AddComponent<BoxCollider>();
            coneCollider.center = new Vector3(0f, 0.85f, 0f);
            coneCollider.size = new Vector3(1.35f, 2.15f, 1.35f);
            cone.AddComponent<PineMorphInspectable>().Configure(
                "PINE CONE REFERENCE",
                "Overlapping scales use bonded tissues with unequal hygroscopic strain to open and close.",
                cone.GetComponentsInChildren<Renderer>());

            GameObject section = PineMorphAssetLoader.InstantiateModel("Models/ScaleCrossSection",
                "Scale Tissue Cross Section", null, new Vector3(2.25f, -0.12f, 2.20f),
                Quaternion.Euler(0f, -18f, 0f), new Vector3(0.72f, 0.72f, 0.72f));
            BoxCollider sectionCollider = section.AddComponent<BoxCollider>();
            sectionCollider.center = new Vector3(0f, 0.18f, 0f);
            sectionCollider.size = new Vector3(2.1f, 0.75f, 0.9f);
            section.AddComponent<PineMorphInspectable>().Configure(
                "SCALE TISSUE SECTION",
                "The active layer, passive layer, cellulose bundles, and pores create directional moisture expansion.",
                section.GetComponentsInChildren<Renderer>());
        }

        private void CreateInterface()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            GameObject canvasObject = new GameObject("PineMorph Interface", typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = compactLayout
                ? new Vector2(1280f, 720f)
                : new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform root = canvasObject.GetComponent<RectTransform>();
            Image header = CreatePanel(root, "Header", 0f, 0f, 1600f, 64f, Ink);
            CreateText(header.rectTransform, "PineMorph Lab", 26, FontStyle.Bold, Color.white,
                26f, 12f, 280f, 38f, TextAnchor.MiddleLeft);
            CreateText(header.rectTransform, "HYGROMORPHIC BILAYER DESIGN STUDIO", 16,
                FontStyle.Bold, new Color(0.66f, 0.86f, 0.82f), 304f, 16f, 430f, 32f,
                TextAnchor.MiddleLeft);
            Button help = CreateButton(header.rectTransform, "?", Teal, 1538f, 11f, 40f, 40f);
            help.onClick.AddListener(() => ShowTutorial(0));

            Image left = CreatePanel(root, "Design Controls", UiLayout.LeftX, UiLayout.WorkspaceTop,
                UiLayout.LeftWidth, 800f, Panel);
            CreateText(left.rectTransform, "DESIGN INPUTS", UiType.Section, FontStyle.Bold, Teal,
                18f, 16f, 314f, 28f, TextAnchor.MiddleLeft);
            CreateText(left.rectTransform,
                "Tune the bonded layers before committing a prediction.", UiType.Body, FontStyle.Normal,
                Muted, 18f, 46f, 314f, 44f, TextAnchor.UpperLeft);

            PineMorphAssetLoader.AddHudImage(left.rectTransform, "Hud/hud_bilayer_coupon", new Rect(18f, 102f, 40f, 40f));
            thicknessSlider = CreateSlider(left.rectTransform, "Active Layer Fraction", 64f, 102f, 268f,
                0.20f, 0.80f, out thicknessValue);
            PineMorphAssetLoader.AddHudImage(left.rectTransform, "Hud/hud_precision_clamp", new Rect(18f, 190f, 40f, 40f));
            stiffnessSlider = CreateSlider(left.rectTransform, "Stiffness Ratio Ea/Ep", 64f, 190f, 268f,
                0.30f, 5f, out stiffnessValue);
            PineMorphAssetLoader.AddHudImage(left.rectTransform, "Hud/hud_scale_section", new Rect(18f, 278f, 40f, 40f));
            fiberSlider = CreateSlider(left.rectTransform, "Fiber Angle", 64f, 278f, 268f,
                0f, 90f, out fiberValue);
            thicknessSlider.onValueChanged.AddListener(_ => OnDesignChanged(0));
            stiffnessSlider.onValueChanged.AddListener(_ => OnDesignChanged(1));
            fiberSlider.onValueChanged.AddListener(_ => OnDesignChanged(2));

            CreateText(left.rectTransform, "PREDICT THE LIMITING OUTCOME", UiType.Control, FontStyle.Bold,
                Muted, 18f, 368f, 314f, 24f, TextAnchor.MiddleLeft);
            predictionButtons = new Button[PredictionLabels.Length];
            for (int i = 0; i < PredictionLabels.Length; i++)
            {
                int captured = i;
                float x = i % 2 == 0 ? 18f : 178f;
                float y = 402f + (i / 2) * 46f;
                float width = i == 4 ? 314f : 152f;
                predictionButtons[i] = CreateButton(left.rectTransform, PredictionLabels[i],
                    new Color(0.35f, 0.41f, 0.40f), x, y, width, 38f, 14);
                predictionButtons[i].onClick.AddListener(() => SelectPrediction(captured));
            }

            runButton = CreateButton(left.rectTransform, "RUN TEST", Amber, 18f, 554f, 314f, 50f, 16);
            runButton.onClick.AddListener(RunTest);
            continueButton = CreateButton(left.rectTransform, "NEXT TRIAL", Teal, 18f, 612f, 314f, 46f, 14);
            continueButton.onClick.AddListener(ContinueLearning);
            continueButton.gameObject.SetActive(false);
            exportButton = CreateButton(left.rectTransform, "EXPORT CSV", Ink, 18f, 666f, 314f, 42f, 13);
            exportButton.onClick.AddListener(ExportCsv);
            exportButton.interactable = false;
            CreateText(left.rectTransform, "MODEL 1.0  |  HUMIDITY STEP 55% RH", UiType.Meta, FontStyle.Bold,
                Muted, 18f, 734f, 314f, 24f, TextAnchor.MiddleLeft);

            Image stage = CreatePanel(root, "Learning Stage", UiLayout.CenterX, UiLayout.WorkspaceTop,
                UiLayout.CenterWidth, 72f,
                new Color(0.03f, 0.11f, 0.12f, 0.94f));
            stageText = CreateText(stage.rectTransform, string.Empty, UiType.Stage, FontStyle.Bold, Color.white,
                20f, 10f, 710f, 52f, TextAnchor.MiddleLeft);

            Image viewportLabel = CreatePanel(root, "Viewport Label", UiLayout.CenterX, 162f,
                UiLayout.CenterWidth, 38f,
                new Color(0.035f, 0.075f, 0.075f, 0.82f));
            CreateText(viewportLabel.rectTransform, "3D BILAYER  |  DRAG TO ROTATE  |  WHEEL TO ZOOM  |  R TO RESET",
                UiType.Viewport, FontStyle.Bold, new Color(0.80f, 0.90f, 0.88f), 16f, 4f, 718f, 30f,
                TextAnchor.MiddleLeft);

            Image right = CreatePanel(root, "Evidence Dashboard", UiLayout.RightX,
                UiLayout.WorkspaceTop, UiLayout.RightWidth, 536f,
                new Color(0.96f, 0.97f, 0.95f, 0.98f));
            CreateText(right.rectTransform, "MECHANICS EVIDENCE", UiType.Section, FontStyle.Bold, Teal,
                18f, 14f, 390f, 28f, TextAnchor.MiddleLeft);
            angleValue = CreateMetric(right.rectTransform, "OPENING ANGLE", "--", 18f, 52f, 118f, Teal);
            responseValue = CreateMetric(right.rectTransform, "RESPONSE t95", "--", 146f, 52f, 118f, Amber);
            stressValue = CreateMetric(right.rectTransform, "PEAK STRESS", "--", 274f, 52f, 134f, Coral);
            traceText = CreateText(right.rectTransform,
                "INPUTS\n  -> mismatch strain\n  -> force + moment balance\n  -> curvature + diffusion\n  -> three constraints",
                UiType.Value, FontStyle.Normal, new Color(0.10f, 0.18f, 0.18f), 18f, 154f, 390f, 118f,
                TextAnchor.UpperLeft);
            Text chartTitle = CreateText(right.rectTransform, "NORMALIZED CONSTRAINT MAP", UiType.Meta, FontStyle.Bold,
                Muted, 18f, 264f, 390f, 20f, TextAnchor.MiddleLeft);
            chart = new GameObject("Trial Comparison Graph", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(PineMorphChart)).GetComponent<PineMorphChart>();
            chart.transform.SetParent(right.transform, false);
            SetRect(chart.rectTransform, 18f, 286f, 390f, 170f);
            Text[] trialAxisLabels = new Text[5];
            for (int i = 0; i < trialAxisLabels.Length; i++)
            {
                trialAxisLabels[i] = CreateText(right.rectTransform, $"T{i + 1}", UiType.Chart, FontStyle.Bold, Muted,
                    29f + 85f * i, 458f, 30f, 18f, TextAnchor.MiddleCenter);
            }
            Text angleRange = CreateText(right.rectTransform, "ANGLE 0-120deg", UiType.Chart, FontStyle.Bold,
                Teal, 18f, 478f, 118f, 20f, TextAnchor.MiddleLeft);
            Text timeRange = CreateText(right.rectTransform, "TIME 0-320s", UiType.Chart, FontStyle.Bold,
                Amber, 146f, 478f, 118f, 20f, TextAnchor.MiddleLeft);
            Text stressRange = CreateText(right.rectTransform, "STRESS 0-8MPa", UiType.Chart, FontStyle.Bold,
                Coral, 274f, 478f, 134f, 20f, TextAnchor.MiddleLeft);
            Text limitText = CreateText(right.rectTransform,
                "Limits: angle 45-75 | time 180s | stress 3.5MPa", UiType.Chart, FontStyle.Normal,
                Muted, 18f, 498f, 390f, 20f, TextAnchor.MiddleLeft);

            if (compactLayout)
            {
                ConfigureCompactMetric(angleValue, 124f);
                ConfigureCompactMetric(responseValue, 124f);
                ConfigureCompactMetric(stressValue, 134f);
                SetRect(traceText.rectTransform, 18f, 184f, 390f, 88f);
                traceText.fontSize = 14;
                SetRect(chartTitle.rectTransform, 18f, 276f, 390f, 20f);
                SetRect(chart.rectTransform, 18f, 300f, 390f, 130f);
                for (int i = 0; i < trialAxisLabels.Length; i++)
                {
                    SetRect(trialAxisLabels[i].rectTransform, 29f + 85f * i, 432f, 30f, 18f);
                }
                SetRect(angleRange.rectTransform, 18f, 452f, 132f, 20f);
                SetRect(timeRange.rectTransform, 150f, 452f, 110f, 20f);
                SetRect(stressRange.rectTransform, 260f, 452f, 148f, 20f);
                angleRange.fontSize = 12;
                timeRange.fontSize = 12;
                stressRange.fontSize = 12;
                SetRect(limitText.rectTransform, 18f, 478f, 390f, 20f);
                limitText.fontSize = 12;
            }

            Image result = CreatePanel(root, "Results", UiLayout.CenterX, UiLayout.ResultsTop,
                1192f, 248f,
                new Color(0.035f, 0.075f, 0.075f, 0.97f));
            resultTitle = CreateText(result.rectTransform, "READY TO TEST", 23, FontStyle.Bold,
                Color.white, 22f, 16f, 500f, 34f, TextAnchor.MiddleLeft);
            resultBody = CreateText(result.rectTransform,
                "Commit a prediction, run the humidity step, and inspect the complete mechanics chain.",
                UiType.Body, FontStyle.Normal, new Color(0.80f, 0.88f, 0.86f), 22f, 58f, 590f, 112f,
                TextAnchor.UpperLeft);
            CreateText(result.rectTransform, "TRIAL RECORD", UiType.Control, FontStyle.Bold,
                new Color(0.60f, 0.84f, 0.80f), 650f, 16f, 500f, 24f, TextAnchor.MiddleLeft);
            trialHistory = CreateText(result.rectTransform, "No completed trials.", UiType.Control, FontStyle.Normal,
                new Color(0.86f, 0.90f, 0.88f), 650f, 46f, 520f, 170f, TextAnchor.UpperLeft);

            CreateTutorial(root);
            CreateAssessment(root);
            if (compactLayout)
            {
                ScaleLayout(root, 0.8f);
            }
        }

        private void CreateTutorial(RectTransform root)
        {
            tutorialOverlay = CreatePanel(root, "Guided Tutorial", 0f, 0f, 1600f, 900f,
                new Color(0.02f, 0.04f, 0.045f, 0.72f)).gameObject;
            Image card = CreatePanel(tutorialOverlay.GetComponent<RectTransform>(), "Tutorial Card",
                360f, 190f, 880f, 500f, new Color(0.035f, 0.075f, 0.075f, 0.99f));
            tutorialProgress = CreateText(card.rectTransform, string.Empty, 14, FontStyle.Bold,
                new Color(0.55f, 0.86f, 0.80f), 30f, 24f, 820f, 28f, TextAnchor.MiddleLeft);
            tutorialTitle = CreateText(card.rectTransform, string.Empty, 27, FontStyle.Bold,
                Color.white, 30f, 64f, 820f, 54f, TextAnchor.MiddleLeft);
            tutorialBody = CreateText(card.rectTransform, string.Empty, 18, FontStyle.Normal,
                new Color(0.84f, 0.90f, 0.89f), 30f, 132f, 820f, 246f, TextAnchor.UpperLeft);
            Button skip = CreateButton(card.rectTransform, "SKIP", Muted, 30f, 420f, 130f, 48f, 14);
            skip.onClick.AddListener(CloseTutorial);
            tutorialBack = CreateButton(card.rectTransform, "BACK", Muted, 548f, 420f, 130f, 48f, 14);
            tutorialBack.onClick.AddListener(() => ShowTutorial(tutorialIndex - 1));
            tutorialNext = CreateButton(card.rectTransform, "NEXT", Teal, 696f, 420f, 154f, 48f, 14);
            tutorialNext.onClick.AddListener(() =>
            {
                if (tutorialIndex >= TutorialTitles.Length - 1)
                {
                    CloseTutorial();
                }
                else
                {
                    BeginTutorialPractice();
                }
            });
        }

        private void CreateAssessment(RectTransform root)
        {
            assessmentPanel = CreatePanel(root, "Evidence Check", 330f, 120f, 940f, 690f,
                new Color(0.035f, 0.075f, 0.075f, 0.99f)).gameObject;
            RectTransform panel = assessmentPanel.GetComponent<RectTransform>();
            CreateText(panel, "CLAIM - EVIDENCE - REASONING CHECK", 24, FontStyle.Bold, Color.white,
                30f, 24f, 880f, 44f, TextAnchor.MiddleLeft);
            CreateText(panel, "Select the strongest response in each row.", 15, FontStyle.Normal,
                new Color(0.73f, 0.85f, 0.83f), 30f, 70f, 880f, 30f, TextAnchor.MiddleLeft);
            CreateAssessmentRow(panel, 105f, "CLAIM",
                new[] { "Maximize angle", "Balance all three limits", "Use the thickest layer" }, 1, false);
            CreateAssessmentRow(panel, 220f, "EVIDENCE",
                new[] { "Appearance only", "Angle only", "Angle + time + stress with units" }, 2, false);
            CreateAssessmentRow(panel, 335f, "REASONING",
                new[] { "Thickness is always better", "Mismatch creates curvature while diffusion and stress constrain it", "Humidity adds motor force" }, 1, false);
            CreateAssessmentRow(panel, 450f, "TRANSFER",
                new[] { "Maximize motion", "Copy the pine-cone thickness", "Balance motion + time + stress" }, 2, true);
            assessmentScore = CreateText(panel, "Complete CER and the unseen seed-pod transfer task.", 18, FontStyle.Bold,
                new Color(0.55f, 0.86f, 0.80f), 30f, 570f, 640f, 72f, TextAnchor.MiddleLeft);
            Button close = CreateButton(panel, "RETURN TO LAB", Teal, 690f, 578f, 220f, 52f, 14);
            close.onClick.AddListener(() => assessmentPanel.SetActive(false));
            assessmentPanel.SetActive(false);
        }

        private void CreateAssessmentRow(RectTransform parent, float y, string label,
            string[] options, int correctIndex, bool transferTask)
        {
            CreateText(parent, label, 14, FontStyle.Bold, new Color(0.55f, 0.86f, 0.80f),
                30f, y, 130f, 34f, TextAnchor.MiddleLeft);
            for (int i = 0; i < options.Length; i++)
            {
                int captured = i;
                Button option = CreateButton(parent, options[i], Muted, 150f + i * 250f, y,
                    235f, 82f, 13);
                option.GetComponentInChildren<Text>().resizeTextForBestFit = true;
                option.onClick.AddListener(() =>
                {
                    SelectAssessmentOption(option, captured == correctIndex, transferTask);
                });
            }
        }

        private void SelectAssessmentOption(Button selected, bool correct, bool transferTask)
        {
            Transform row = selected.transform.parent;
            float y = selected.GetComponent<RectTransform>().anchoredPosition.y;
            for (int i = 0; i < row.childCount; i++)
            {
                Button button = row.GetChild(i).GetComponent<Button>();
                if (button != null && Mathf.Abs(button.GetComponent<RectTransform>().anchoredPosition.y - y) < 1f)
                {
                    button.interactable = false;
                }
            }

            assessmentSelections++;
            if (transferTask)
            {
                transferCorrect = correct;
                PineMorphTelemetry.RecordAction(
                    "transfer_task_completed", trialIndex + 1, correct ? "correct" : "incorrect");
            }
            else if (correct)
            {
                correctCerSelections++;
            }

            selected.image.color = correct ? Teal : Coral;
            if (assessmentSelections < 4)
            {
                assessmentScore.text = $"EVIDENCE CHECK  {assessmentSelections}/4 RESPONSES";
            }
            else
            {
                PineMorphCompetencyInput input = new PineMorphCompetencyInput(
                    correctPredictions,
                    testedResults.Count,
                    testedResults.Count == 5 && testedResults[4].PassesAllConstraints,
                    finalRevisionAttempts,
                    correctCerSelections,
                    transferCorrect);
                PineMorphCompetencyResult result = PineMorphCompetencyAssessment.Evaluate(input);
                assessmentScore.text =
                    $"{result.Level}  {result.TotalScore}/100\n"
                    + $"Prediction {result.PredictionScore}/20 | Trial 5 {result.OptimizationScore}/25 | "
                    + $"CER {result.CerScore}/45 | Transfer {result.TransferScore}/10";
                PineMorphTelemetry.RecordAssessment(result);
            }

            RecordEvent("assessment_selected", correct ? "correct" : "incorrect");
        }

        private void ApplyTrialPreset(int index)
        {
            applyingPreset = true;
            PineMorphInput preset = PineMorphInput.Baseline;
            switch (index)
            {
                case 1:
                    preset.FiberAngleDeg = 35f;
                    break;
                case 2:
                    preset.ActiveLayerFraction = 0.75f;
                    preset.FiberAngleDeg = 35f;
                    break;
                case 3:
                    preset.StiffnessRatio = 0.30f;
                    preset.FiberAngleDeg = 65f;
                    break;
                case 4:
                    preset.ActiveLayerFraction = 0.75f;
                    preset.StiffnessRatio = 0.30f;
                    preset.FiberAngleDeg = 65f;
                    break;
            }

            thicknessSlider.value = preset.ActiveLayerFraction;
            stiffnessSlider.value = preset.StiffnessRatio;
            fiberSlider.value = preset.FiberAngleDeg;
            applyingPreset = false;
            predictionIndex = -1;
            UpdatePredictionButtons();
            UpdateInputLabels();
            PineMorphResult preview = PineMorphPhysics.Evaluate(CurrentInput());
            ribbon.SetMorph(CurrentInput(), preview, 0f);
            ClearMetrics();
            continueButton.gameObject.SetActive(false);
            stageText.text = TrialPrompt(index);
            resultTitle.text = "READY TO TEST";
            resultTitle.color = Color.white;
            resultBody.text = index == 4
                ? "The starting design violates multiple constraints. Change at least one variable and optimize independently."
                : "Commit a prediction before the humidity step reveals the analytical result.";
        }

        private void OnDesignChanged(int changedControl)
        {
            UpdateInputLabels();
            if (applyingPreset || running)
            {
                return;
            }

            predictionIndex = -1;
            UpdatePredictionButtons();
            PineMorphInput input = CurrentInput();
            ribbon.SetMorph(input, PineMorphPhysics.Evaluate(input), 0f);
            string inputName = changedControl == 0 ? "active_layer_fraction"
                : changedControl == 1 ? "stiffness_ratio" : "fiber_angle_deg";
            float inputValue = changedControl == 0 ? input.ActiveLayerFraction
                : changedControl == 1 ? input.StiffnessRatio : input.FiberAngleDeg;
            PineMorphTelemetry.RecordInputChange(
                trialIndex + 1,
                inputName,
                inputValue,
                $"activeLayerFraction={input.ActiveLayerFraction:0.000};stiffnessRatio={input.StiffnessRatio:0.000};fiberAngleDeg={input.FiberAngleDeg:0.0}",
                trialIndex == 4 ? finalRevisionAttempts + 1 : 0);
            ClearMetrics();
            continueButton.gameObject.SetActive(false);
            stageText.text = trialIndex == 4
                ? "5. OPTIMIZE | Design changed. Commit a new prediction and test all three constraints."
                : $"{trialIndex + 1}. PREDICT | Design changed. Commit a prediction that matches these settings.";

            if (guidedPractice > 0)
            {
                int expectedControl = guidedPractice - 1;
                if (changedControl == expectedControl)
                {
                    int completedStep = guidedPractice;
                    guidedPractice = -1;
                    SetTutorialFocus(-1);
                    RecordEvent("tutorial_practice_completed",
                        (completedStep + 1).ToString(CultureInfo.InvariantCulture));
                    ShowTutorial(completedStep + 1);
                }
                else
                {
                    stageText.text = $"GUIDED TRY {guidedPractice + 1}/5 | Move the highlighted control.";
                }
            }
        }

        private PineMorphInput CurrentInput()
        {
            return new PineMorphInput
            {
                ActiveLayerFraction = thicknessSlider.value,
                StiffnessRatio = stiffnessSlider.value,
                FiberAngleDeg = fiberSlider.value,
                HumidityChange = 0.55f
            };
        }

        private void UpdateInputLabels()
        {
            thicknessValue.text = $"{thicknessSlider.value * 100f:0}% active";
            stiffnessValue.text = $"{stiffnessSlider.value:0.00}";
            fiberValue.text = $"{fiberSlider.value:0} deg";
        }

        private void SelectPrediction(int index)
        {
            if (running)
            {
                return;
            }

            predictionIndex = index;
            UpdatePredictionButtons();
            stageText.text = $"PREDICTION COMMITTED | {PredictionLabels[index]}. Run the humidity step.";
            PineMorphTelemetry.RecordPrediction(trialIndex + 1, PredictionLabels[index], 0);
        }

        private void UpdatePredictionButtons()
        {
            for (int i = 0; i < predictionButtons.Length; i++)
            {
                predictionButtons[i].image.color = i == predictionIndex ? Teal : new Color(0.35f, 0.41f, 0.40f);
            }
        }

        private void RunTest()
        {
            if (running)
            {
                return;
            }

            if (predictionIndex < 0)
            {
                stageText.text = "PREDICTION REQUIRED | Choose the limiting outcome before testing.";
                return;
            }

            StartCoroutine(RunSequence());
        }

        private IEnumerator RunSequence()
        {
            running = true;
            SetControlsInteractable(false);
            continueButton.gameObject.SetActive(false);
            PineMorphInput input = CurrentInput();
            PineMorphResult result = PineMorphPhysics.Evaluate(input);
            float started = Time.unscaledTime;
            const float duration = 3.2f;
            while (Time.unscaledTime - started < duration)
            {
                float progress = Mathf.Clamp01((Time.unscaledTime - started) / duration);
                float smooth = Mathf.SmoothStep(0f, 1f, progress);
                ribbon.SetMorph(input, result, smooth);
                UpdateTrace(progress, input, result);
                yield return null;
            }

            ribbon.SetMorph(input, result, 1f);
            CompleteTrial(input, result);
            SetControlsInteractable(true);
            running = false;
        }

        private void UpdateTrace(float progress, PineMorphInput input, PineMorphResult result)
        {
            if (progress < 0.2f)
            {
                traceText.text = $"<b><color=#38C6B7>INPUTS</color></b>\n  RH step 55% | fiber {input.FiberAngleDeg:0} deg\n  -> mismatch strain\n  -> force + moment balance\n  -> curvature + diffusion\n  -> three constraints";
            }
            else if (progress < 0.4f)
            {
                traceText.text = $"INPUTS complete\n<b><color=#38C6B7>  -> mismatch strain {result.ActiveHygroStrain * 100f:0.00}%</color></b>\n  -> force + moment balance\n  -> curvature + diffusion\n  -> three constraints";
            }
            else if (progress < 0.6f)
            {
                traceText.text = "INPUTS complete\n  -> mismatch strain complete\n<b><color=#38C6B7>  -> force + moment balance</color></b>\n  -> curvature + diffusion\n  -> three constraints";
            }
            else if (progress < 0.8f)
            {
                traceText.text = $"INPUTS complete\n  -> mismatch strain complete\n  -> equilibrium complete\n<b><color=#38C6B7>  -> curvature {Mathf.Abs(result.CurvaturePerMeter):0.0} 1/m + diffusion</color></b>\n  -> three constraints";
            }
            else
            {
                traceText.text = "INPUTS complete\n  -> mismatch strain complete\n  -> equilibrium complete\n  -> curvature + diffusion complete\n<b><color=#38C6B7>  -> CHECK ALL THREE CONSTRAINTS</color></b>";
            }
        }

        private void CompleteTrial(PineMorphInput input, PineMorphResult result)
        {
            bool revisingCurrentTrial = testedResults.Count > trialIndex;
            if (revisingCurrentTrial)
            {
                testedInputs[trialIndex] = input;
                testedResults[trialIndex] = result;
                predictions[trialIndex] = PredictionLabels[predictionIndex];
            }
            else if (testedResults.Count == trialIndex)
            {
                testedInputs.Add(input);
                testedResults.Add(result);
                predictions.Add(PredictionLabels[predictionIndex]);
            }
            else
            {
                throw new InvalidOperationException("Trials must be completed in curriculum order.");
            }

            int statusSize = compactLayout ? 12 : 14;
            angleValue.text = $"{result.OpeningAngleDeg:0.0} deg\n<size={statusSize}>{(result.AngleSafe ? "IN RANGE" : "OUTSIDE 45-75")}</size>";
            responseValue.text = $"{result.ResponseTimeSeconds:0} s\n<size={statusSize}>{(result.ResponseSafe ? "ON TIME" : "TOO SLOW")}</size>";
            stressValue.text = $"{result.PeakStressMPa:0.00} MPa\n<size={statusSize}>{(result.StressSafe ? "BELOW LIMIT" : "OVER LIMIT")}</size>";
            string outcome = OutcomeLabel(result);
            bool predictionCorrect = PredictionLabels[predictionIndex] == outcome;
            if (predictionMatches.Count > trialIndex)
            {
                predictionMatches[trialIndex] = predictionCorrect;
            }
            else
            {
                predictionMatches.Add(predictionCorrect);
            }
            correctPredictions = 0;
            for (int index = 0; index < predictionMatches.Count; index++)
            {
                if (predictionMatches[index]) correctPredictions++;
            }
            if (trialIndex == 4 && revisingCurrentTrial)
            {
                finalRevisionAttempts++;
            }
            resultTitle.text = result.PassesAllConstraints ? "VIABLE PASSIVE ACTUATOR" : outcome;
            resultTitle.color = result.PassesAllConstraints ? new Color(0.35f, 0.90f, 0.64f) : CoralBright;
            resultBody.text = $"Prediction {(predictionCorrect ? "matched" : "did not match")} the result. "
                + ExplainResult(result);
            stageText.text = $"{trialIndex + 1}. {outcome} | Evidence captured. "
                + (trialIndex < 4 ? "Review the metrics, then continue."
                    : result.PassesAllConstraints ? "Proceed to the evidence check."
                    : "Revise the design and test again.");
            chart.SetResults(testedResults);
            RefreshTrialHistory();
            exportButton.interactable = true;
            if (trialIndex < 4 || result.PassesAllConstraints)
            {
                continueButton.gameObject.SetActive(true);
                continueButton.GetComponentInChildren<Text>().text = trialIndex == 4
                    ? "EVIDENCE CHECK" : "NEXT TRIAL";
            }
            else
            {
                continueButton.gameObject.SetActive(false);
                stageText.text = "REVISE | Trial 5 must satisfy angle, response, and stress together. Change the design and retry.";
            }

            PineMorphTelemetry.RecordTrial(
                trialIndex + 1,
                testedResults.Count,
                input,
                result,
                PredictionLabels[predictionIndex],
                outcome,
                trialIndex == 4 ? finalRevisionAttempts : 0);
            if (trialIndex == 4 && result.PassesAllConstraints)
            {
                PineMorphTelemetry.RecordFinalDesign(input, result, finalRevisionAttempts);
            }
        }

        private void ContinueLearning()
        {
            if (trialIndex >= 4)
            {
                assessmentPanel.SetActive(true);
                assessmentPanel.transform.SetAsLastSibling();
                RecordEvent("assessment_opened", "");
                return;
            }

            trialIndex++;
            ApplyTrialPreset(trialIndex);
        }

        private void SetControlsInteractable(bool value)
        {
            thicknessSlider.interactable = value;
            stiffnessSlider.interactable = value;
            fiberSlider.interactable = value;
            runButton.interactable = value;
            for (int i = 0; i < predictionButtons.Length; i++)
            {
                predictionButtons[i].interactable = value;
            }
        }

        private void ClearMetrics()
        {
            angleValue.text = "--";
            responseValue.text = "--";
            stressValue.text = "--";
            traceText.text = "INPUTS\n  -> mismatch strain\n  -> force + moment balance\n  -> curvature + diffusion\n  -> three constraints";
        }

        private void RefreshTrialHistory()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < testedResults.Count; i++)
            {
                PineMorphResult result = testedResults[i];
                builder.Append("T").Append(i + 1).Append("  ")
                    .Append(result.OpeningAngleDeg.ToString("0", CultureInfo.InvariantCulture)).Append(" deg  ")
                    .Append(result.ResponseTimeSeconds.ToString("0", CultureInfo.InvariantCulture)).Append(" s  ")
                    .Append(result.PeakStressMPa.ToString("0.00", CultureInfo.InvariantCulture)).Append(" MPa  ")
                    .Append(OutcomeLabel(result)).Append('\n');
            }

            trialHistory.text = builder.ToString();
        }

        private void ExportCsv()
        {
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("trial,prediction,outcome,active_layer_fraction,stiffness_ratio,fiber_angle_deg,humidity_change,curvature_per_m,opening_angle_deg,tip_displacement_mm,response_t95_s,peak_stress_mpa,angle_safe,response_safe,stress_safe,model_version");
            for (int i = 0; i < testedResults.Count; i++)
            {
                PineMorphInput input = testedInputs[i];
                PineMorphResult result = testedResults[i];
                csv.Append(i + 1).Append(',').Append(predictions[i]).Append(',').Append(OutcomeLabel(result)).Append(',')
                    .Append(input.ActiveLayerFraction.ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                    .Append(input.StiffnessRatio.ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                    .Append(input.FiberAngleDeg.ToString("0.0", CultureInfo.InvariantCulture)).Append(',')
                    .Append(input.HumidityChange.ToString("0.00", CultureInfo.InvariantCulture)).Append(',')
                    .Append(result.CurvaturePerMeter.ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                    .Append(result.OpeningAngleDeg.ToString("0.00", CultureInfo.InvariantCulture)).Append(',')
                    .Append(result.TipDisplacementMm.ToString("0.00", CultureInfo.InvariantCulture)).Append(',')
                    .Append(result.ResponseTimeSeconds.ToString("0.00", CultureInfo.InvariantCulture)).Append(',')
                    .Append(result.PeakStressMPa.ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                    .Append(result.AngleSafe).Append(',').Append(result.ResponseSafe).Append(',')
                    .Append(result.StressSafe).Append(',').Append(PineMorphPhysics.ModelVersion).AppendLine();
            }

            string fileName = $"pinemorph_session_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
#if UNITY_WEBGL && !UNITY_EDITOR
            PineMorphDownload(fileName, csv.ToString());
#else
            string path = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllText(path, csv.ToString());
            resultBody.text = $"CSV exported to {path}";
#endif
            RecordEvent("csv_exported", testedResults.Count.ToString(CultureInfo.InvariantCulture));
        }

        private void RecordEvent(string eventName, string detail)
        {
            int opportunityIndex = eventName == "session_started" || eventName.StartsWith("tutorial_", StringComparison.Ordinal)
                ? 0
                : trialIndex + 1;
            PineMorphTelemetry.RecordAction(eventName, opportunityIndex, detail);
        }

        private static string OutcomeLabel(PineMorphResult result)
        {
            if (!result.StressSafe) return "OVER-STRESS";
            if (!result.ResponseSafe) return "TOO SLOW";
            if (result.OpeningAngleDeg < PineMorphPhysics.MinimumOpeningDeg) return "UNDER-OPENS";
            if (result.OpeningAngleDeg > PineMorphPhysics.MaximumOpeningDeg) return "OVER-OPENS";
            return "BALANCED";
        }

        private static string ExplainResult(PineMorphResult result)
        {
            if (!result.StressSafe) return "The strain mismatch creates excessive layer stress.";
            if (!result.ResponseSafe) return "The active layer is too diffusion-limited for the response target.";
            if (!result.AngleSafe) return result.OpeningAngleDeg < PineMorphPhysics.MinimumOpeningDeg
                ? "The differential strain does not generate enough curvature."
                : "The actuator exceeds the useful opening range.";
            return "The design balances useful curvature, moisture response, and structural stress.";
        }

        private static string TrialPrompt(int index)
        {
            switch (index)
            {
                case 0: return "1. BASELINE | Predict how fibers aligned with the hinge affect opening.";
                case 1: return "2. FIBER ORIENTATION | Rotate fibers only and compare the transformed expansion strain.";
                case 2: return "3. THICKNESS TRADE-OFF | A thick active layer may morph strongly but absorb moisture slowly.";
                case 3: return "4. STIFFNESS MISMATCH | Track stress as the passive layer becomes relatively stiff.";
                default: return "5. OPTIMIZE | Change the unsafe starting design and satisfy all three constraints.";
            }
        }

        private void ShowTutorial(int index)
        {
            guidedPractice = -1;
            SetTutorialFocus(-1);
            tutorialIndex = Mathf.Clamp(index, 0, TutorialTitles.Length - 1);
            tutorialOverlay.SetActive(true);
            tutorialOverlay.transform.SetAsLastSibling();
            tutorialProgress.text = $"GUIDED LEARNING  {tutorialIndex + 1}/{TutorialTitles.Length}";
            tutorialTitle.text = TutorialTitles[tutorialIndex];
            tutorialBody.text = $"<b><color=#74D7CB>CONCEPT</color></b>  {TutorialConcepts[tutorialIndex]}\n\n"
                + $"<b><color=#74D7CB>DO</color></b>  {TutorialActions[tutorialIndex]}\n\n"
                + $"<b><color=#74D7CB>WATCH FOR</color></b>  {TutorialEvidence[tutorialIndex]}";
            tutorialBack.interactable = tutorialIndex > 0;
            tutorialNext.GetComponentInChildren<Text>().text = tutorialIndex == TutorialTitles.Length - 1
                ? "START LAB"
                : tutorialIndex == 0 ? "EXPLORE" : "TRY CONTROL";
            RecordEvent("tutorial_step", (tutorialIndex + 1).ToString(CultureInfo.InvariantCulture));
        }

        private void CloseTutorial()
        {
            guidedPractice = -1;
            SetTutorialFocus(-1);
            tutorialOverlay.SetActive(false);
            if (initialGuidanceActive)
            {
                ApplyTrialPreset(0);
                initialGuidanceActive = false;
            }
            RecordEvent("tutorial_closed", (tutorialIndex + 1).ToString(CultureInfo.InvariantCulture));
        }

        private void BeginTutorialPractice()
        {
            guidedPractice = tutorialIndex;
            tutorialOverlay.SetActive(false);
            if (guidedPractice == 0)
            {
                cameraRotated = false;
                cameraZoomed = false;
                objectInspected = false;
                stageText.text = "GUIDED TRY 1/5 | Drag to rotate, wheel to zoom, then click an object.";
            }
            else
            {
                SetTutorialFocus(guidedPractice - 1);
                stageText.text = $"GUIDED TRY {guidedPractice + 1}/5 | Move the highlighted control.";
            }
            RecordEvent("tutorial_practice_started",
                (guidedPractice + 1).ToString(CultureInfo.InvariantCulture));
        }

        private void SetTutorialFocus(int controlIndex)
        {
            if (thicknessValue == null)
            {
                return;
            }

            thicknessValue.color = controlIndex == 0 ? Amber : Teal;
            stiffnessValue.color = controlIndex == 1 ? Amber : Teal;
            fiberValue.color = controlIndex == 2 ? Amber : Teal;
        }

        public void NotifyCameraRotated()
        {
            if (!cameraRotated)
            {
                cameraRotated = true;
                RecordEvent("camera_rotated", "");
            }
            CheckViewportPractice();
        }

        public void NotifyCameraZoomed()
        {
            if (!cameraZoomed)
            {
                cameraZoomed = true;
                RecordEvent("camera_zoomed", "");
            }
            CheckViewportPractice();
        }

        public void NotifyObjectSelected(PineMorphInspectable inspectable)
        {
            if (inspectable == null)
            {
                return;
            }

            selectedInspectable?.SetSelected(false);
            selectedInspectable = inspectable;
            selectedInspectable.SetSelected(true);
            objectInspected = true;
            stageText.text = $"OBJECT: {inspectable.Label} | {inspectable.Detail}";
            RecordEvent("object_selected", inspectable.Label);
            CheckViewportPractice();
        }

        public bool SelectObjectFromRay(Ray ray)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
            if (hits.Length == 0)
            {
                return false;
            }

            Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
            PineMorphInspectable first = null;
            PineMorphInspectable alternate = null;
            for (int i = 0; i < hits.Length; i++)
            {
                PineMorphInspectable inspectable =
                    hits[i].collider.GetComponentInParent<PineMorphInspectable>();
                if (inspectable == null)
                {
                    continue;
                }

                if (first == null)
                {
                    first = inspectable;
                }
                else if (inspectable != first)
                {
                    alternate = inspectable;
                    break;
                }
            }

            if (first == null)
            {
                return false;
            }

            NotifyObjectSelected(selectedInspectable == first && alternate != null ? alternate : first);
            return true;
        }

        private void CheckViewportPractice()
        {
            if (guidedPractice != 0)
            {
                return;
            }

            if (cameraRotated && cameraZoomed && objectInspected)
            {
                guidedPractice = -1;
                RecordEvent("tutorial_practice_completed", "1");
                ShowTutorial(1);
                return;
            }

            string rotate = cameraRotated ? "done" : "drag";
            string zoom = cameraZoomed ? "done" : "wheel";
            string inspect = objectInspected ? "done" : "click";
            stageText.text = $"GUIDED TRY 1/5 | Rotate: {rotate}  Zoom: {zoom}  Inspect: {inspect}";
        }

        private void HandleViewportSelection()
        {
            Camera camera = Camera.main;
            if (camera == null || TutorialVisible || !Input.GetMouseButtonDown(0))
            {
                return;
            }

            Vector2 pointer = Input.mousePosition;
            if (!camera.pixelRect.Contains(pointer))
            {
                return;
            }

            SelectObjectFromRay(camera.ScreenPointToRay(pointer));
        }

        private Slider CreateSlider(RectTransform parent, string label, float x, float y, float width,
            float min, float max, out Text valueText)
        {
            CreateText(parent, label.ToUpperInvariant(), UiType.Control, FontStyle.Bold, Muted,
                x, y, width - 100f, 28f, TextAnchor.MiddleLeft);
            valueText = CreateText(parent, string.Empty, UiType.Value, FontStyle.Bold, Teal,
                x + width - 104f, y, 104f, 28f, TextAnchor.MiddleRight);
            GameObject sliderObject = new GameObject(label, typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            SetRect(sliderRect, x, y + 34f, width, 36f);

            Image background = CreatePanel(sliderRect, "Track", 10f, 14f, width - 20f, 8f,
                new Color(0.72f, 0.76f, 0.73f)).GetComponent<Image>();
            GameObject fillAreaObject = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaObject.transform.SetParent(sliderRect, false);
            RectTransform fillArea = fillAreaObject.GetComponent<RectTransform>();
            SetRect(fillArea, 10f, 14f, width - 20f, 8f);
            Image fill = CreatePanel(fillArea, "Fill", 0f, 0f, width - 20f, 8f, Teal).GetComponent<Image>();
            Stretch(fill.rectTransform);

            GameObject handleAreaObject = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleAreaObject.transform.SetParent(sliderRect, false);
            RectTransform handleArea = handleAreaObject.GetComponent<RectTransform>();
            SetRect(handleArea, 10f, 0f, width - 20f, 36f);
            GameObject handleObject = new GameObject("Handle", typeof(RectTransform));
            handleObject.transform.SetParent(handleArea, false);
            RectTransform handleRect = handleObject.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0f, 0.5f);
            handleRect.anchorMax = new Vector2(0f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.anchoredPosition = Vector2.zero;
            handleRect.sizeDelta = Vector2.zero;
            Image handle = CreatePanel(handleRect, "Indicator", -8f, -8f, 16f, 16f, Color.white).GetComponent<Image>();
            handle.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            handle.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            handle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            handle.rectTransform.anchoredPosition = Vector2.zero;
            handle.rectTransform.sizeDelta = new Vector2(16f, 16f);
            Slider slider = sliderObject.GetComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            background.raycastTarget = true;
            fill.raycastTarget = false;
            return slider;
        }

        private Text CreateMetric(RectTransform parent, string label, string value, float x, float y,
            float width, Color accent)
        {
            Image metric = CreatePanel(parent, label, x, y, width, 88f,
                new Color(0.06f, 0.12f, 0.12f, 1f));
            CreatePanel(metric.rectTransform, "Accent", 0f, 0f, width, 5f, accent);
            CreateText(metric.rectTransform, label, UiType.MetricLabel, FontStyle.Bold,
                new Color(0.70f, 0.80f, 0.78f), 8f, 11f, width - 16f, 20f, TextAnchor.MiddleLeft);
            return CreateText(metric.rectTransform, value, UiType.MetricValue, FontStyle.Bold, Color.white,
                8f, 33f, width - 16f, 46f, TextAnchor.UpperLeft);
        }

        private static void ConfigureCompactMetric(Text value, float width)
        {
            RectTransform metric = (RectTransform)value.rectTransform.parent;
            metric.sizeDelta = new Vector2(width, 124f);
            RectTransform accent = (RectTransform)metric.GetChild(0);
            accent.sizeDelta = new Vector2(width, accent.sizeDelta.y);
            Text label = metric.GetComponentsInChildren<Text>()[0];
            label.text = label.text.Replace(" ", "\n");
            label.fontSize = 12;
            SetRect(label.rectTransform, 8f, 8f, width - 16f, 40f);
            value.fontSize = 18;
            SetRect(value.rectTransform, 8f, 44f, width - 16f, 76f);
        }

        private Image CreatePanel(RectTransform parent, string name, float x, float y, float width,
            float height, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image image = panel.GetComponent<Image>();
            image.color = color;
            SetRect(image.rectTransform, x, y, width, height);
            return image;
        }

        private Text CreateText(RectTransform parent, string text, int size, FontStyle style, Color color,
            float x, float y, float width, float height, TextAnchor alignment)
        {
            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text label = textObject.GetComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.color = color;
            label.alignment = alignment;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.supportRichText = true;
            SetRect(label.rectTransform, x, y, width, height);
            return label;
        }

        private Button CreateButton(RectTransform parent, string label, Color color, float x, float y,
            float width, float height, int fontSize = 14)
        {
            Image image = CreatePanel(parent, label, x, y, width, height, color);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.14f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            colors.disabledColor = new Color(0.32f, 0.36f, 0.35f, 0.55f);
            button.colors = colors;
            Text text = CreateText(image.rectTransform, label, fontSize, FontStyle.Bold, Color.white,
                6f, 2f, width - 12f, height - 4f, TextAnchor.MiddleCenter);
            text.raycastTarget = false;
            return button;
        }

        private static Material MaterialFor(Color color, float metallic, float smoothness)
        {
            Shader shader = Resources.Load<Shader>("PineMorphLit")
                ?? Shader.Find("PineMorph/Lit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("UI/Default");
            if (shader == null)
            {
                throw new InvalidOperationException("No supported runtime material shader is available.");
            }

            Material material = new Material(shader);
            material.color = color;
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }
            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }
            return material;
        }

        private static void ScaleLayout(RectTransform root, float scale)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                ScaleRectTree((RectTransform)root.GetChild(i), scale);
            }
        }

        private static void ScaleRectTree(RectTransform rect, float scale)
        {
            rect.anchoredPosition *= scale;
            rect.sizeDelta *= scale;
            for (int i = 0; i < rect.childCount; i++)
            {
                ScaleRectTree((RectTransform)rect.GetChild(i), scale);
            }
        }

        private static bool ShouldUseCompactLayout()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            int cssWidth = PineMorphCanvasCssWidth();
            return cssWidth > 0 && cssWidth < 1200;
#else
            return false;
#endif
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void PineMorphDownload(string fileName, string content);

        [DllImport("__Internal")]
        private static extern void PineMorphEmitEvent(string json);

        [DllImport("__Internal")]
        private static extern int PineMorphCanvasCssWidth();
#endif
    }
}
