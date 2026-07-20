using System;

namespace AdieLab.PineMorphLab
{
    [Serializable]
    public sealed class BioDesignLearningEvent
    {
        public string schemaVersion = "bio-design-learning-event/1.0";
        public string appId;
        public string sessionId;
        public string timestampUtc;
        public string eventName;
        public int opportunityIndex;
        public int opportunitiesCompleted;
        public int opportunitiesAvailable;
        public float normalizedOpportunityProgress;
        public string inputName;
        public float inputValue;
        public string prediction;
        public int confidence;
        public string result;
        public string constraintFlags;
        public int revisionAttempt;
        public bool isFinalDesign;
        public string finalDesign;
        public int competencyScore;
        public string detail;
    }
}
