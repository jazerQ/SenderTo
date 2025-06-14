using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace WatermarkService.Services;

public class MarkService
{
    private FontCollection _fontCollection;

    public MarkService()
    {
        _fontCollection = new FontCollection();
    }
    
    public byte[] SetWatermark(byte[] bytes)
    {
        using (var img = Image.Load(bytes))
        {
            var fontFamily = _fontCollection.Add("minecraft_0.ttf");
            Font font = fontFamily.CreateFont(36, FontStyle.Bold);

            using (var newImg = img.Clone(ctx => ApplyScalingWaterMark(ctx, font, "ExamplePic", Color.White, 5, false)))
            {
                using (var ms = new MemoryStream())
                {
                    newImg.Save(ms, new PngEncoder());
                    return ms.ToArray();
                }
            }
        }
    }

    private IImageProcessingContext ApplyScalingWaterMark(IImageProcessingContext processingContext,
        Font font,
        string text,
        Color color,
        float padding,
        bool _)
    {
        var imageSize = processingContext.GetCurrentSize();
        float targetWidth = imageSize.Width - (padding * 2);
        float targetHeight = imageSize.Height - (padding * 2);
        
        //Бинарный поиск для нахождения оптимального размера шрифта
        float minFontSize = 1;
        float maxFontSize = font.Size;
        
        //Сначала проверьте, соответствует ли развернутый текст изображению, и увеличьте масштаб, если нет.
        //Мы уменьшаем результат, чтобы учесть накопленные ошибки округления.
        FontRectangle currentBounds = TextMeasurer.MeasureAdvance(text, new TextOptions(font));
        if (currentBounds.Width < targetWidth)
        {
            maxFontSize = MathF.Floor(maxFontSize * (targetWidth / currentBounds.Width));
        }

        while (minFontSize < maxFontSize)
        {
            var midFontSize = (maxFontSize + minFontSize) / 2; // находим середину между минимальным и максимальным шрифтом
            var midFont = new Font(font, midFontSize); // создаем шрифт такого размера
            currentBounds = TextMeasurer.MeasureAdvance(text, new TextOptions(midFont)); // Замеряем сколько места займет текст, если его рисовать

            if (currentBounds.Height > targetHeight)
            {
                maxFontSize = midFontSize - 0.1f; // если по высоте не влезает, шрифт слишком большой - уменьшаем шрифт
            }
            else
            {
                minFontSize = midFontSize + 0.1f; // если влезает то можно чуть побольше
            }
        }

        //используем оптимальный размер шрифта
        Font scaledFont = new(font, minFontSize);
        
        //Создадим настройки текста с измененными параметрами
        var textOptions = new RichTextOptions(scaledFont)
        {
            Origin = new Vector2(padding, imageSize.Height * .5f),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        return processingContext.DrawText(textOptions, text, color);
    }
}