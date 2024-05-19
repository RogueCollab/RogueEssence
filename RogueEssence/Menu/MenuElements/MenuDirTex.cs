using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class MenuDirTex : IMenuElement
    {
        public enum TexType
        {
            Item,
            Particle,
            Icon,
            Object,
            BG
        }

        public string Label { get; set; }
        public Loc Loc;
        public TexType Type;
        public AnimData Anim;

        public MenuDirTex(Loc loc, TexType type, AnimData texture)
        {
            Loc = loc;
            Type = type;
            Anim = texture;
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            DirSheet sheet = null;
            switch (Type)
            {
                case TexType.Item:
                    sheet = GraphicsManager.GetItem(Anim.AnimIndex);
                    break;
                case TexType.Particle:
                    sheet = GraphicsManager.GetAttackSheet(Anim.AnimIndex);
                    break;
                case TexType.Icon:
                    sheet = GraphicsManager.GetIcon(Anim.AnimIndex);
                    break;
                case TexType.Object:
                    sheet = GraphicsManager.GetObject(Anim.AnimIndex);
                    break;
                case TexType.BG:
                    sheet = GraphicsManager.GetBackground(Anim.AnimIndex);
                    break;
            }

            Loc drawLoc = Loc + offset;

            if (sheet != null)
                sheet.DrawDir(spriteBatch, new Vector2(drawLoc.X, drawLoc.Y), Anim.GetCurrentFrame(GraphicsManager.TotalFrameTick, sheet.TotalFrames), Anim.GetDrawDir(Dir8.None), Color.White);

        }
    }
}
