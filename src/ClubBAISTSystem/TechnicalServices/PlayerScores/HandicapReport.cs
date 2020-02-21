using System;

namespace TechnicalServices.PlayerScores
{
    public class HandicapReport
    {
        public HandicapReport(string memberName, string email, DateTime lastUpdated, double handicapFactor, double average, double bestOfTenAverage, ScoreCard[] previousRounds)
        {
            MemberName = memberName;
            Email = email;
            LastUpdated = lastUpdated;
            HandicapFactor = handicapFactor;
            Average = average;
            BestOfTenAverage = bestOfTenAverage;
            PreviousRounds = previousRounds;
        }

        public string MemberName { get; private set; }
        public DateTime LastUpdated { get; private set; }
        public double HandicapFactor { get; private set; }
        public double Average { get; private set; }
        public double BestOfTenAverage { get; private set; }
        public ScoreCard[] PreviousRounds { get; private set; }
        public string Email { get; private set; }
    }
}