using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class AutoTileStacked : AutoTileBase
    {
        public TileLayer TopLeft;
        public TileLayer Top;
        public TileLayer TopRight;

        public TileLayer Left;
        public TileLayer Center;
        public TileLayer Right;

        public TileLayer BottomLeft;
        public TileLayer Bottom;
        public TileLayer BottomRight;


        public TileLayer TopLeftEdge;
        public TileLayer TopRightEdge;
        public TileLayer BottomRightEdge;
        public TileLayer BottomLeftEdge;


        public TileLayer ColumnTop;
        public TileLayer ColumnCenter;
        public TileLayer ColumnBottom;

        public TileLayer RowLeft;
        public TileLayer RowCenter;
        public TileLayer RowRight;


        public TileLayer Surrounded;

        public AutoTileStacked()
        {

            TopLeft = new TileLayer();
            Top = new TileLayer();
            TopRight = new TileLayer();

            Left = new TileLayer();
            Center = new TileLayer();
            Right = new TileLayer();

            BottomLeft = new TileLayer();
            Bottom = new TileLayer();
            BottomRight = new TileLayer();


            TopLeftEdge = new TileLayer();
            TopRightEdge = new TileLayer();
            BottomRightEdge = new TileLayer();
            BottomLeftEdge = new TileLayer();


            ColumnTop = new TileLayer();
            ColumnCenter = new TileLayer();
            ColumnBottom = new TileLayer();

            RowLeft = new TileLayer();
            RowCenter = new TileLayer();
            RowRight = new TileLayer();


            Surrounded = new TileLayer();
        }


        public override void AutoTileArea(INoise noise, Loc rectStart, Loc rectSize, Loc totalSize, PlacementMethod placementMethod, QueryMethod presenceMethod, QueryMethod queryMethod)
        {
            for (int xx = 0; xx < rectSize.X; xx++)
            {
                for (int yy = 0; yy < rectSize.Y; yy++)
                {
                    int neighborCode = -1;
                    if (Collision.InBounds(totalSize.X, totalSize.Y, rectStart + new Loc(xx, yy)) && presenceMethod(xx + rectStart.X, yy + rectStart.Y))
                        neighborCode = textureWaterTile(xx + rectStart.X, yy + rectStart.Y, queryMethod);
                    
                    if (neighborCode != -1)
                        placementMethod(rectStart.X + xx, rectStart.Y + yy, neighborCode);
                }
            }
        }

        private int textureWaterTile(int x, int y, QueryMethod queryMethod)
        {
            int tex_num = 0;
            // 526
            // 1 3
            // 407
            for (int n = 0; n < DirRemap.WRAPPED_DIR8.Length; n++)
            {
                if (IsBlocked(queryMethod, x, y, DirRemap.WRAPPED_DIR8[n]))
                    tex_num |= (0x1 << n);
            }

            //if a cardinal direction does not pass, its adjacent diagonals do not either
            for (int n = 0; n < DirExt.DIR4_COUNT; n++)
            {
                if ((tex_num & (0x1 << n)) == 0x0)
                {
                    Dir8 dir1 = DirExt.AddAngles(((Dir4)n).ToDir8(), Dir8.DownLeft);
                    Dir8 dir2 = DirExt.AddAngles(((Dir4)n).ToDir8(), Dir8.DownRight);

                    tex_num &= ~(0x1 << toWrappedInt(dir1));
                    tex_num &= ~(0x1 << toWrappedInt(dir2));
                }
            }

            return tex_num;
        }

        public override int GetVariantCode(ulong randCode, int neighborCode)
        {
            return neighborCode;
        }

        public override List<TileLayer> GetLayers(int variantCode)
        {
            if (variantCode == -1)
                new List<TileLayer>() { Center };

            List<TileLayer> tileList = new List<TileLayer>();
            int mask = Convert.ToInt32("11110000", 2);
            if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00000000", 2))
                tileList.Add(new TileLayer(Surrounded));
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00000001", 2))
                tileList.Add(new TileLayer(ColumnTop));
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00000010", 2))
                tileList.Add(new TileLayer(RowRight));
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00000100", 2))
                tileList.Add(new TileLayer(ColumnBottom));
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00001000", 2))
                tileList.Add(new TileLayer(RowLeft));
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00000101", 2))
                tileList.Add(new TileLayer(ColumnCenter));
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00001010", 2))
                tileList.Add(new TileLayer(RowCenter));
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00000011", 2))
            {
                tileList.Add(new TileLayer(TopRight));
                mask &= Convert.ToInt32("11100000", 2);
            }
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00000110", 2))
            {
                tileList.Add(new TileLayer(BottomRight));
                mask &= Convert.ToInt32("11010000", 2);
            }
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00001100", 2))
            {
                tileList.Add(new TileLayer(BottomLeft));
                mask &= Convert.ToInt32("10110000", 2);
            }
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00001001", 2))
            {
                tileList.Add(new TileLayer(TopLeft));
                mask &= Convert.ToInt32("01110000", 2);
            }
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00000111", 2))
            {
                tileList.Add(new TileLayer(Right));
                mask &= Convert.ToInt32("11000000", 2);
            }
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00001011", 2))
            {
                tileList.Add(new TileLayer(Top));
                mask &= Convert.ToInt32("01100000", 2);
            }
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00001101", 2))
            {
                tileList.Add(new TileLayer(Left));
                mask &= Convert.ToInt32("00110000", 2);
            }
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00001110", 2))
            {
                tileList.Add(new TileLayer(Bottom));
                mask &= Convert.ToInt32("10010000", 2);
            }
            else if ((variantCode & Convert.ToInt32("00001111", 2)) == Convert.ToInt32("00001111", 2))
            {
                tileList.Add(new TileLayer(Center));
                mask &= Convert.ToInt32("00000000", 2);
            }

            //add edges
            int diagonal_map = variantCode | mask;
            if ((diagonal_map & Convert.ToInt32("00010000", 2)) == 0)
                tileList.Add(new TileLayer(TopRightEdge));

            if ((diagonal_map & Convert.ToInt32("00100000", 2)) == 0)
                tileList.Add(new TileLayer(BottomRightEdge));

            if ((diagonal_map & Convert.ToInt32("01000000", 2)) == 0)
                tileList.Add(new TileLayer(BottomLeftEdge));

            if ((diagonal_map & Convert.ToInt32("10000000", 2)) == 0)
                tileList.Add(new TileLayer(TopLeftEdge));

            return tileList;
        }


        private static int toWrappedInt(Dir8 dir)
        {
            switch (dir)
            {
                case Dir8.None: return -1;
                case Dir8.Down: return 0;
                case Dir8.Left: return 1;
                case Dir8.Up: return 2;
                case Dir8.Right: return 3;
                case Dir8.DownLeft: return 4;
                case Dir8.UpLeft: return 5;
                case Dir8.UpRight: return 6;
                case Dir8.DownRight: return 7;
                default:
                    throw new ArgumentException("Invalid value to convert.");
            }
        }
    }
}
