using Domain;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TechnicalServices;
using TechnicalServices.Memberships;
using TechnicalServices.PlayerScores;
using Xunit;

namespace CBSAutomatedTests
{
    public class PlayerScoresTests: IDisposable
    {
        private readonly List<ScoreCard> scoreCards;
        private readonly PlayerScores playerScores;
        private readonly string connectonString;

        public PlayerScoresTests()
        {
            connectonString = "Server=(localdb)\\mssqllocaldb;Database=CBS;Integrated Security=True;";
            playerScores = new PlayerScores(connectonString, "shareholder1@test.com");
            scoreCards = new List<ScoreCard>()
            {
                new ScoreCard("Club BAIST", 70.6, 128, DateTime.Now, "shareholder1@test.com", new List<int>()
                {
                    5,7,6,7,4,5,6,3,4,5,7,6,4,5,7,5,6,4
                }
                )
            };
        }

        [Fact]
        public void TestScoreFoundWhenRecorded()
        {
            bool confirmation = playerScores.RecordScores(scoreCards.FirstOrDefault(), out string message);
            Assert.True(confirmation);
            string commandText = $"SELECT SUM(Score) FROM ScoreDetails WHERE Date = '{scoreCards.FirstOrDefault().Date.ToString("yyyy-MM-dd HH:mm")}' AND " +
                $"UserId = (SELECT TOP 1 UserId FROM AspNetUsers WHERE Email = '{scoreCards.FirstOrDefault().Email}')";

            using(SqlConnection connection = new SqlConnection(connectonString))
            {
                using(SqlCommand command = new SqlCommand(commandText, connection) { CommandType = System.Data.CommandType.Text })
                {
                    connection.Open();
                    Assert.Equal(scoreCards.FirstOrDefault().Score.ToString(), command.ExecuteScalar().ToString());
                }
            }
        }

        [Fact]
        public void TestCorrectHandicapReportLocated()
        {
            HandicapReport foundHandicapReport = playerScores.GetHandicapReport(new DateTime(2020,02,01));
            Assert.NotNull(foundHandicapReport);
            Assert.Equal("Shareholder Member",foundHandicapReport.MemberName);
            Assert.Equal(2, foundHandicapReport.LastUpdated.Month);
            Assert.Equal(2020, foundHandicapReport.LastUpdated.Year);
            Assert.Equal(5.9, foundHandicapReport.HandicapFactor); 
            Assert.Equal(12.9, foundHandicapReport.Average);
            Assert.Equal(6.20, foundHandicapReport.BestOfTenAverage);
        }

        [Fact]
        public void TestGetAllHandicapReportsNotEmpty()
        {
            var allHandicapReports = playerScores.GetAllHandicapReports();
            Assert.NotNull(allHandicapReports);
            Assert.Contains(allHandicapReports, hr => hr.MemberName == "Shareholder Member" && hr.LastUpdated.Date == new DateTime(2020, 02, 18));
        }
        public void Dispose()
        {
            StringBuilder stringBuilder = new StringBuilder("ALTER TABLE ScoreDetails DROP CONSTRAINT FK_ScoreDetails_DateId " +
                "ALTER TABLE ScoreDetails ADD CONSTRAINT FK_ScoreDetails_DateId FOREIGN KEY (Date,UserId) REFERENCES ScoreCard(Date,UserId) ON DELETE CASCADE " +
                "DELETE ScoreCard WHERE Date IN(");
            foreach(var scoreCard in scoreCards)
            {
                stringBuilder.Append($"'{scoreCard.Date.ToString("yyyy-MM-dd HH:mm")}'");
            }
            stringBuilder.Append(") AND UserId IN(SELECT UserId FROM AspNetUsers WHERE Email IN(");
            foreach (var scoreCard in scoreCards)
            {
                stringBuilder.Append($"'{scoreCard.Email}'");
            }
            stringBuilder.Append(")) ALTER TABLE ScoreDetails DROP CONSTRAINT FK_ScoreDetails_DateId " +
                "ALTER TABLE ScoreDetails ADD CONSTRAINT FK_ScoreDetails_DateId FOREIGN KEY (Date,UserId) REFERENCES ScoreCard(Date,UserId)");

            using(SqlConnection connection = new SqlConnection(connectonString))
            {
                using(SqlCommand command = new SqlCommand(stringBuilder.ToString(), connection) { CommandType = System.Data.CommandType.Text })
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
