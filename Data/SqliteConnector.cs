using Dapper;
using ExT.Config;
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

            sqliteConnection.Open();

                // 테이블 생성 쿼리
                var createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Exercise (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    exercise_time TEXT,
                    calories_burned TEXT,
                    other_data TEXT
                    );
                ";
            // Dapper를 이용해 테이블 생성 쿼리 실행
            sqliteConnection.Execute(createTableQuery);
            
        }
    }
}
