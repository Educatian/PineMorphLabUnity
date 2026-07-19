using System;
using AdieLab.PineMorphLab;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AdieLab.PineMorphLab.Editor
{
    [InitializeOnLoad]
    public static class PineMorphPlayModeQa
    {
        private const string ArmedKey = "AdieLab.PineMorph.QaArmed";
        private const string QuitKey = "AdieLab.PineMorph.QaQuit";
        private const string ScenePath = "Assets/PineMorphLab/Scenes/PineMorphLab.unity";
        private static int phase;
        private static double phaseStarted;

        static PineMorphPlayModeQa()
        {
            if (SessionState.GetBool(ArmedKey, false) || SessionState.GetBool(QuitKey, false))
            {
                Arm();
            }
        }

        public static void RunFromCommandLine()
        {
            SessionState.SetBool(ArmedKey, true);
            SessionState.SetBool(QuitKey, false);
            Arm();
            if (System.IO.File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            else
            {
                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            EditorApplication.EnterPlaymode();
        }

        private static void Arm()
        {
            EditorApplication.playModeStateChanged -= OnStateChanged;
            EditorApplication.playModeStateChanged += OnStateChanged;
        }

        private static void OnStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(ArmedKey, false))
            {
                phase = 0;
                phaseStarted = EditorApplication.timeSinceStartup;
                EditorApplication.update -= Tick;
                EditorApplication.update += Tick;
            }
            else if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(QuitKey, false))
            {
                SessionState.SetBool(QuitKey, false);
                EditorApplication.Exit(0);
            }
        }

        private static void Tick()
        {
            double elapsed = EditorApplication.timeSinceStartup - phaseStarted;
            try
            {
                if (phase == 0 && elapsed > 2.5d)
                {
                    ValidateInitial();
                    ValidateHandsOnTutorial();
                    Find<Button>("UNDER-OPENS").onClick.Invoke();
                    Find<Button>("RUN TEST").onClick.Invoke();
                    phase = 1;
                    phaseStarted = EditorApplication.timeSinceStartup;
                }
                else if (phase == 1 && elapsed > 4.2d)
                {
                    ValidateResult();
                    Find<Button>("RUN TEST").onClick.Invoke();
                    phase = 2;
                    phaseStarted = EditorApplication.timeSinceStartup;
                }
                else if (phase == 2 && elapsed > 4.2d)
                {
                    PineMorphApp app = UnityEngine.Object.FindAnyObjectByType<PineMorphApp>();
                    Require(app.CompletedTrialCount == 1,
                        "Rerunning a trial must replace its record instead of duplicating it.");
                    Debug.Log("PINEMORPH_PLAYMODE_QA_OK tutorial=passed controls=passed trial=passed rerun=replace");
                    Finish();
                }
            }
            catch (Exception exception)
            {
                EditorApplication.update -= Tick;
                Debug.LogException(exception);
                SessionState.SetBool(ArmedKey, false);
                SessionState.SetBool(QuitKey, false);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateInitial()
        {
            PineMorphApp app = UnityEngine.Object.FindAnyObjectByType<PineMorphApp>();
            Require(app != null, "PineMorph app is missing.");
            Require(app.TutorialVisible, "Guided learning should open on first load.");
            Require(Find<Button>("NEXT").GetComponentInChildren<Text>().text == "EXPLORE",
                "The first tutorial action should launch hands-on viewport practice.");
            Require(Camera.main != null, "Main camera is missing.");
            Require(Find<Slider>("Active Layer Fraction").interactable,
                "Active layer slider should be adjustable.");
            Require(Find<Slider>("Stiffness Ratio Ea/Ep").interactable,
                "Stiffness ratio slider should be adjustable.");
            Require(Find<Slider>("Fiber Angle").interactable,
                "Fiber angle slider should be adjustable.");
            Text[] labels = UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None);
            Require(labels.Length >= 35, $"Expected a complete interface, found {labels.Length} labels.");
            Canvas.ForceUpdateCanvases();
            foreach (Text label in labels)
            {
                if (label.gameObject.activeInHierarchy && !string.IsNullOrWhiteSpace(label.text))
                {
                    Require(label.cachedTextGenerator.vertexCount > 0,
                        $"Text {label.name} did not generate visible geometry.");
                }
            }
        }

        private static void ValidateHandsOnTutorial()
        {
            PineMorphApp app = UnityEngine.Object.FindAnyObjectByType<PineMorphApp>();
            Button next = Find<Button>("NEXT");
            next.onClick.Invoke();
            Require(!app.TutorialVisible, "Explore should reveal the interactive viewport.");

            app.NotifyCameraRotated();
            app.NotifyCameraZoomed();
            PineMorphInspectable inspectable =
                UnityEngine.Object.FindAnyObjectByType<PineMorphInspectable>();
            Require(inspectable != null, "At least one 3D object must be inspectable.");
            app.NotifyObjectSelected(inspectable);
            Require(app.TutorialVisible, "Viewport practice should advance after all three actions.");

            next.onClick.Invoke();
            Find<Slider>("Active Layer Fraction").value += 0.05f;
            Require(app.TutorialVisible, "Thickness practice should advance after manipulation.");
            next.onClick.Invoke();
            Find<Slider>("Stiffness Ratio Ea/Ep").value += 0.10f;
            Require(app.TutorialVisible, "Stiffness practice should advance after manipulation.");
            next.onClick.Invoke();
            Find<Slider>("Fiber Angle").value += 5f;
            Require(app.TutorialVisible, "Fiber practice should advance after manipulation.");
            next.onClick.Invoke();
            Require(!app.TutorialVisible, "Start Lab should close guided learning.");
        }

        private static void ValidateResult()
        {
            PineMorphApp app = UnityEngine.Object.FindAnyObjectByType<PineMorphApp>();
            Require(app.CompletedTrialCount == 1, "Baseline trial was not recorded.");
            Text[] angleLabels = GameObject.Find("OPENING ANGLE").GetComponentsInChildren<Text>();
            Require(angleLabels.Length == 2 && angleLabels[1].text != "--",
                "Opening angle result was not rendered.");
            Require(Find<Slider>("Fiber Angle").interactable,
                "Sliders should unlock after a trial.");
            Require(GameObject.Find("NEXT TRIAL").activeInHierarchy,
                "Next Trial should be available after the guided baseline.");
        }

        private static T Find<T>(string objectName) where T : Component
        {
            T component = GameObject.Find(objectName)?.GetComponent<T>();
            Require(component != null, $"Required {typeof(T).Name} on {objectName} is missing.");
            return component;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void Finish()
        {
            EditorApplication.update -= Tick;
            SessionState.SetBool(ArmedKey, false);
            SessionState.SetBool(QuitKey, true);
            EditorApplication.ExitPlaymode();
        }
    }
}
