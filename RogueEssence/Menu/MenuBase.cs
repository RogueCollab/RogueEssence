using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public abstract class MenuBase : ILabeled
    {
        public const int VERT_SPACE = 14;
        public const int LINE_HEIGHT = 12;

        //system colors:
        //White #FFFFFF
        //Yellow #FFFF00
        //Red #FF0000
        //Cyan #00FFFF
        //Lime #00FF00
        public static readonly Color TextBlue = new Color(132, 132, 255); // #8484FF
        public static readonly Color TextIndigo = new Color(0, 156, 255); // #009CFF
        public static readonly Color TextPink = new Color(255, 165, 255); // #FFA5FF
        public static readonly Color TextPale = new Color(255,206,206); // #FFCEFF
        public static readonly Color TextTan = new Color(255, 198, 99); // #FFC663

        public virtual string Label { get; protected set; }
        public Rect Bounds;

        public bool Visible { get; set; }
        public static bool Transparent;
        public static int BorderStyle;
        public static int BorderFlash;

        public bool HasLabel()
        {
            return !string.IsNullOrEmpty(Label);
        }
        public bool LabelContains(string substr)
        {
            return HasLabel() && Label.Contains(substr);
        }

        public MenuBase()
        {
            Label = "";
            Visible = true;

            elements = new List<IMenuElement>();
        }

        // TODO: set to private when deprecated setters are removed.
        protected List<IMenuElement> elements;
        public virtual List<IMenuElement> Elements { get { return elements; } }


        /// <summary>
        /// Returns an iterator of all elements for the purpose of drawing.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<IMenuElement> GetDrawElements()
        {
            foreach (IMenuElement element in Elements)
                yield return element;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;

            spriteBatch.End();
            float scale = GraphicsManager.WindowZoom;
            Matrix zoomMatrix = Matrix.CreateScale(new Vector3(scale, scale, 1));
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, GraphicsManager.MenuPreStencil, null, GraphicsManager.MenuAlpha);

            TileSheet menuBack = GraphicsManager.MenuBG;
            TileSheet menuBorder = GraphicsManager.MenuBorder;

            int addTrans = Transparent ? 3 : 0;

            DrawMenuPiece(spriteBatch, menuBack, Color.White, 0, addTrans);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, GraphicsManager.MenuPostStencil, null, null, zoomMatrix);

            //draw Texts
            foreach (IMenuElement element in GetDrawElements())
                element.Draw(spriteBatch, Bounds.Start);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, zoomMatrix);

            int addX = 3 * BorderStyle;
            int addY = 3 * BorderFlash;

            DrawMenuPiece(spriteBatch, menuBorder, Color.White, addX, addY);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, zoomMatrix);
        }

        public virtual bool GetRelativeMouseLoc(Loc screenLoc, out MenuBase menu, out Loc? relativeLoc)
        {
            menu = null;
            relativeLoc = null;

            if (!Visible)
                return false;

            screenLoc /= GraphicsManager.WindowZoom;

            if (Bounds.Contains(screenLoc))
            {
                menu = this;
                relativeLoc = screenLoc - Bounds.Start;
                return true;
            }

            return false;
        }

        private void DrawMenuPiece(SpriteBatch spriteBatch, TileSheet menu, Color color, int addX, int addY)
        {
            //draw background
            //top-left
            menu.DrawTile(spriteBatch, new Vector2(Bounds.X, Bounds.Y), addX, addY, color);
            //top-right
            menu.DrawTile(spriteBatch, new Vector2(Bounds.End.X - menu.TileWidth, Bounds.Y), addX + 2, addY, color);
            //bottom-right
            menu.DrawTile(spriteBatch, new Vector2(Bounds.End.X - menu.TileWidth, Bounds.End.Y - menu.TileHeight), addX + 2, addY + 2, color);
            //bottom-left
            menu.DrawTile(spriteBatch, new Vector2(Bounds.X, Bounds.End.Y - menu.TileHeight), addX, addY + 2, color);

            //top
            menu.DrawTile(spriteBatch, new Rectangle(Bounds.X + menu.TileWidth, Bounds.Y, Bounds.End.X - Bounds.X - 2 * menu.TileWidth, menu.TileHeight), addX + 1, addY, color);

            //right
            menu.DrawTile(spriteBatch, new Rectangle(Bounds.End.X - menu.TileWidth, Bounds.Y + menu.TileHeight, menu.TileWidth, Bounds.End.Y - Bounds.Y - 2 * menu.TileHeight), addX + 2, addY + 1, color);

            //bottom
            menu.DrawTile(spriteBatch, new Rectangle(Bounds.X + menu.TileWidth, Bounds.End.Y - menu.TileHeight, Bounds.End.X - Bounds.X - 2 * menu.TileWidth, menu.TileHeight), addX + 1, addY + 2, color);

            //left
            menu.DrawTile(spriteBatch, new Rectangle(Bounds.X, Bounds.Y + menu.TileHeight, menu.TileWidth, Bounds.End.Y - Bounds.Y - 2 * menu.TileHeight), addX, addY + 1, color);

            //center
            menu.DrawTile(spriteBatch, new Rectangle(Bounds.X + menu.TileWidth, Bounds.Y + menu.TileHeight, Bounds.End.X - Bounds.X - 2 * menu.TileWidth, Bounds.End.Y - Bounds.Y - 2 * menu.TileHeight), addX + 1, addY + 1, color);
        }


        public int GetElementIndexByLabel(string label)
        {
            return GetElementIndicesByLabel(label)[label];
        }
        public virtual Dictionary<string, int> GetElementIndicesByLabel(params string[] labels)
        {
            return SearchLabels(labels, Elements);
        }

        public static Dictionary<string, int> SearchLabels(string[] labels, IEnumerable<ILabeled> list)
        {
            Dictionary<string, int> indices = new Dictionary<string, int>();
            int totalFound = 0;
            int ii = 0;
            foreach (string label in labels)
                indices.Add(label, -1);

            foreach (ILabeled element in list)
            {
                int curIndex;
                if (element.HasLabel() && indices.TryGetValue(element.Label, out curIndex))
                {
                    // case for duplicate labels somehow; only get the first index found
                    if (curIndex == -1)
                    {
                        indices[element.Label] = ii;
                        totalFound++;

                        // short-circuit case for having found all indices
                        if (totalFound == indices.Count)
                            return indices;
                    }
                }
                ii++;
            }
            return indices;
        }
    }
}
