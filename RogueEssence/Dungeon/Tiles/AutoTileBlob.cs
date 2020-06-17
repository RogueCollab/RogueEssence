using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class AutoTileBlob : AutoTileBase
    {
        public override TileLayer[] Generic { get { return Center.ToArray(); } }

        public List<TileLayer> TopLeft;
        public List<TileLayer> TopCenter;
        public List<TileLayer> TopRight;

        public List<TileLayer> Left;
        public List<TileLayer> Center;
        public List<TileLayer> Right;

        public List<TileLayer> BottomLeft;
        public List<TileLayer> BottomCenter;
        public List<TileLayer> BottomRight;


        public List<TileLayer> TopLeftEdge;
        public List<TileLayer> TopRightEdge;
        public List<TileLayer> BottomLeftEdge;
        public List<TileLayer> BottomRightEdge;

        public List<TileLayer> DiagonalForth;
        public List<TileLayer> DiagonalBack;

        public List<TileLayer> Isolated;

        public List<TileLayer> Variations;

        public AutoTileBlob()
        {
            TopLeft = new List<TileLayer>();
            TopCenter = new List<TileLayer>();
            TopRight = new List<TileLayer>();

            Left = new List<TileLayer>();
            Center = new List<TileLayer>();
            Right = new List<TileLayer>();

            BottomLeft = new List<TileLayer>();
            BottomCenter = new List<TileLayer>();
            BottomRight = new List<TileLayer>();


            TopLeftEdge = new List<TileLayer>();
            TopRightEdge = new List<TileLayer>();
            BottomLeftEdge = new List<TileLayer>();
            BottomRightEdge = new List<TileLayer>();

            DiagonalForth = new List<TileLayer>();
            DiagonalBack = new List<TileLayer>();

            Isolated = new List<TileLayer>();

            Variations = new List<TileLayer>();
        }


        public override void AutoTileArea(ReRandom rand, Loc rectStart, Loc rectSize, PlacementMethod placementMethod, QueryMethod queryMethod)
        {
            int[][] pass1Array = new int[rectSize.X][];
            for (int ii = 0; ii < rectSize.X; ii++)
            {
                pass1Array[ii] = new int[rectSize.Y];
                for (int jj = 0; jj < rectSize.Y; jj++)
                    pass1Array[ii][jj] = -1;
            }

            for (int x = 0; x < rectSize.X; x++)
            {
                for (int y = 0; y < rectSize.Y; y++)
                {
                    int neighborCode = -1;
                    if (queryMethod(rectStart.X + x, rectStart.Y + y))
                        neighborCode = textureBlock(rectStart.X + x, rectStart.Y + y, queryMethod);

                    if (neighborCode != -1)
                        placementMethod(rectStart.X + x, rectStart.Y + y, GetTile(rand, neighborCode));
                }
            }
        }


        private int textureBlock(int x, int y, QueryMethod queryMethod)
        {
            //  2
            // 1 3
            //  0
            bool[] blockedDirs = new bool[DirExt.DIR4_COUNT];
            for (int n = 0; n < DirExt.DIR4_COUNT; n++)
            {
                blockedDirs[n] = IsBlocked(queryMethod, x, y, ((Dir4)n).ToDir8());
            }

            // 1|2
            // -+-
            // 0|3
            bool[] blockedQuads = new bool[DirExt.DIR4_COUNT];
            for (int n = 0; n < DirExt.DIR4_COUNT; n++)
            {
                if (blockedDirs[n] && blockedDirs[(n + 1) % DirExt.DIR4_COUNT])
                {
                    Dir8 diag = DirExt.AddAngles(((Dir4)n).ToDir8(), Dir8.DownLeft);
                    if (IsBlocked(queryMethod, x, y, diag))
                        blockedQuads[n] = true;
                }
            }
            int tex_num = 0;

            if (blockedQuads[(int)Dir4.Down])//down and left are blocked
                tex_num |= Convert.ToInt32("00010011", 2);
            if (blockedQuads[(int)Dir4.Left])//left and up are blocked
                tex_num |= Convert.ToInt32("00100110", 2);
            if (blockedQuads[(int)Dir4.Up])//up and right are blocked
                tex_num |= Convert.ToInt32("01001100", 2);
            if (blockedQuads[(int)Dir4.Right])//right and down are blocked
                tex_num |= Convert.ToInt32("10001001", 2);

            return tex_num;
        }



        private List<TileLayer> GetTile(ReRandom rand, int neighborCode)
        {
            List<TileLayer> tileList = new List<TileLayer>();
            if (neighborCode == Convert.ToInt32("00010011", 2))
                tileList.Add(new TileLayer(SelectTile(rand, TopRight)));
            else if (neighborCode == Convert.ToInt32("00100110", 2))
                tileList.Add(new TileLayer(SelectTile(rand, BottomRight)));
            else if (neighborCode == Convert.ToInt32("01001100", 2))
                tileList.Add(new TileLayer(SelectTile(rand, BottomLeft)));
            else if (neighborCode == Convert.ToInt32("10001001", 2))
                tileList.Add(new TileLayer(SelectTile(rand, TopLeft)));
            else if (neighborCode == Convert.ToInt32("00110111", 2))
                tileList.Add(new TileLayer(SelectTile(rand, Right)));
            else if (neighborCode == Convert.ToInt32("10011011", 2))
                tileList.Add(new TileLayer(SelectTile(rand, TopCenter)));
            else if (neighborCode == Convert.ToInt32("11001101", 2))
                tileList.Add(new TileLayer(SelectTile(rand, Left)));
            else if (neighborCode == Convert.ToInt32("01101110", 2))
                tileList.Add(new TileLayer(SelectTile(rand, BottomCenter)));
            else if (neighborCode == Convert.ToInt32("01011111", 2))
                tileList.Add(new TileLayer(SelectTile(rand, DiagonalBack)));
            else if (neighborCode == Convert.ToInt32("10101111", 2))
                tileList.Add(new TileLayer(SelectTile(rand, DiagonalForth)));
            else if (neighborCode == Convert.ToInt32("11101111", 2))
                tileList.Add(new TileLayer(SelectTile(rand, TopRightEdge)));
            else if (neighborCode == Convert.ToInt32("11011111", 2))
                tileList.Add(new TileLayer(SelectTile(rand, BottomRightEdge)));
            else if (neighborCode == Convert.ToInt32("10111111", 2))
                tileList.Add(new TileLayer(SelectTile(rand, BottomLeftEdge)));
            else if (neighborCode == Convert.ToInt32("01111111", 2))
                tileList.Add(new TileLayer(SelectTile(rand, TopLeftEdge)));
            else if (neighborCode == Convert.ToInt32("11111111", 2))
            {
                tileList.Add(new TileLayer(SelectTile(rand, Center)));

                if (Variations.Count > 0)
                    tileList.Add(new TileLayer(SelectTile(rand, Variations)));
            }
            else
                tileList.Add(new TileLayer(SelectTile(rand, Isolated)));

            return tileList;
        }
    }
}
