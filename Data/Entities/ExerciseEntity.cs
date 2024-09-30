using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExT.Data.Entities
{
    public class ExerciseEntity
    {
        public string ExerciseTime { get; init; } = string.Empty;

        public string CaloriesBurned { get; init; } = string.Empty;

        public string OtherData { get; init; } = string.Empty;
    }
}
