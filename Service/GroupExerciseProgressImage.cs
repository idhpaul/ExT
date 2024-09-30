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

namespace ExT.Service
{

    public class GroupExerciseProgressImage
    {
        private (string name, int steps)[] peopleSteps;
        private int imageWidth;
        private int imageHeightPerBar;
        private string fontName;
        private float fontSize;
        private int margin;

        public GroupExerciseProgressImage((string name, int steps)[] peopleSteps, int imageWidth = 300, int imageHeightPerBar = 50, string fontName = "Arial", float fontSize = 25, int margin = 10)
        {
            this.peopleSteps = peopleSteps.OrderByDescending(p => p.steps).ToArray(); // 걸음 수에 따라 정렬
            this.imageWidth = imageWidth;
            this.imageHeightPerBar = imageHeightPerBar;
            this.fontName = fontName;
            this.fontSize = fontSize;
            this.margin = margin;
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
            int maxSteps = peopleSteps.Max(p => p.steps);   // 최대 걸음 수

            // 폰트 크기 및 텍스트 길이 측정
            int maxTextWidth = 0; // 최대 텍스트 너비를 저장할 변수
            foreach (var person in peopleSteps)
            {
                var nameSize = MeasureText(person.name, font);
                var stepsSize = MeasureText(person.steps.ToString(), font);
                maxTextWidth = Math.Max(maxTextWidth, (int)Math.Max(nameSize.Width, stepsSize.Width));
            }

            // 이미지 크기 결정
            int finalImageWidth = Math.Max(imageWidth, maxTextWidth + margin * 2); // 텍스트에 따라 이미지 너비 조정
            int finalImageHeight = imageHeightPerBar * peopleSteps.Length;

            // 이미지 생성
            using (Image<Rgba32> image = new Image<Rgba32>(finalImageWidth, finalImageHeight))
            {
                // 이미지 배경색 설정
                image.Mutate(ctx => ctx.Fill(Color.White));

                // 바 그래프 그리기 (세로 방향)
                for (int i = 0; i < peopleSteps.Length; i++)
                {
                    var person = peopleSteps[i];
                    float barWidth = (float)person.steps / maxSteps * (finalImageWidth - maxTextWidth - margin * 2); // 비례적으로 바 너비 설정

                    // 바의 좌상단 위치 계산
                    int yPosition = i * imageHeightPerBar;

                    // 바 그리기 (회색 바)
                    image.Mutate(ctx => ctx.Fill(Color.Gray, new Rectangle(margin, yPosition, (int)barWidth, imageHeightPerBar - 10)));

                    // 이름 표시 (바 왼쪽 표시)
                    image.Mutate(ctx => ctx.DrawText(
                        person.name,
                        font,
                        Color.Black,
                        new PointF(margin, yPosition + imageHeightPerBar / 2 - 10))); // 이름을 바 옆에 표시

                    // 걸음 수 텍스트의 너비를 측정하여 오른쪽 정렬
                    var stepsText = person.steps.ToString();
                    var stepsSize = MeasureText(stepsText, font);
                    float stepsXPosition = finalImageWidth - stepsSize.Width - margin; // 이미지 끝에서 걸음 수 텍스트 너비만큼 빼서 오른쪽 정렬

                    // 걸음 수 표시 (바 오른쪽 표시)
                    image.Mutate(ctx => ctx.DrawText(
                        stepsText,
                        font,
                        Color.Black,
                        new PointF(stepsXPosition, yPosition + imageHeightPerBar / 2 - 10))); // 걸음 수를 바 옆에 표시
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
