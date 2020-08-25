using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class AutoTileAdjacentLite : AutoTileBase
    {
        //like autotile adjacent, but requires less tiles (22 instead of 47)
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


        public List<TileLayer> ColumnTop;
        public List<TileLayer> ColumnCenter;
        public List<TileLayer> ColumnBottom;


        public List<TileLayer> RowLeft;
        public List<TileLayer> RowCenter;
        public List<TileLayer> RowRight;


        public List<TileLayer> Isolated;

        
        public AutoTileAdjacentLite()
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


            ColumnTop = new List<TileLayer>();
            ColumnCenter = new List<TileLayer>();
            ColumnBottom = new List<TileLayer>();


            RowLeft = new List<TileLayer>();
            RowCenter = new List<TileLayer>();
            RowRight = new List<TileLayer>();


            Isolated = new List<TileLayer>();

        }

        public override void AutoTileArea(ReRandom rand, Loc rectStart, Loc rectSize, PlacementMethod placementMethod, QueryMethod queryMethod)
        {
            int[][] mainArray = new int[rectSize.X+2][];
            for (int ii = 0; ii < rectSize.X + 2; ii++)
            {
                mainArray[ii] = new int[rectSize.Y + 2];
                for (int jj = 0; jj < rectSize.Y + 2; jj++)
                    mainArray[ii][jj] = -1;
            }
            int[][] looseArray = new int[rectSize.X][];
            for (int ii = 0; ii < rectSize.X; ii++)
            {
                looseArray[ii] = new int[rectSize.Y];
                for (int jj = 0; jj < rectSize.Y; jj++)
                    looseArray[ii][jj] = -1;
            }

            for (int x = 0; x < rectSize.X + 2; x++)
            {
                for (int y = 0; y < rectSize.Y + 2; y++)
                {
                    if (queryMethod(rectStart.X + x - 1, rectStart.Y + y - 1))
                        textureMainBlock(mainArray, rectStart, rectStart.X + x - 1, rectStart.Y + y - 1, queryMethod);
                }
            }

            for (int x = 0; x < rectSize.X; x++)
            {
                for (int y = 0; y < rectSize.Y; y++)
                {
                    if (queryMethod(rectStart.X + x, rectStart.Y + y) && mainArray[x+1][y+1] == -1)
                        textureLooseBlock(looseArray, mainArray, rectStart, rectStart.X + x, rectStart.Y + y, queryMethod);
                }
            }

            for (int ii = 0; ii < rectSize.X; ii++)
            {
                for (int jj = 0; jj < rectSize.Y; jj++)
                {
                    int neighborCode = mainArray[ii + 1][jj + 1];
                    if (looseArray[ii][jj] != -1)
                        neighborCode = looseArray[ii][jj];

                    if (neighborCode != -1)
                        placementMethod(rectStart.X + ii, rectStart.Y + jj, GetTile(rand, neighborCode));
                }
            }
        }


        private void textureMainBlock(int[][] textureArray, Loc rectStart, int x, int y, QueryMethod queryMethod)
        {
            //  2
            // 1 3
            //  0
            bool[] blockedDirs = new bool[DirExt.DIR4_COUNT];
            for (int n = 0; n < DirExt.DIR4_COUNT; n++)
                blockedDirs[n] = IsBlocked(queryMethod, x, y, ((Dir4)n).ToDir8());

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

            if (blockedQuads[(int)Dir4.Down])//down and left pass
                tex_num |= Convert.ToInt32("00010011", 2);
            if (blockedQuads[(int)Dir4.Left])//left and up pass
                tex_num |= Convert.ToInt32("00100110", 2);
            if (blockedQuads[(int)Dir4.Up])//up and right pass
                tex_num |= Convert.ToInt32("01001100", 2);
            if (blockedQuads[(int)Dir4.Right])//right and down pass
                tex_num |= Convert.ToInt32("10001001", 2);

            if (tex_num > 0)
                textureArray[x - rectStart.X + 1][y - rectStart.Y + 1] = tex_num;
        }



        private void textureLooseBlock(int[][] textureArray, int[][] mainTextureArray, Loc rectStart, int x, int y, QueryMethod queryMethod)
        {
            //  2
            // 1 3
            //  0
            bool[] blockedDirs = new bool[DirExt.DIR4_COUNT];
            for (int n = 0; n < DirExt.DIR4_COUNT; n++)
                blockedDirs[n] = IsLooseBlocked(queryMethod, mainTextureArray, rectStart, x, y, ((Dir4)n).ToDir8());

            int tex_num = 0;
            //check horizontal continuity first
            if (blockedDirs[(int)Dir4.Left])
                tex_num |= Convert.ToInt32("00000010", 2);
            if (blockedDirs[(int)Dir4.Right])
                tex_num |= Convert.ToInt32("00001000", 2);

            if (tex_num == 0)
            {
                if (blockedDirs[(int)Dir4.Down])
                {
                    //check to see if tiles to the downleft and downright are empty
                    if (!IsLooseBlocked(queryMethod, mainTextureArray, rectStart, x, y, Dir8.DownLeft) && !IsLooseBlocked(queryMethod, mainTextureArray, rectStart, x, y, Dir8.DownRight))
                        tex_num |= Convert.ToInt32("00000001", 2);
                }
                if (blockedDirs[(int)Dir4.Up])
                {
                    //check to see if tiles to the downleft and downright are empty
                    if (!IsLooseBlocked(queryMethod, mainTextureArray, rectStart, x, y, Dir8.UpLeft) && !IsLooseBlocked(queryMethod, mainTextureArray, rectStart, x, y, Dir8.UpRight))
                        tex_num |= Convert.ToInt32("00000100", 2);
                }
            }

            textureArray[x - rectStart.X][y - rectStart.Y] = tex_num;
        }

        private bool IsLooseBlocked(QueryMethod queryMethod, int[][] mainTextureArray, Loc rectStart, int x, int y, Dir8 dir)
        {
            Loc loc = new Loc(x, y) + dir.GetLoc();

            return queryMethod(loc.X,loc.Y) && mainTextureArray[loc.X - rectStart.X + 1][loc.Y - rectStart.Y + 1] == -1;
        }


        private List<TileLayer> GetTile(ReRandom rand, int neighborCode)
        {
            List<TileLayer> tileList = new List<TileLayer>();
            if (neighborCode == Convert.ToInt32("00000000", 2))
                tileList.Add(new TileLayer(SelectTile(rand, Isolated)));
            else if (neighborCode == Convert.ToInt32("00000001", 2))
                tileList.Add(new TileLayer(SelectTile(rand, ColumnTop)));
            else if (neighborCode == Convert.ToInt32("00000010", 2))
                tileList.Add(new TileLayer(SelectTile(rand, RowRight)));
            else if (neighborCode == Convert.ToInt32("00000100", 2))
                tileList.Add(new TileLayer(SelectTile(rand, ColumnBottom)));
            else if (neighborCode == Convert.ToInt32("00001000", 2))
                tileList.Add(new TileLayer(SelectTile(rand, RowLeft)));
            else if (neighborCode == Convert.ToInt32("00000101", 2))
                tileList.Add(new TileLayer(SelectTile(rand, ColumnCenter)));
            else if (neighborCode == Convert.ToInt32("00001010", 2))
                tileList.Add(new TileLayer(SelectTile(rand, RowCenter)));
            else if (neighborCode == Convert.ToInt32("00010011", 2))
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
                tileList.Add(new TileLayer(SelectTile(rand, Center)));

            return tileList;
        }

    }
}
