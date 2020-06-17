using System.Collections.Generic;

namespace RectPacker
{
    /// <summary>
    /// Represents the contents of a sprite image.
    /// </summary>
    public class Atlas
    {
        private List<MappedImageInfo> _mappedImages = null;
        private int _width = 0;
        private int _height = 0;

        /// <summary>
        /// Holds the locations of all the individual images within the sprite image.
        /// </summary>
        public List<MappedImageInfo> MappedImages { get { return _mappedImages; } }

        /// <summary>
        /// Width of the sprite image
        /// </summary>
        public int Width { get { return _width; } }

        /// <summary>
        /// Height of the sprite image
        /// </summary>
        public int Height { get { return _height; } }

        /// <summary>
        /// Area of the sprite image
        /// </summary>
        public int Area { get { return _width * _height; } }

        public Atlas()
        {
            _mappedImages = new List<MappedImageInfo>();
            _width = 0;
            _height = 0;
        }

        /// <summary>
        /// Adds a Rectangle to the SpriteInfo, and updates the width and height of the SpriteInfo.
        /// </summary>
        /// <param name="imageLocation"></param>
        public void AddMappedImage(MappedImageInfo imageLocation)
        {
            _mappedImages.Add(imageLocation);

            ImageInfo newImage = imageLocation.ImageInfo;

            int highestY = imageLocation.Y + newImage.Height;
            int rightMostX = imageLocation.X + newImage.Width;

            if (_height < highestY) { _height = highestY; }
            if (_width < rightMostX) { _width = rightMostX; }
        }

    }
}
