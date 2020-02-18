using System;
using System.Collections.Generic;
using System.Linq;

namespace TechnicalServices.PlayerScores
{
    public class ScoreCard
    {
        public string Course { get; }
        public double Rating { get; }
        public double Slope { get; }
        public DateTime Date { get; }
        public int Score { get; }
        public string Email { get; }
        public List<int> HoleByHoleScore { get;}

        public ScoreCard(string course, double rating, double slope, DateTime date,
            string email, List<int> holeByHoleScore)
        {
            Course = course;
            Rating = rating;
            Slope = slope;
            Date = date;
            Score = holeByHoleScore.Sum();
            Email = email;
            HoleByHoleScore = holeByHoleScore;
        }
    }
}