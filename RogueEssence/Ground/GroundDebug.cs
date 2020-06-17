using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueElements;
using C3.XNA;
using RogueEssence.Dev;

namespace RogueEssence.Ground
{

    /// <summary>
    /// Class with handy methods for drawing debug and editor stuff.
    /// </summary>
    class GroundDebug
    {
        public Color DrawColor { get; set; }
        public SpriteBatch Batch { get; set; }
        public float LineThickness { get; set; }

        public GroundDebug(SpriteBatch bat, Color drawcolor)
        {
            Batch = bat;
            DrawColor = drawcolor;
            LineThickness = 1;
        }

        Vector2 ProjectToScreen( Vector2 pos )
        {
            Rect viewRect = GroundEditScene.Instance.GetViewRectangle();
            float windowScale = GroundEditScene.Instance.GetWindowScale();
            return new Vector2(((pos.X - viewRect.X) * windowScale), (pos.Y - viewRect.Y) * windowScale);
        }

        Vector2 ProjectToScreen(int X, int Y)
        {
            Rect viewRect = GroundEditScene.Instance.GetViewRectangle();
            float windowScale = GroundEditScene.Instance.GetWindowScale();
            return new Vector2(((X - viewRect.X) * windowScale), (Y - viewRect.Y) * windowScale);
        }

        Rectangle ProjectToScreen(Rect bounds)
        {
            return ProjectToScreen(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        Rectangle ProjectToScreen(Rectangle bounds)
        {
            return ProjectToScreen(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        Rectangle ProjectToScreen(int x, int y, int w, int h)
        {
            Rect viewRect = GroundEditScene.Instance.GetViewRectangle();
            float windowScale = GroundEditScene.Instance.GetWindowScale();
            return new Rectangle((int)((x - viewRect.X) /** windowScale*/),
                                  (int)((y - viewRect.Y) /** windowScale*/),
                                  (int)(w /** windowScale*/),
                                  (int)(h /** windowScale*/));
        }

        public void SetRGBDrawColor(byte r, byte g, byte b, byte a)
        {
            DrawColor = new Color(r, g, b, a);

        }

        public void DrawBox( int x, int y, int w, int h )
        {
            Batch.DrawRectangle(ProjectToScreen(x, y, w, h), DrawColor, LineThickness);
        }

        public void DrawBox(Rect bounds)
        {
            Batch.DrawRectangle(ProjectToScreen(bounds), DrawColor, LineThickness);
        }

        public void DrawFilledBox(Rect bounds, byte fillingalpha)
        {
            DrawBox(bounds);
            Color fillingcol = DrawColor;
            fillingcol.A = fillingalpha;
            Batch.FillRectangle(ProjectToScreen(bounds), fillingcol);
        }
    }
}
