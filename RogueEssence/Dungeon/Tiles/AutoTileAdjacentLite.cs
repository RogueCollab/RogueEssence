using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class AutoTileAdjacentLite : AutoTileBase
    {
        //like autotile adjacent, but requires less tiles (22 instead of 47)

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

        public override void AutoTileArea(INoise noise, Loc rectStart, Loc rectSize, Loc totalSize, PlacementMethod placementMethod, QueryMethod presenceMethod, QueryMethod queryMethod)
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

            for (int xx = 0; xx < rectSize.X + 2; xx++)
            {
                for (int yy = 0; yy < rectSize.Y + 2; yy++)
                {
                    if (Collision.InBounds(totalSize.X, totalSize.Y, new Loc(rectStart.X + xx, rectStart.Y + yy)) && presenceMethod(rectStart.X + xx - 1, rectStart.Y + yy - 1))
                        textureMainBlock(mainArray, rectStart, rectStart.X + xx - 1, rectStart.Y + yy - 1, queryMethod);
                }
            }

            for (int xx = 0; xx < rectSize.X; xx++)
            {
                for (int yy = 0; yy < rectSize.Y; yy++)
                {
                    if (Collision.InBounds(totalSize.X, totalSize.Y, new Loc(rectStart.X + xx, rectStart.Y + yy)) && presenceMethod(rectStart.X + xx, rectStart.Y + yy) && mainArray[xx+1][yy+1] == -1)
                        textureLooseBlock(looseArray, mainArray, rectStart, rectStart.X + xx, rectStart.Y + yy, queryMethod);
                }
            }

            for (int xx = rectStart.X; xx < rectSize.X; xx++)
            {
                for (int yy = rectStart.Y; yy < rectSize.Y; yy++)
                {
                    int neighborCode = mainArray[xx + 1 - rectStart.X][yy + 1 - rectStart.Y];
                    if (looseArray[xx - rectStart.X][yy - rectStart.Y] != -1)
                        neighborCode = looseArray[xx - rectStart.X][yy - rectStart.Y];

                    if (neighborCode != -1)
                        placementMethod(xx, yy, GetVariantCode(noise.Get2DUInt64((ulong)xx, (ulong)yy), neighborCode));
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


        public override int GetVariantCode(ulong randCode, int neighborCode)
        {
            List<TileLayer> tileVars = GetTileVariants(neighborCode);
            return SelectTileVariant(randCode, tileVars.Count) << 8 | (neighborCode & 0xFF);
        }


        public override List<TileLayer> GetLayers(int variantCode)
        {
            if (variantCode == -1)
                new List<TileLayer>() { Center[0] };

            int lowerCode = variantCode & 0xFF;
            int upperCode = variantCode >> 8 & 0xFF;

            List<TileLayer> tileVars = GetTileVariants(lowerCode);
            List<TileLayer> tileList = new List<TileLayer>();
            AddBoundedLayer(tileList, tileVars, upperCode);

            return tileList;
        }

        private void AddBoundedLayer(List<TileLayer> results, List<TileLayer> variants, int variantCode)
        {
            if (variants.Count == 0)
                return;
            results.Add(variants[Math.Min(variantCode, variants.Count - 1)]);
        }



        private List<TileLayer> GetTileVariants(int neighborCode)
        {
            if (neighborCode == Convert.ToInt32("00000000", 2))
                return Isolated;
            else if (neighborCode == Convert.ToInt32("00000001", 2))
                return ColumnTop;
            else if (neighborCode == Convert.ToInt32("00000010", 2))
                return RowRight;
            else if (neighborCode == Convert.ToInt32("00000100", 2))
                return ColumnBottom;
            else if (neighborCode == Convert.ToInt32("00001000", 2))
                return RowLeft;
            else if (neighborCode == Convert.ToInt32("00000101", 2))
                return ColumnCenter;
            else if (neighborCode == Convert.ToInt32("00001010", 2))
                return RowCenter;
            else if (neighborCode == Convert.ToInt32("00010011", 2))
                return TopRight;
            else if (neighborCode == Convert.ToInt32("00100110", 2))
                return BottomRight;
            else if (neighborCode == Convert.ToInt32("01001100", 2))
                return BottomLeft;
            else if (neighborCode == Convert.ToInt32("10001001", 2))
                return TopLeft;
            else if (neighborCode == Convert.ToInt32("00110111", 2))
                return Right;
            else if (neighborCode == Convert.ToInt32("10011011", 2))
                return TopCenter;
            else if (neighborCode == Convert.ToInt32("11001101", 2))
                return Left;
            else if (neighborCode == Convert.ToInt32("01101110", 2))
                return BottomCenter;
            else if (neighborCode == Convert.ToInt32("01011111", 2))
                return DiagonalBack;
            else if (neighborCode == Convert.ToInt32("10101111", 2))
                return DiagonalForth;
            else if (neighborCode == Convert.ToInt32("11101111", 2))
                return TopRightEdge;
            else if (neighborCode == Convert.ToInt32("11011111", 2))
                return BottomRightEdge;
            else if (neighborCode == Convert.ToInt32("10111111", 2))
                return BottomLeftEdge;
            else if (neighborCode == Convert.ToInt32("01111111", 2))
                return TopLeftEdge;
            else if (neighborCode == Convert.ToInt32("11111111", 2))
                return Center;
            return Center;
        }

    }
}
