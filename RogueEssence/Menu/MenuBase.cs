using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public abstract class MenuBase
    {
        public const int VERT_SPACE = 14;
        public const int LINE_SPACE = 12;

        //system colors:
        //White
        //Yellow
        //Red
        //Cyan
        //Lime
        public static readonly Color TextBlue = new Color(132, 132, 255);
        public static readonly Color TextIndigo = new Color(0, 156, 255);
        public static readonly Color TextPink = new Color(255, 165, 255);
        public static readonly Color TextPale = new Color(255,206,206);
        public static readonly Color TextTan = new Color(255, 198, 99);
        
        public Rect Bounds;

        public bool Visible { get; set; }
        public static bool Transparent;
        public static int BorderStyle;
        public static int BorderFlash;

        DepthStencilState s1;
        DepthStencilState s2;
        AlphaTestEffect alphaTest;

        public MenuBase()
        {
            Visible = true;

            s1 = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                ReferenceStencil = 1,
                DepthBufferEnable = false,
            };

            s2 = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.LessEqual,
                StencilPass = StencilOperation.Keep,
                ReferenceStencil = 1,
                DepthBufferEnable = false,
            };
            alphaTest = new AlphaTestEffect(GraphicsManager.GraphicsDevice);
        }


        public abstract IEnumerable<IMenuElement> GetElements();

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;

            spriteBatch.End();
            float scale = GraphicsManager.WindowZoom;
            Matrix zoomMatrix = Matrix.CreateScale(new Vector3(scale, scale, 1));
            Matrix orthMatrix = zoomMatrix * Matrix.CreateOrthographicOffCenter(0,
                GraphicsManager.GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsManager.GraphicsDevice.PresentationParameters.BackBufferHeight,
                0, 0, 1);

            alphaTest.Projection = orthMatrix;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, s1, null, alphaTest);

            TileSheet menuBack = GraphicsManager.MenuBG;
            TileSheet menuBorder = GraphicsManager.MenuBorder;

            int addTrans = Transparent ? 3 : 0;

            DrawMenuPiece(spriteBatch, menuBack, Color.White, 0, addTrans);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, s2, null, null, zoomMatrix);

            //draw Texts
            foreach (IMenuElement element in GetElements())
                element.Draw(spriteBatch, new Loc());

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, zoomMatrix);

            int addX = 3 * BorderStyle;
            int addY = 3 * BorderFlash;

            DrawMenuPiece(spriteBatch, menuBorder, Color.White, addX, addY);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, zoomMatrix);

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
    }
}
