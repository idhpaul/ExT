using SixLabors.Fonts;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using ExT.Data.Entities;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ExT.Service
{
    public class UserExerciseSummary
    {
        public required string UserName { get; set; }
        public required ulong UserId { get; set; }
        public required TimeSpan TotalExerciseTime { get; set; }
        public required string TotalCaloriesBurned { get; set; }
    }

    public class GroupExerciseProgressImage
    {
        private readonly List<ExerciseEntity> userExercise;
        private int imageWidth;
        private int imageHeightPerBar;
        private string fontName;
        private float fontSize;
        private int margin;

        public List<UserExerciseSummary> ExerciseSummary { get; init; }

        public GroupExerciseProgressImage(List<ExerciseEntity> userExercise, int imageWidth = 300, int imageHeightPerBar = 50, string fontName = "Arial", float fontSize = 25, int margin = 10)
        {
            this.userExercise = userExercise;
            this.imageWidth = imageWidth;
            this.imageHeightPerBar = imageHeightPerBar;
            this.fontName = fontName;
            this.fontSize = fontSize;
            this.margin = margin;

            // 시간 및 칼로리 문자열 후처리(캐스팅, 단위 필터) (LINQ)
            ExerciseSummary = userExercise
            .GroupBy(e => new { e.UserId, e.UserName })
            .Select(group => new UserExerciseSummary
            {
                UserName = group.Key.UserName,
                UserId = group.Key.UserId,
                TotalExerciseTime = group.Aggregate(TimeSpan.Zero, (sum, e) => sum.Add(ParseExerciseTime(e.ExerciseTime))),
                TotalCaloriesBurned = group.Sum(e => ParseCalories(e.CaloriesBurned)) + " kcal"
            })
            .ToList();

            foreach (var data in ExerciseSummary)
            {
                Console.WriteLine($"User: {data.UserName}, Total Exercise Time: {data.TotalExerciseTime}, Total Calories Burned: {data.TotalCaloriesBurned}");
            }

        }

        public TimeSpan ParseExerciseTime(string time)
        {
            TimeSpan result;

            // ":" 개수를 기반으로 형식 결정
            if (time.Count(c => c == ':') == 1)  // "mm:ss" 형식일 때
            {
                // "mm:ss" 형식으로 파싱
                return TimeSpan.ParseExact(time, "m\\:ss", null);
            }
            else if (time.Count(c => c == ':') == 2) // "hh:mm:ss" 형식일 때
            {
                // "hh:mm:ss" 형식으로 파싱
                return TimeSpan.Parse(time);
            }
            else
            {
                throw new FormatException("Invalid time format");
            }
        }

        public int ParseCalories(string calories)
        {
            // 숫자를 추출하는 정규식
            var match = Regex.Match(calories, @"\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        }

        // 텍스트 크기를 측정하는 도우미 메서드
        private FontRectangle MeasureText(string text, Font font)
        {
            using (var tempImage = new Image<Rgba32>(1, 1)) // 임시 이미지 생성
            {
                return TextMeasurer.MeasureSize(text, new TextOptions(font));
            }
        }

        // 이미지를 생성하고 저장하는 메서드
        public void GenerateImage(string outputPath)
        {
            var font = SystemFonts.CreateFont(fontName, fontSize);
            var orderExerciseTime = ExerciseSummary.OrderByDescending(u => u.TotalExerciseTime);   // 최대 운동 시간

            // 폰트 크기 및 텍스트 길이 측정
            int maxTextWidth = 0; // 최대 텍스트 너비를 저장할 변수
            var nameSize = MeasureText(orderExerciseTime.First().UserName, font);
            var exerciseTimeSize = MeasureText(orderExerciseTime.First().TotalExerciseTime.ToString(), font);
            maxTextWidth = Math.Max(maxTextWidth, (int)Math.Max(nameSize.Width, exerciseTimeSize.Width));
            
            // 이미지 크기 결정
            int finalImageWidth = Math.Max(imageWidth, maxTextWidth + margin * 2); // 텍스트에 따라 이미지 너비 조정
            int finalImageHeight = imageHeightPerBar * ExerciseSummary.Count();

            // 이미지 생성
            using (Image<Rgba32> image = new Image<Rgba32>(finalImageWidth, finalImageHeight))
            {
                // 이미지 배경색 설정
                image.Mutate(ctx => ctx.Fill(Color.White));

                // 바 그래프 그리기 (세로 방향)
                for (int i = 0; i < orderExerciseTime.Count(); i++)
                {
                    var user = orderExerciseTime.ElementAt(i);
                    float barWidth = (float)user.TotalExerciseTime.ToString().Length / orderExerciseTime.First().TotalExerciseTime.ToString().Length * (finalImageWidth - maxTextWidth - margin * 2); // 비례적으로 바 너비 설정

                    // 바의 좌상단 위치 계산
                    int yPosition = i * imageHeightPerBar;

                    // 바 그리기 (회색 바)
                    image.Mutate(ctx => ctx.Fill(Color.Gray, new Rectangle(margin, yPosition, (int)barWidth, imageHeightPerBar - 10)));

                    // 이름 표시 (바 왼쪽 표시)
                    image.Mutate(ctx => ctx.DrawText(
                        user.UserName,
                        font,
                        Color.Black,
                        new PointF(margin, yPosition + imageHeightPerBar / 2 - 10))); // 이름을 바 옆에 표시

                    // 걸음 수 텍스트의 너비를 측정하여 오른쪽 정렬
                    var exerciseTimeText = user.TotalExerciseTime.ToString();
                    var exerciseTimeSize1 = MeasureText(exerciseTimeText, font);
                    float exerciseTimeXPosition = finalImageWidth - exerciseTimeSize1.Width - margin; // 이미지 끝에서 걸음 수 텍스트 너비만큼 빼서 오른쪽 정렬

                    // 총 운동시간 표시 (바 오른쪽 표시)
                    image.Mutate(ctx => ctx.DrawText(
                        user.TotalExerciseTime.ToString(),
                        font,
                        Color.Black,
                        new PointF(exerciseTimeXPosition, yPosition + imageHeightPerBar / 2 - 10))); // 걸음 수를 바 옆에 표시
                }

                // WebP 옵션 설정
                var webpOptions = new WebpEncoder
                {
                    FileFormat = WebpFileFormatType.Lossy,
                    Quality = 80 // 품질 설정 (0-100)
                };

                // 파일 저장
                using FileStream fs = new FileStream(outputPath, FileMode.Create);
                
                image.Save(fs, webpOptions);
                

                Console.WriteLine($"WebP 파일이 생성되었습니다: {outputPath}");
            }
        }
    }

}
