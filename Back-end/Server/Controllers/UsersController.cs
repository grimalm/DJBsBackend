using Back_end.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using static System.Reflection.Metadata.BlobBuilder;
using Back_end.Client.Services;

namespace Back_end.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly SqliteConnection _sqliteConnection;

        public UsersController(ILogger<UsersController> logger, SqliteConnection sqlConnection)
        {
            _logger = logger;
            _sqliteConnection = sqlConnection;
        }

        // Get all users
        [HttpGet]
        public IEnumerable<User> Get()
        {
            List<User> users = new();

            using (_sqliteConnection)
            {
                _sqliteConnection.Open();
                SqliteCommand command = _sqliteConnection.CreateCommand();
                command.CommandText = @"SELECT * FROM Users;";
                using (SqliteDataReader SQReader = command.ExecuteReader())
                {
                    while (SQReader.Read())
                    {
                        int userID = SQReader.GetInt32(0);
                        string username = SQReader.GetString(1);
                        string password = SQReader.GetString(2);
                        double handicap = SQReader.GetDouble(3);

                        User temp = new(userID, username, password, handicap);
                        users.Add(temp);
                    }
                }
            }

            return users;
        }

        // Create a new user
        [HttpPost("createuser")]
        public void CreateUser([FromBody] UserData userData)
        {
            using (_sqliteConnection)
            {
                _sqliteConnection.Open();
                SqliteCommand command = _sqliteConnection.CreateCommand();
                command.CommandText = @"INSERT INTO Users (UserName, Password) VALUES (@username, @password);";
                command.Parameters.AddWithValue("@username", userData.UserName);
                command.Parameters.AddWithValue("@password", userData.Password);
                command.ExecuteNonQuery();
            }
        }

        // Check login credentials
        [HttpPost("checklogin")]
        public ActionResult CheckLogin([FromBody] UserData userData)
        {
            using (_sqliteConnection)
            {
                _sqliteConnection.Open();
                SqliteCommand command = _sqliteConnection.CreateCommand();
                command.CommandText = @"SELECT UserId, Password FROM Users WHERE UserName = @username;";
                command.Parameters.AddWithValue("@username", userData.UserName);

                using (SqliteDataReader SQReader = command.ExecuteReader())
                {
                    if (SQReader.Read())
                    {
                        int userId = SQReader.GetInt32(0);
                        string password = SQReader.GetString(1);

                        return Ok(userData.Password == password ? userId : -1);
                    }
                    return Ok(-1);
                }
            }
        }

        [HttpPost("checkUsernameTaken")]
        public ActionResult CheckUsernameTaken([FromBody] string username)
        {
            using (_sqliteConnection)
            {
                _sqliteConnection.Open();
                SqliteCommand command = _sqliteConnection.CreateCommand();
                command.CommandText = @"SELECT UserId FROM Users WHERE UserName = @username;";
                command.Parameters.AddWithValue("@username", username);

                using (SqliteDataReader SQReader = command.ExecuteReader())
                {
                    return Ok(SQReader.Read());
                }
            }
        }

        [HttpPost("setclubs")]
        public IActionResult SetClubs([FromBody] ClubData clubData)
        {
            using (_sqliteConnection)
            {
                _sqliteConnection.Open();
                using (var transaction = _sqliteConnection.BeginTransaction()) 
                {
                    try
                    {
                        var updateCommand = _sqliteConnection.CreateCommand();
                        updateCommand.CommandText = "UPDATE Users SET Handicap = @handicap WHERE UserId = @userId;";
                        updateCommand.Parameters.AddWithValue("@handicap", clubData.Handicap);
                        updateCommand.Parameters.AddWithValue("@userId", clubData.UserId);
                        updateCommand.ExecuteNonQuery();

                        var deleteClubsCommand = _sqliteConnection.CreateCommand();
                        deleteClubsCommand.CommandText = "DELETE FROM UserGolfClubs WHERE UserId = @userId;";
                        deleteClubsCommand.Parameters.AddWithValue("@userId", clubData.UserId);
                        deleteClubsCommand.ExecuteNonQuery();

                        foreach (var club in clubData.Clubs.Where(c => c.IsSelected))
                        {
                            var insertClubCommand = _sqliteConnection.CreateCommand();
                            insertClubCommand.CommandText = @"
                                INSERT INTO UserGolfClubs (UserId, ClubName, AverageDistance)
                                VALUES (@userId, @clubName, @averageDistance);";
                            insertClubCommand.Parameters.AddWithValue("@userId", clubData.UserId);
                            insertClubCommand.Parameters.AddWithValue("@clubName", club.Name);
                            insertClubCommand.Parameters.AddWithValue("@averageDistance", club.AverageDistance);
                            insertClubCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback(); 
                        return StatusCode(500, "Error updating club data.");
                    }
                }
            }

            return Ok(new { Message = "Handicap and club data updated successfully." });
        }

        // Retrieve selected clubs and their average distances for a specific user
        [HttpGet("retrieveclubs")]
        public ActionResult<List<GolfClub>> GetClubs(int userId)
        {
            Console.WriteLine("start");
            var clubs = new List<GolfClub>();

            using (_sqliteConnection)
            {
                _sqliteConnection.Open();

                var command = _sqliteConnection.CreateCommand();
                command.CommandText = @"
                    SELECT ClubName, AverageDistance
                    FROM UserGolfClubs
                    WHERE UserId = @userId;";
                command.Parameters.AddWithValue("@userId", userId);

                Console.WriteLine("middle");


                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var clubName = reader.GetString(0);
                        var averageDistance = reader.GetInt32(1);

                        Console.WriteLine("middle2");

                        clubs.Add(new GolfClub { Name = clubName, IsSelected = true, AverageDistance = averageDistance });
                    }
                }
            }
            Console.WriteLine("end");

            return Ok(clubs);

        }

       
        [HttpGet("gethandicap")]
        public ActionResult<double> GetSurveyData(int userId)
        {
            double handicap;

            using (_sqliteConnection)
            {
                _sqliteConnection.Open();

                var command = _sqliteConnection.CreateCommand();
                command.CommandText = @"
                    SELECT Handicap
                    FROM Users
                    WHERE UserId = @userId;";
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        handicap = reader.GetInt32(0);
                    }
                    else
                    {
                        handicap = 99;
                    }
                }

            }

            return Ok(handicap);
        }


        [HttpPost("submitScorecard")]
        public IActionResult SubmitScorecard([FromBody] List<Hole> holes)
        {
            if (holes == null || !holes.Any())
            {
                return BadRequest("No holes provided.");
            }

            using (_sqliteConnection)
            {
                _sqliteConnection.Open();
                using (var transaction = _sqliteConnection.BeginTransaction()) 
                {
                    try
                    {
                        var command = _sqliteConnection.CreateCommand();
                        command.CommandText = @"
                            INSERT INTO Games (UserId, Date, SessionNumber)
                            VALUES (@userId, DATE('now'), 
                                (SELECT IFNULL(MAX(SessionNumber), 0) + 1 
                                FROM Games 
                                WHERE UserId = @userId AND Date = DATE('now')));
                            SELECT last_insert_rowid();
                        ";
                        command.Parameters.AddWithValue("@userId", holes.First().UserId);
                        int gameId = Convert.ToInt32(command.ExecuteScalar());

                        foreach (var hole in holes.Where(h => h.Score > 0))
                        {
                            var scoreCommand = _sqliteConnection.CreateCommand();
                            scoreCommand.CommandText = @"
                                INSERT INTO UserScores (GameId, HoleNumber, Score)
                                VALUES (@gameId, @holeNumber, @score);
                            ";
                            scoreCommand.Parameters.AddWithValue("@gameId", gameId);
                            scoreCommand.Parameters.AddWithValue("@holeNumber", hole.HoleNumber);
                            scoreCommand.Parameters.AddWithValue("@score", hole.Score);
                            scoreCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback(); 
                        return StatusCode(500, "Error submitting scorecard.");
                    }
                }
            }

            return Ok(new { Message = "Scorecard submitted successfully." });
        }

        [HttpGet("getScorecard")]
        public ActionResult<List<Hole>> GetScorecardByDateAndSession(int userId, string date, int sessionNumber)
        {
            var holes = new List<Hole>();
            using (_sqliteConnection)
            {
                _sqliteConnection.Open();
                var command = _sqliteConnection.CreateCommand();
                command.CommandText = @"
                    SELECT HoleNumber, Score
                    FROM UserScores
                    INNER JOIN Games ON UserScores.GameId = Games.GameId
                    WHERE UserId = @userId AND Date = @date AND SessionNumber = @sessionNumber;
                ";
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@date", date);
                command.Parameters.AddWithValue("@sessionNumber", sessionNumber);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        holes.Add(new Hole
                        {
                            HoleNumber = reader.GetInt32(0),
                            Score = reader.GetInt32(1)
                        });
                    }
                }
            }

            Console.WriteLine("end");

            return Ok(holes);
        }
    }
}
