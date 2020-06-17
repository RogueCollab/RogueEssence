using Microsoft.Xna.Framework.Graphics;

namespace RectPacker
{
    /// <summary>
    /// Describes an image. Note that this only defines those properties that are relevant when
    /// it comes to mapping an image onto a sprite - its width and height. So for example image file
    /// name is not needed here.
    /// 
    /// This is called IImageInfo rather than IImage, because System.Drawing already defines an Image class.
    /// </summary>
    public class ImageInfo
    {
        public int ID { get; private set; }
        public Texture2D Texture { get; private set; }
        public int Width { get { return Texture.Width; } }
        public int Height { get { return Texture.Height; } }

        public ImageInfo(int id, Texture2D tex)
        {
            ID = id;
            Texture = tex;
        }
    }
}

