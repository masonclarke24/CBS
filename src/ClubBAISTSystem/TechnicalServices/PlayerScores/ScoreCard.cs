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
        public double Differential => Math.Round((Score - Rating) * 113.0 / Slope, 1);
        public DateTime Date { get; }
        public int Score { get { return HoleByHoleScore.Sum(); } }
        public string Email { get; }
        public List<int> HoleByHoleScore { get; internal set; }

        public ScoreCard(string course, double rating, double slope, DateTime date,
            string email, List<int> holeByHoleScore)
        {
            Course = course;
            Rating = rating;
            Slope = slope;
            Date = date;            
            Email = email;
            HoleByHoleScore = holeByHoleScore;
        }
    }
}