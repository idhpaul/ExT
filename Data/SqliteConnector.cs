using Dapper;
using ExT.Config;
using ExT.Data.Entities;
using Microsoft.Extensions.Configuration;
using System.Data.SQLite;

namespace ExT.Data
{
    public class SqliteConnector
    {
        private readonly BotConfig _config;
        private readonly IConfigurationRoot _secretConfig;

        public SqliteConnector(BotConfig config, IConfigurationRoot secretConfig)
        {
            Console.WriteLine("SQLiteConnection constructor called");

            _config = config;
            _secretConfig = secretConfig;

        }

        public void Initialize()
        {
            using var sqliteConnection = new SQLiteConnection(_config.botDbLocate);
            
            if (!File.Exists(_config.botDbLocate))
            {
                SQLiteConnection.CreateFile(_config.botDbName);
            }

            DbCreateChallengeTable();
            DbCreateExerciseTable();

        }

        private void DbCreateChallengeTable()
        {
            using var sqliteConnection = new SQLiteConnection(_config.botDbLocate);

            sqliteConnection.Open();

            // 테이블 생성 쿼리
            var createChallengeTable = @"
                CREATE TABLE IF NOT EXISTS  Challenge (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    title TEXT,
                    channel_id INTEGER,
                    leader_name TEXT,
                    leader_id INTEGER
                    );
                ";

            // Dapper를 이용해 테이블 생성 쿼리 실행
            sqliteConnection.Execute(createChallengeTable);

        }

        private void DbCreateExerciseTable()
        {
            using var sqliteConnection = new SQLiteConnection(_config.botDbLocate);

            sqliteConnection.Open();

            // 테이블 생성 쿼리
            var createExerciseTable = @"
                CREATE TABLE IF NOT EXISTS Exercise (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    exercise_time TEXT,
                    calories_burned TEXT,
                    other_data TEXT
                    );
                ";
            // Dapper를 이용해 테이블 생성 쿼리 실행
            sqliteConnection.Execute(createExerciseTable);
        }

        public void DbInsertChallenge(ChallengeEntity challenge)
        {
            using var sqliteConnection = new SQLiteConnection(_config.botDbLocate);

            var sql = "INSERT INTO Challenge (title, channel_id, leader_name, leader_id) VALUES (@Title, @ChannelId, @LeaderName, @LeaderId)";
            {
                var rowsAffected = sqliteConnection.Execute(sql, challenge);
                Console.WriteLine($"{rowsAffected} row(s) inserted.");
            }
        }

        public void DbUpdateChallenge(ChallengeEntity challenge)
        {
            using var sqliteConnection = new SQLiteConnection(_config.botDbLocate);

            var sql = "INSERT INTO Challenge (title, channel_id, leader_name, leader_id) VALUES (@Title, @ChannelId, @LeaderName, @LeaderId)";
            {
                var rowsAffected = sqliteConnection.Execute(sql, challenge);
                Console.WriteLine($"{rowsAffected} row(s) inserted.");
            }
        }
    }
}
