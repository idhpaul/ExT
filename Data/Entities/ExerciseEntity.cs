using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExT.Data.Entities
{
    public class ExerciseEntity
    {
        private string _exercise_time = default!;
        public string ExerciseTime { get { return _exercise_time; } set { _exercise_time = value; } }

        private string _calories_burned = default!;
        public string CaloriesBurned { get { return _calories_burned; } set { _calories_burned = value; } }

        private string _other_data = default!;
        public string OtherData { get { return _other_data; } set { _other_data = value; } }

        public ExerciseEntity() { }

        public ExerciseEntity(string exercise_time, string calories_burned, string other_data)
        {
            _exercise_time = exercise_time;
            _calories_burned= calories_burned;
            _other_data = other_data;
        }
    }
}
