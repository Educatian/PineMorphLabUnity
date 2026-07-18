using NUnit.Framework;

namespace AdieLab.PineMorphLab.Tests
{
    public sealed class PineMorphPhysicsTests
    {
        [Test]
        public void ZeroHumidityChangeProducesNoMorphing()
        {
            PineMorphInput input = PineMorphInput.Baseline;
            input.HumidityChange = 0f;
            PineMorphResult result = PineMorphPhysics.Evaluate(input);
            Assert.That(result.OpeningAngleDeg, Is.EqualTo(0f).Within(0.001f));
            Assert.That(result.PeakStressMPa, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void FiberRotationIncreasesHygroscopicOpening()
        {
            PineMorphInput aligned = PineMorphInput.Baseline;
            PineMorphInput transverse = aligned;
            transverse.FiberAngleDeg = 90f;
            Assert.That(PineMorphPhysics.Evaluate(transverse).OpeningAngleDeg,
                Is.GreaterThan(PineMorphPhysics.Evaluate(aligned).OpeningAngleDeg));
        }

        [Test]
        public void ThickerActiveLayerRespondsMoreSlowly()
        {
            PineMorphInput thin = PineMorphInput.Baseline;
            thin.ActiveLayerFraction = 0.25f;
            PineMorphInput thick = thin;
            thick.ActiveLayerFraction = 0.75f;
            Assert.That(PineMorphPhysics.Evaluate(thick).ResponseTimeSeconds,
                Is.GreaterThan(PineMorphPhysics.Evaluate(thin).ResponseTimeSeconds));
        }

        [Test]
        public void ComparableLayerStiffnessProducesFiniteCurvature()
        {
            PineMorphInput input = PineMorphInput.Baseline;
            input.FiberAngleDeg = 35f;
            PineMorphResult result = PineMorphPhysics.Evaluate(input);
            Assert.That(result.OpeningAngleDeg, Is.InRange(45f, 55f));
            Assert.That(result.CurvaturePerMeter, Is.Not.NaN);
        }

        [Test]
        public void BalancedDesignPassesAllInstructionalConstraints()
        {
            PineMorphInput input = PineMorphInput.Baseline;
            input.FiberAngleDeg = 40f;
            PineMorphResult result = PineMorphPhysics.Evaluate(input);
            Assert.That(result.PassesAllConstraints, Is.True);
        }

        [Test]
        public void OverRotatedFibersExceedAngleConstraint()
        {
            PineMorphInput input = PineMorphInput.Baseline;
            input.FiberAngleDeg = 75f;
            PineMorphResult result = PineMorphPhysics.Evaluate(input);
            Assert.That(result.AngleSafe, Is.False);
            Assert.That(result.OpeningAngleDeg, Is.GreaterThan(PineMorphPhysics.MaximumOpeningDeg));
        }

        [Test]
        public void ThickActiveLayerExceedsResponseConstraint()
        {
            PineMorphInput input = PineMorphInput.Baseline;
            input.ActiveLayerFraction = 0.75f;
            PineMorphResult result = PineMorphPhysics.Evaluate(input);
            Assert.That(result.ResponseSafe, Is.False);
        }

        [Test]
        public void LowStiffnessRatioCanExceedStressConstraint()
        {
            PineMorphInput input = PineMorphInput.Baseline;
            input.StiffnessRatio = 0.3f;
            input.FiberAngleDeg = 65f;
            PineMorphResult result = PineMorphPhysics.Evaluate(input);
            Assert.That(result.StressSafe, Is.False);
        }

        [Test]
        public void InputIsClampedToInstructionalDomain()
        {
            PineMorphInput input = new PineMorphInput
            {
                ActiveLayerFraction = -10f,
                StiffnessRatio = 100f,
                FiberAngleDeg = 180f,
                HumidityChange = 2f
            };
            PineMorphInput clamped = PineMorphPhysics.Clamp(input);
            Assert.That(clamped.ActiveLayerFraction, Is.EqualTo(0.2f));
            Assert.That(clamped.StiffnessRatio, Is.EqualTo(5f));
            Assert.That(clamped.FiberAngleDeg, Is.EqualTo(90f));
            Assert.That(clamped.HumidityChange, Is.EqualTo(1f));
        }

        [Test]
        public void ResultFlagsMatchPublishedInstructionalLimits()
        {
            PineMorphInput input = PineMorphInput.Baseline;
            input.FiberAngleDeg = 40f;
            PineMorphResult result = PineMorphPhysics.Evaluate(input);
            Assert.That(result.AngleSafe,
                Is.EqualTo(result.OpeningAngleDeg >= PineMorphPhysics.MinimumOpeningDeg
                    && result.OpeningAngleDeg <= PineMorphPhysics.MaximumOpeningDeg));
            Assert.That(result.ResponseSafe,
                Is.EqualTo(result.ResponseTimeSeconds <= PineMorphPhysics.MaximumResponseSeconds));
            Assert.That(result.StressSafe,
                Is.EqualTo(result.PeakStressMPa <= PineMorphPhysics.MaximumStressMPa));
        }
    }
}
