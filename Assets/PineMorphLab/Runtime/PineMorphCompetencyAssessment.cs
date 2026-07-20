using UnityEngine;

namespace AdieLab.PineMorphLab
{
    public readonly struct PineMorphCompetencyInput
    {
        public PineMorphCompetencyInput(
            int correctPredictions,
            int completedPredictions,
            bool finalDesignPassed,
            int revisionAttempts,
            int correctCerSelections,
            bool transferCorrect)
        {
            CorrectPredictions = Mathf.Clamp(correctPredictions, 0, Mathf.Max(1, completedPredictions));
            CompletedPredictions = Mathf.Max(1, completedPredictions);
            FinalDesignPassed = finalDesignPassed;
            RevisionAttempts = Mathf.Max(0, revisionAttempts);
            CorrectCerSelections = Mathf.Clamp(correctCerSelections, 0, 3);
            TransferCorrect = transferCorrect;
        }

        public int CorrectPredictions { get; }
        public int CompletedPredictions { get; }
        public bool FinalDesignPassed { get; }
        public int RevisionAttempts { get; }
        public int CorrectCerSelections { get; }
        public bool TransferCorrect { get; }
    }

    public readonly struct PineMorphCompetencyResult
    {
        public PineMorphCompetencyResult(
            int predictionScore,
            int optimizationScore,
            int cerScore,
            int transferScore,
            string level)
        {
            PredictionScore = predictionScore;
            OptimizationScore = optimizationScore;
            CerScore = cerScore;
            TransferScore = transferScore;
            Level = level;
        }

        public int PredictionScore { get; }
        public int OptimizationScore { get; }
        public int CerScore { get; }
        public int TransferScore { get; }
        public int TotalScore => PredictionScore + OptimizationScore + CerScore + TransferScore;
        public string Level { get; }
    }

    public static class PineMorphCompetencyAssessment
    {
        public static PineMorphCompetencyResult Evaluate(PineMorphCompetencyInput input)
        {
            int prediction = Mathf.RoundToInt(
                20f * input.CorrectPredictions / input.CompletedPredictions);
            int optimization = input.FinalDesignPassed
                ? 20 + Mathf.Min(5, input.RevisionAttempts * 5)
                : 0;
            int cer = input.CorrectCerSelections * 15;
            int transfer = input.TransferCorrect ? 10 : 0;
            int total = prediction + optimization + cer + transfer;
            string level = total >= 85 ? "MASTERY"
                : total >= 70 ? "PROFICIENT"
                : total >= 50 ? "DEVELOPING" : "BEGINNING";
            return new PineMorphCompetencyResult(prediction, optimization, cer, transfer, level);
        }
    }
}
