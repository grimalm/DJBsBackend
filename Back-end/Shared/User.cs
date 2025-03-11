namespace Back_end.Shared
{
    public class User : UserData
    {
        public int UserId { get; set; }
        public double Handicap { get; set; }

        public User(int userid, string username, string password, double handicap) : base(username, password)
        {
            UserId = userid;
            UserName = username;
            Password = password;
            Handicap = handicap;
        }
    }

    public class UserData
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public UserData(string username, string password)
        {
            UserName = username;
            Password = password;
        }
    }

    public class GolfClub
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; } = false;
        public int AverageDistance { get; set; } = 0;
    }

    public class ClubData
    {
        public int UserId { get; set; }
        public double Handicap { get; set; }
        public List<GolfClub> Clubs { get; set; }

        public ClubData(double handicap, List<GolfClub> clubs, int userId)
        {
            Handicap = handicap;
            Clubs = clubs;
            UserId = userId;
        }
    }

    public class Hole
    {
        public int HoleNumber { get; set; }
        public bool HasBeenPlayed { get; set; } = false;
        public int Score { get; set; } = 0;
        public int UserId { get; set; }

    }

    public class HoleData
    {
        public int UserId { get; set; }
        public List<Hole> Holes { get; set; }

        public HoleData(List<Hole> holes, int userId)
        {
            Holes = holes;
            UserId = userId;
        }
    }

    //
    public class Game
    {
        public int GameId { get; set; }
        public int UserId { get; set; }
        public int SessionNumber { get; set; }
        public string Date { get; set; }
    }

    public class UserScore
    {
        public int UserScoreId { get; set; }
        public int GameId { get; set; }
        public int HoleNumber { get; set; }
        public int Score { get; set; }
    }


}