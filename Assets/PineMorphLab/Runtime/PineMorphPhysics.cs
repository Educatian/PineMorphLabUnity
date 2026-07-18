using System;
using UnityEngine;

namespace AdieLab.PineMorphLab
{
    [Serializable]
    public struct PineMorphInput
    {
        public float ActiveLayerFraction;
        public float StiffnessRatio;
        public float FiberAngleDeg;
        public float HumidityChange;

        public static PineMorphInput Baseline => new PineMorphInput
        {
            ActiveLayerFraction = 0.55f,
            StiffnessRatio = 1f,
            FiberAngleDeg = 0f,
            HumidityChange = 0.55f
        };
    }

    [Serializable]
    public struct PineMorphResult
    {
        public float CurvaturePerMeter;
        public float OpeningAngleDeg;
        public float TipDisplacementMm;
        public float ResponseTimeSeconds;
        public float PeakStressMPa;
        public float ActiveHygroStrain;
        public bool AngleSafe;
        public bool ResponseSafe;
        public bool StressSafe;

        public bool PassesAllConstraints => AngleSafe && ResponseSafe && StressSafe;
    }

    public static class PineMorphPhysics
    {
        public const string ModelVersion = "hygromorph-bilayer-1.0";
        public const float TotalThicknessM = 0.0016f;
        public const float HingeLengthM = 0.040f;
        public const float ActiveModulusPa = 120_000_000f;
        public const float LongitudinalHygroExpansion = 0.015f;
        public const float TransverseHygroExpansion = 0.120f;
        public const float PassiveHygroExpansion = 0.008f;
        public const float MoistureDiffusivityM2PerS = 1.5e-9f;
        public const float MinimumOpeningDeg = 45f;
        public const float MaximumOpeningDeg = 75f;
        public const float MaximumResponseSeconds = 180f;
        public const float MaximumStressMPa = 3.5f;

        public static PineMorphInput Clamp(PineMorphInput input)
        {
            input.ActiveLayerFraction = Mathf.Clamp(input.ActiveLayerFraction, 0.20f, 0.80f);
            input.StiffnessRatio = Mathf.Clamp(input.StiffnessRatio, 0.30f, 5f);
            input.FiberAngleDeg = Mathf.Clamp(input.FiberAngleDeg, 0f, 90f);
            input.HumidityChange = Mathf.Clamp01(input.HumidityChange);
            return input;
        }

        public static PineMorphResult Evaluate(PineMorphInput rawInput)
        {
            PineMorphInput input = Clamp(rawInput);
            double activeThickness = TotalThicknessM * input.ActiveLayerFraction;
            double passiveThickness = TotalThicknessM - activeThickness;
            double activeModulus = ActiveModulusPa;
            double passiveModulus = activeModulus / input.StiffnessRatio;

            double angleRadians = input.FiberAngleDeg * Math.PI / 180d;
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);
            double activeExpansion = LongitudinalHygroExpansion * cos * cos
                + TransverseHygroExpansion * sin * sin;
            double activeFreeStrain = activeExpansion * input.HumidityChange;
            double passiveFreeStrain = PassiveHygroExpansion * input.HumidityChange;

            // Laminate force and moment equilibrium at the bonded interface.
            double a = activeModulus * activeThickness + passiveModulus * passiveThickness;
            double b = (-activeModulus * activeThickness * activeThickness
                + passiveModulus * passiveThickness * passiveThickness) / 2d;
            double d = (activeModulus * Math.Pow(activeThickness, 3d)
                + passiveModulus * Math.Pow(passiveThickness, 3d)) / 3d;
            double freeForce = activeModulus * activeThickness * activeFreeStrain
                + passiveModulus * passiveThickness * passiveFreeStrain;
            double freeMoment = (-activeModulus * activeFreeStrain * activeThickness * activeThickness
                + passiveModulus * passiveFreeStrain * passiveThickness * passiveThickness) / 2d;
            double denominator = a * d - b * b;
            double interfaceStrain = (freeForce * d - b * freeMoment) / denominator;
            double curvature = (a * freeMoment - b * freeForce) / denominator;

            double openingRadians = Math.Abs(curvature) * HingeLengthM;
            double openingDegrees = openingRadians * 180d / Math.PI;
            double tipDisplacement = Math.Abs(curvature) < 1e-8d
                ? 0d
                : Math.Abs((1d - Math.Cos(openingRadians)) / curvature) * 1000d;
            double responseTime = -Math.Log(0.05d) * activeThickness * activeThickness
                / (Math.PI * Math.PI * MoistureDiffusivityM2PerS);

            double activeOuterStress = activeModulus
                * (interfaceStrain - curvature * activeThickness - activeFreeStrain);
            double activeInterfaceStress = activeModulus * (interfaceStrain - activeFreeStrain);
            double passiveInterfaceStress = passiveModulus * (interfaceStrain - passiveFreeStrain);
            double passiveOuterStress = passiveModulus
                * (interfaceStrain + curvature * passiveThickness - passiveFreeStrain);
            double peakStress = Math.Max(
                Math.Max(Math.Abs(activeOuterStress), Math.Abs(activeInterfaceStress)),
                Math.Max(Math.Abs(passiveInterfaceStress), Math.Abs(passiveOuterStress))) / 1_000_000d;

            return new PineMorphResult
            {
                CurvaturePerMeter = (float)curvature,
                OpeningAngleDeg = (float)openingDegrees,
                TipDisplacementMm = (float)tipDisplacement,
                ResponseTimeSeconds = (float)responseTime,
                PeakStressMPa = (float)peakStress,
                ActiveHygroStrain = (float)activeFreeStrain,
                AngleSafe = openingDegrees >= MinimumOpeningDeg && openingDegrees <= MaximumOpeningDeg,
                ResponseSafe = responseTime <= MaximumResponseSeconds,
                StressSafe = peakStress <= MaximumStressMPa
            };
        }
    }
}
