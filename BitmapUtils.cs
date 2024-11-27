using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace CS3DSpring;

public static class BitmapUtils
{
    public static Bitmap GetBitmapByImageSource(BitmapSource source, bool isPng = false)
    {
        using var stream = new MemoryStream();
        BitmapEncoder encoder = isPng
          ? new PngBitmapEncoder()
          : new BmpBitmapEncoder()
          ;
        encoder.Frames.Add(BitmapFrame.Create(source));
        encoder.Save(stream);
        using var bp = new Bitmap(stream);
        return new (bp);
    }
    //BitmapImage to Bitmap
    public static Bitmap GetBitmapByBitmapImage(BitmapImage bitmapImage, bool isPng = false)
    {
        using var stream = new MemoryStream();
        BitmapEncoder encoder = isPng
            ? new PngBitmapEncoder()
            : new BmpBitmapEncoder()
            ;
        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
        encoder.Save(stream);
        using var bp = new Bitmap(stream);
        return new (bp);
    }
    // Bitmap to BitmapImage
    public static BitmapImage GetBitmapImageBybitmap(Bitmap bitmap, bool isPng = true)
    {
        var image = new BitmapImage();
        try
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, isPng ? ImageFormat.Png : ImageFormat.Bmp);
            image.BeginInit();
            image.StreamSource = stream;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch (Exception)
        {
            return null;
        }
    }
}