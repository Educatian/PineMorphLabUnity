using NUnit.Framework;

namespace AdieLab.PineMorphLab.Tests
{
    public sealed class PineMorphCompetencyAssessmentTests
    {
        [Test]
        public void Evaluate_ReturnsMastery_WhenAllEvidenceIsCorrect()
        {
            PineMorphCompetencyInput input = new PineMorphCompetencyInput(
                5, 5, true, 2, 3, true);

            PineMorphCompetencyResult result = PineMorphCompetencyAssessment.Evaluate(input);

            Assert.That(result.TotalScore, Is.EqualTo(100));
            Assert.That(result.Level, Is.EqualTo("MASTERY"));
        }

        [Test]
        public void Evaluate_NormalizesPredictionAccuracy_ByCompletedOpportunities()
        {
            PineMorphCompetencyInput input = new PineMorphCompetencyInput(
                2, 4, true, 2, 3, false);

            PineMorphCompetencyResult result = PineMorphCompetencyAssessment.Evaluate(input);

            Assert.That(result.PredictionScore, Is.EqualTo(10));
        }

        [Test]
        public void Evaluate_CombinesPredictionOptimizationAndCer()
        {
            PineMorphCompetencyInput input = new PineMorphCompetencyInput(
                3, 5, true, 1, 2, true);

            PineMorphCompetencyResult result = PineMorphCompetencyAssessment.Evaluate(input);

            Assert.That(result.PredictionScore, Is.EqualTo(12));
            Assert.That(result.OptimizationScore, Is.EqualTo(25));
            Assert.That(result.CerScore, Is.EqualTo(30));
            Assert.That(result.TransferScore, Is.EqualTo(10));
        }
    }
}
