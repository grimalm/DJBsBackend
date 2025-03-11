using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Data.Sqlite;
using System.Globalization;

namespace Back_end
{//video tests
    //developer tools show local storage, network, inspect
    //show call to ai from ai interface
    //test api from postman
    public class Program
    {
        public static void Main(string[] args)
        {
            string createDatabase = @"
        CREATE TABLE IF NOT EXISTS Users (
            UserId INTEGER PRIMARY KEY,
            UserName TEXT,
            Password TEXT,
            Handicap REAL DEFAULT 99
        );

        CREATE TABLE IF NOT EXISTS UserGolfClubs (
            UserGolfClubId INTEGER PRIMARY KEY,
            UserId INTEGER,
            ClubName TEXT,
            AverageDistance INTEGER,
            FOREIGN KEY(UserId) REFERENCES Users(UserId)
        );

        CREATE TABLE IF NOT EXISTS UserScores (
            UserScoreId INTEGER PRIMARY KEY,
            GameId INTEGER,
            HoleNumber INTEGER,
            Score INTEGER,
            FOREIGN KEY(GameId) REFERENCES Games(GameId)
        );
        CREATE TABLE IF NOT EXISTS Games (
            GameId INTEGER PRIMARY KEY,
            UserId INTEGER,
            SessionNumber INTEGER,
            Date TEXT,
            FOREIGN KEY(UserId) REFERENCES Users(UserId)
        );
    ";

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            //test change
            SqliteConnection sqlConnection = new("Data Source=NEAdatabase.db");

            builder.Services.AddScoped<SqliteConnection>(_ => sqlConnection);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();


            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            using (sqlConnection)
            {
                sqlConnection.Open();
                SqliteCommand command = sqlConnection.CreateCommand();
                command.CommandText = createDatabase;

                command.ExecuteNonQuery();
            }

            using (sqlConnection)
            {
                sqlConnection.Open();
                SqliteCommand Command = sqlConnection.CreateCommand();
                Command.CommandText = @"SELECT * FROM Users;";
                using (SqliteDataReader SQReader = Command.ExecuteReader())
                {
                    while (SQReader.Read() == true)
                    {
                        int UserID = SQReader.GetInt32(0);
                        String Username = SQReader.GetString(1);
                        String Password = SQReader.GetString(2);
                        Console.WriteLine($"{UserID} , {Username} , {Password}");
                    }
                }
            }
                //Any database setup code or intial code that should be run goes here

            app.Run();
        }
    }
}