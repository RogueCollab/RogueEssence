namespace RectPacker
{
    public class MappedImageInfo
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public ImageInfo ImageInfo { get; private set; }

        public MappedImageInfo(int x, int y, ImageInfo imageInfo)
        {
            X = x;
            Y = y;
            ImageInfo = imageInfo;
        }
    }
}
