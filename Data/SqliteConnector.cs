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
            Console.WriteLine($"Sqlite db name : {_config.botName}");
            Console.WriteLine($"Sqlite db locate : {_config.botDbLocate}");

            using var sqliteConnection = new SQLiteConnection(_config.botDbLocate);
            
            if (!File.Exists(_config.botDbLocate))
            {
                Console.WriteLine("Sqlite db 파일 생성");
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
                    Title TEXT,
                    MessageId INTEGER,
                    ChannelId INTEGER,
                    LeaderName TEXT,
                    LeaderId INTEGER
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
                    ExerciseTime TEXT,
                    CaloriesBurned TEXT,
                    OtherData TEXT,
                    UserName TEXT,
                    UserId INTEGER,
                    ChannelId INTEGER
                    );
                ";
            // Dapper를 이용해 테이블 생성 쿼리 실행
            sqliteConnection.Execute(createExerciseTable);
        }

        public async Task<ChallengeEntity> DbSelectChallenge(ulong messageId)
        {
            using var sqliteConnection = new SQLiteConnection(_config.botDbLocate);

            var sql = "SELECT * FROM Challenge WHERE MessageId = @MessageId";

            var challenge = await sqliteConnection.QuerySingleOrDefaultAsync<ChallengeEntity>(sql, new { MessageId = messageId });
            return challenge;

        }

        public void DbInsertChallenge(ChallengeEntity challenge)
        {
            using var sqliteConnection = new SQLiteConnection(_config.botDbLocate);

            var sql = "INSERT INTO Challenge (Title, MessageId, ChannelId, LeaderName, LeaderId) VALUES (@Title, @MessageId, @ChannelId, @LeaderName, @LeaderId)";
            {
                var rowsAffected = sqliteConnection.Execute(sql, challenge);
                Console.WriteLine($"{rowsAffected} row(s) inserted.");
            }
        }

        public void DbDeleteChallenge(ChallengeEntity challenge)
        {
            using var sqliteConnection = new SQLiteConnection(_config.botDbLocate);
            
            var sql = "DELETE FROM Challenge WHERE MessageId = @MessageId";
            {
                var rowsAffected = sqliteConnection.Execute(sql, challenge);
                Console.WriteLine($"{rowsAffected} row(s) deleted.");
            }
        }

        public void DbUpdateChallenge(ChallengeEntity challenge)
        {
            using var sqliteConnection = new SQLiteConnection(_config.botDbLocate);

            var sql = "UPDATE Challenge (Title, MessageId, ChannelId, LeaderName, LeaderId) VALUES (@Title, @MessageId, @ChannelId, @LeaderName, @LeaderId)";
            {
                var rowsAffected = sqliteConnection.Execute(sql, challenge);
                Console.WriteLine($"{rowsAffected} row(s) updated.");
            }
        }

        public void DbInsertExercise(ExerciseEntity exercise)
        {
            using var sqliteConnection = new SQLiteConnection(_config.botDbLocate);

            var sql = "INSERT INTO Exercise (ExerciseTime, CaloriesBurned, OtherData, UserName, UserId, ChannelId) VALUES (@ExerciseTime, @CaloriesBurned, @OtherData, @UserName, @UserId, @ChannelId)";
            {
                var rowsAffected = sqliteConnection.Execute(sql, exercise);
                Console.WriteLine($"{rowsAffected} row(s) inserted.");
            }
        }
    }
}
