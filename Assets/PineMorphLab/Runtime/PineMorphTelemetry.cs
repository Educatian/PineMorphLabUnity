using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AdieLab.PineMorphLab
{
    public static class PineMorphTelemetry
    {
        private const int OpportunitiesAvailable = 5;
        private static readonly string SessionId = Guid.NewGuid().ToString("N");

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void PineMorphEmitEvent(string json);
#endif

        public static string CurrentSessionId => SessionId;

        public static void Record(BioDesignLearningEvent evidence)
        {
            evidence.appId = "PineMorphLab";
            evidence.sessionId = SessionId;
            evidence.timestampUtc = DateTime.UtcNow.ToString("O");
            evidence.opportunitiesAvailable = OpportunitiesAvailable;
            evidence.normalizedOpportunityProgress =
                evidence.opportunitiesCompleted / (float)OpportunitiesAvailable;
            string json = JsonUtility.ToJson(evidence);
#if UNITY_WEBGL && !UNITY_EDITOR
            PineMorphEmitEvent(json);
#endif
            try
            {
                File.AppendAllText(
                    Path.Combine(Application.persistentDataPath, "pinemorph_evidence.jsonl"),
                    json + Environment.NewLine);
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"PineMorph evidence log unavailable: {exception.Message}");
            }
        }

        public static void RecordAction(string eventName, int trial, string detail)
        {
            Record(new BioDesignLearningEvent
            {
                eventName = eventName,
                opportunityIndex = trial,
                detail = detail
            });
        }

        public static void RecordInputChange(
            int trial, string inputName, float value, string designState, int revisionAttempt)
        {
            Record(new BioDesignLearningEvent
            {
                eventName = "input_changed",
                opportunityIndex = trial,
                inputName = inputName,
                inputValue = value,
                finalDesign = designState,
                revisionAttempt = revisionAttempt
            });
        }

        public static void RecordPrediction(int trial, string prediction, int confidence)
        {
            Record(new BioDesignLearningEvent
            {
                eventName = "prediction_selected",
                opportunityIndex = trial,
                prediction = prediction,
                confidence = Mathf.Clamp(confidence, 0, 100)
            });
        }

        public static void RecordTrial(
            int trial, int completed, PineMorphInput input, PineMorphResult result,
            string prediction, string outcome, int revisionAttempt)
        {
            Record(new BioDesignLearningEvent
            {
                eventName = "trial_completed",
                opportunityIndex = trial,
                opportunitiesCompleted = completed,
                prediction = prediction,
                result = outcome,
                constraintFlags =
                    $"angleSafe={result.AngleSafe};responseSafe={result.ResponseSafe};stressSafe={result.StressSafe}",
                revisionAttempt = revisionAttempt,
                finalDesign = DesignState(input)
            });
        }

        public static void RecordFinalDesign(
            PineMorphInput input, PineMorphResult result, int revisionAttempt)
        {
            Record(new BioDesignLearningEvent
            {
                eventName = "final_design_submitted",
                opportunityIndex = OpportunitiesAvailable,
                opportunitiesCompleted = OpportunitiesAvailable,
                result = result.PassesAllConstraints ? "BALANCED" : "CONSTRAINT_FAILURE",
                constraintFlags =
                    $"angleSafe={result.AngleSafe};responseSafe={result.ResponseSafe};stressSafe={result.StressSafe}",
                revisionAttempt = revisionAttempt,
                isFinalDesign = true,
                finalDesign = DesignState(input)
            });
        }

        public static void RecordAssessment(PineMorphCompetencyResult result)
        {
            Record(new BioDesignLearningEvent
            {
                eventName = "competency_assessed",
                opportunityIndex = OpportunitiesAvailable,
                opportunitiesCompleted = OpportunitiesAvailable,
                result = result.Level,
                competencyScore = result.TotalScore
            });
        }

        private static string DesignState(PineMorphInput input)
        {
            return $"activeLayerFraction={input.ActiveLayerFraction:0.000};"
                + $"stiffnessRatio={input.StiffnessRatio:0.000};fiberAngleDeg={input.FiberAngleDeg:0.0}";
        }
    }
}
