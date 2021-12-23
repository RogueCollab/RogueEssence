using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class AutoTileBlob : AutoTileBase
    {
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
        }


        public override void AutoTileArea(INoise noise, Loc rectStart, Loc rectSize, Loc totalSize, PlacementMethod placementMethod, QueryMethod presenceMethod, QueryMethod queryMethod)
        {
            int[][] pass1Array = new int[rectSize.X][];
            for (int ii = 0; ii < rectSize.X; ii++)
            {
                pass1Array[ii] = new int[rectSize.Y];
                for (int jj = 0; jj < rectSize.Y; jj++)
                    pass1Array[ii][jj] = -1;
            }
            
            for (int xx = rectStart.X; xx < rectStart.X + rectSize.X; xx++)
            {
                for (int yy = rectStart.Y; yy < rectStart.Y + rectSize.Y; yy++)
                {
                    int neighborCode = -1;
                    if (Collision.InBounds(totalSize.X, totalSize.Y, new Loc(xx, yy)) && presenceMethod(xx, yy))
                        neighborCode = textureBlock(xx, yy, queryMethod);

                    if (neighborCode != -1)
                        placementMethod(xx, yy, GetVariantCode(noise.Get2DUInt64((ulong)xx, (ulong)yy), neighborCode));
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
            if (neighborCode == Convert.ToInt32("00010011", 2))
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
            else
                return Isolated;
        }

    }
}
