using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class AutoTileAdjacent : AutoTileBase
    {
        public override TileLayer[] Generic { get { return TilexFF[0].ToArray(); } }

        public List<List<TileLayer>> Tilex00;
        public List<List<TileLayer>> Tilex01;
        public List<List<TileLayer>> Tilex02;
        public List<List<TileLayer>> Tilex03;
        public List<List<TileLayer>> Tilex13;
        public List<List<TileLayer>> Tilex04;
        public List<List<TileLayer>> Tilex05;
        public List<List<TileLayer>> Tilex06;
        public List<List<TileLayer>> Tilex26;
        public List<List<TileLayer>> Tilex07;
        public List<List<TileLayer>> Tilex17;
        public List<List<TileLayer>> Tilex27;
        public List<List<TileLayer>> Tilex37;
        public List<List<TileLayer>> Tilex08;
        public List<List<TileLayer>> Tilex09;
        public List<List<TileLayer>> Tilex89;
        public List<List<TileLayer>> Tilex0A;
        public List<List<TileLayer>> Tilex0B;
        public List<List<TileLayer>> Tilex1B;
        public List<List<TileLayer>> Tilex8B;
        public List<List<TileLayer>> Tilex9B;
        public List<List<TileLayer>> Tilex0C;
        public List<List<TileLayer>> Tilex4C;
        public List<List<TileLayer>> Tilex0D;
        public List<List<TileLayer>> Tilex4D;
        public List<List<TileLayer>> Tilex8D;
        public List<List<TileLayer>> TilexCD;
        public List<List<TileLayer>> Tilex0E;
        public List<List<TileLayer>> Tilex2E;
        public List<List<TileLayer>> Tilex4E;
        public List<List<TileLayer>> Tilex6E;
        public List<List<TileLayer>> Tilex0F;
        public List<List<TileLayer>> Tilex1F;
        public List<List<TileLayer>> Tilex2F;
        public List<List<TileLayer>> Tilex3F;
        public List<List<TileLayer>> Tilex4F;
        public List<List<TileLayer>> Tilex5F;
        public List<List<TileLayer>> Tilex6F;
        public List<List<TileLayer>> Tilex7F;
        public List<List<TileLayer>> Tilex8F;
        public List<List<TileLayer>> Tilex9F;
        public List<List<TileLayer>> TilexAF;
        public List<List<TileLayer>> TilexBF;
        public List<List<TileLayer>> TilexCF;
        public List<List<TileLayer>> TilexDF;
        public List<List<TileLayer>> TilexEF;
        public List<List<TileLayer>> TilexFF;

        public AutoTileAdjacent()
        {
            Tilex00 = new List<List<TileLayer>>();
            Tilex01 = new List<List<TileLayer>>();
            Tilex02 = new List<List<TileLayer>>();
            Tilex03 = new List<List<TileLayer>>();
            Tilex13 = new List<List<TileLayer>>();
            Tilex04 = new List<List<TileLayer>>();
            Tilex05 = new List<List<TileLayer>>();
            Tilex06 = new List<List<TileLayer>>();
            Tilex26 = new List<List<TileLayer>>();
            Tilex07 = new List<List<TileLayer>>();
            Tilex17 = new List<List<TileLayer>>();
            Tilex27 = new List<List<TileLayer>>();
            Tilex37 = new List<List<TileLayer>>();
            Tilex08 = new List<List<TileLayer>>();
            Tilex09 = new List<List<TileLayer>>();
            Tilex89 = new List<List<TileLayer>>();
            Tilex0A = new List<List<TileLayer>>();
            Tilex0B = new List<List<TileLayer>>();
            Tilex1B = new List<List<TileLayer>>();
            Tilex8B = new List<List<TileLayer>>();
            Tilex9B = new List<List<TileLayer>>();
            Tilex0C = new List<List<TileLayer>>();
            Tilex4C = new List<List<TileLayer>>();
            Tilex0D = new List<List<TileLayer>>();
            Tilex4D = new List<List<TileLayer>>();
            Tilex8D = new List<List<TileLayer>>();
            TilexCD = new List<List<TileLayer>>();
            Tilex0E = new List<List<TileLayer>>();
            Tilex2E = new List<List<TileLayer>>();
            Tilex4E = new List<List<TileLayer>>();
            Tilex6E = new List<List<TileLayer>>();
            Tilex0F = new List<List<TileLayer>>();
            Tilex1F = new List<List<TileLayer>>();
            Tilex2F = new List<List<TileLayer>>();
            Tilex3F = new List<List<TileLayer>>();
            Tilex4F = new List<List<TileLayer>>();
            Tilex5F = new List<List<TileLayer>>();
            Tilex6F = new List<List<TileLayer>>();
            Tilex7F = new List<List<TileLayer>>();
            Tilex8F = new List<List<TileLayer>>();
            Tilex9F = new List<List<TileLayer>>();
            TilexAF = new List<List<TileLayer>>();
            TilexBF = new List<List<TileLayer>>();
            TilexCF = new List<List<TileLayer>>();
            TilexDF = new List<List<TileLayer>>();
            TilexEF = new List<List<TileLayer>>();
            TilexFF = new List<List<TileLayer>>();

        }

        public override void AutoTileArea(ulong randSeed, Loc rectStart, Loc rectSize, Loc totalSize, PlacementMethod placementMethod, QueryMethod queryMethod)
        {
            int[][] mainArray = new int[rectSize.X][];
            for (int ii = 0; ii < rectSize.X; ii++)
            {
                mainArray[ii] = new int[rectSize.Y];
                for (int jj = 0; jj < rectSize.Y; jj++)
                    mainArray[ii][jj] = -1;
            }

            for (int xx = 0; xx < rectSize.X; xx++)
            {
                for (int yy = 0; yy < rectSize.Y; yy++)
                {
                    if (queryMethod(rectStart.X + xx, rectStart.Y + yy))
                        textureMainBlock(mainArray, rectStart, rectStart.X + xx, rectStart.Y + yy, queryMethod);
                }
            }

            ReRandom rand = new ReRandom(randSeed);
            //rand next is called for every tile up to the rectangle involved
            //there exists a jump function for rand, but not for arbitrary length
            //if the rand function changes to allow it, change this code block to jump directly to the correct values.
            for (int xx = 0; xx < rectStart.X + rectSize.X; xx++)
            {
                int yy = 0;
                for (; yy < rectStart.Y + rectSize.Y; yy++)
                {
                    ulong subSeed = rand.NextUInt64();
                    if (xx >= rectStart.X && yy >= rectStart.Y)
                    {
                        int neighborCode = mainArray[xx - rectStart.X][yy - rectStart.Y];
                        if (neighborCode != -1)
                            placementMethod(xx, yy, GetTile(new ReRandom(subSeed), neighborCode));
                    }
                }
                while (yy < totalSize.Y)
                {
                    rand.NextUInt64();
                    yy++;
                }
            }
        }


        private void textureMainBlock(int[][] textureArray, Loc rectStart, int x, int y, QueryMethod queryMethod)
        {
            int texn_um = 0;

            //  2
            // 1 3
            //  0
            bool[] blockedDirs = new bool[DirExt.DIR4_COUNT];
            for (int n = 0; n < DirExt.DIR4_COUNT; n++)
            {
                if (IsBlocked(queryMethod, x, y, ((Dir4)n).ToDir8()))
                {
                    blockedDirs[n] = true;
                    texn_um |= (1 << n);
                }
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
                    {
                        blockedQuads[n] = true;
                        texn_um |= (1 << (n + 4));
                    }
                }
            }

            textureArray[x - rectStart.X][y - rectStart.Y] = texn_um;
        }

        private List<TileLayer> GetTile(ReRandom rand, int neighborCode)
        {
            switch (neighborCode)
            {
                case 0x00:
                    return SelectTileLayers(rand, Tilex00);
                case 0x01:
                    return SelectTileLayers(rand, Tilex01);
                case 0x02:
                    return SelectTileLayers(rand, Tilex02);
                case 0x03:
                    return SelectTileLayers(rand, Tilex03);
                case 0x13:
                    return SelectTileLayers(rand, Tilex13);
                case 0x04:
                    return SelectTileLayers(rand, Tilex04);
                case 0x05:
                    return SelectTileLayers(rand, Tilex05);
                case 0x06:
                    return SelectTileLayers(rand, Tilex06);
                case 0x26:
                    return SelectTileLayers(rand, Tilex26);
                case 0x07:
                    return SelectTileLayers(rand, Tilex07);
                case 0x17:
                    return SelectTileLayers(rand, Tilex17);
                case 0x27:
                    return SelectTileLayers(rand, Tilex27);
                case 0x37:
                    return SelectTileLayers(rand, Tilex37);
                case 0x08:
                    return SelectTileLayers(rand, Tilex08);
                case 0x09:
                    return SelectTileLayers(rand, Tilex09);
                case 0x89:
                    return SelectTileLayers(rand, Tilex89);
                case 0x0A:
                    return SelectTileLayers(rand, Tilex0A);
                case 0x0B:
                    return SelectTileLayers(rand, Tilex0B);
                case 0x1B:
                    return SelectTileLayers(rand, Tilex1B);
                case 0x8B:
                    return SelectTileLayers(rand, Tilex8B);
                case 0x9B:
                    return SelectTileLayers(rand, Tilex9B);
                case 0x0C:
                    return SelectTileLayers(rand, Tilex0C);
                case 0x4C:
                    return SelectTileLayers(rand, Tilex4C);
                case 0x0D:
                    return SelectTileLayers(rand, Tilex0D);
                case 0x4D:
                    return SelectTileLayers(rand, Tilex4D);
                case 0x8D:
                    return SelectTileLayers(rand, Tilex8D);
                case 0xCD:
                    return SelectTileLayers(rand, TilexCD);
                case 0x0E:
                    return SelectTileLayers(rand, Tilex0E);
                case 0x2E:
                    return SelectTileLayers(rand, Tilex2E);
                case 0x4E:
                    return SelectTileLayers(rand, Tilex4E);
                case 0x6E:
                    return SelectTileLayers(rand, Tilex6E);
                case 0x0F:
                    return SelectTileLayers(rand, Tilex0F);
                case 0x1F:
                    return SelectTileLayers(rand, Tilex1F);
                case 0x2F:
                    return SelectTileLayers(rand, Tilex2F);
                case 0x3F:
                    return SelectTileLayers(rand, Tilex3F);
                case 0x4F:
                    return SelectTileLayers(rand, Tilex4F);
                case 0x5F:
                    return SelectTileLayers(rand, Tilex5F);
                case 0x6F:
                    return SelectTileLayers(rand, Tilex6F);
                case 0x7F:
                    return SelectTileLayers(rand, Tilex7F);
                case 0x8F:
                    return SelectTileLayers(rand, Tilex8F);
                case 0x9F:
                    return SelectTileLayers(rand, Tilex9F);
                case 0xAF:
                    return SelectTileLayers(rand, TilexAF);
                case 0xBF:
                    return SelectTileLayers(rand, TilexBF);
                case 0xCF:
                    return SelectTileLayers(rand, TilexCF);
                case 0xDF:
                    return SelectTileLayers(rand, TilexDF);
                case 0xEF:
                    return SelectTileLayers(rand, TilexEF);
                case 0xFF:
                    return SelectTileLayers(rand, TilexFF);
            }

            return new List<TileLayer>();
        }


        protected List<TileLayer> SelectTileLayers(ReRandom rand, List<List<TileLayer>> anims)
        {
            List<TileLayer> returnLayers = new List<TileLayer>();
            if (anims.Count > 0)
            {
                int index = 0;
                for (int ii = 0; ii < anims.Count - 1; ii++)
                {
                    if (rand.Next() % 2 == 0)
                        index++;
                    else
                        break;
                }
                foreach (TileLayer anim in anims[index])
                    returnLayers.Add(new TileLayer(anim));
            }
            return returnLayers;
        }
    }
}
