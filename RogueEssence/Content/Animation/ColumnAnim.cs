using System;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Dev;
#if EDITORS
using System.Windows.Forms;
using RogueEssence.Dungeon;
#endif

namespace RogueEssence.Content
{
    [Serializable]
    public class ColumnAnim : BaseAnim, IEmittable
    {
        public ColumnAnim() { }
        public ColumnAnim(BeamAnimData anim) : this(anim, 1, 0) { }
        public ColumnAnim(BeamAnimData anim, int cycles) : this(anim, cycles, 0) { }
        public ColumnAnim(BeamAnimData anim, int cycles, int totalTime)
        {
            Anim = anim;
            TotalTime = totalTime;
            Cycles = cycles;
        }


        protected ColumnAnim(ColumnAnim other)
        {
            Anim = other.Anim;
            TotalTime = other.TotalTime;
            Cycles = other.Cycles;
        }
        public virtual IEmittable CloneIEmittable() { return new ColumnAnim(this); }


        [SubGroup]
        public BeamAnimData Anim;

        public int TotalTime;
        public int Cycles;

        [NonSerialized]
        public FrameTick ActionTime;


        public IEmittable CreateStatic(Loc startLoc, int startHeight, Dir8 dir)
        {
            ColumnAnim anim = (ColumnAnim)CloneIEmittable();
            anim.SetupEmitted(startLoc, startHeight, dir);
            return anim;
        }

        public virtual void SetupEmitted(Loc startLoc, int startHeight, Dir8 dir)
        {
            mapLoc = startLoc;
            locHeight = startHeight;
            SetUp();
        }
        public void SetUp()
        {
            TotalTime = Cycles > 0 ? Anim.FrameTime * Anim.GetTotalFrames(GraphicsManager.GetAttackSheet(Anim.AnimIndex).TotalFrames) * Math.Max(1, Cycles) : TotalTime;
        }



        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            ActionTime += elapsedTime;

            int totalTime = Anim.FrameTime * Anim.GetTotalFrames(GraphicsManager.GetBeam(Anim.AnimIndex).TotalFrames) * Math.Max(1, Cycles);
            if (ActionTime >= totalTime)
                finished = true;
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            if (Finished)
                return;

            Loc drawLoc = MapLoc - offset + new Loc(GraphicsManager.TileSize / 2);
            BeamSheet sheet = GraphicsManager.GetBeam(Anim.AnimIndex);
            sheet.DrawColumn(spriteBatch, new Vector2(drawLoc.X, drawLoc.Y - LocHeight), Anim.GetCurrentFrame(ActionTime, sheet.TotalFrames), Color.White * ((float)Anim.Alpha / 255));
            
        }

        public override Loc GetDrawLoc(Loc offset)
        {
            return MapLoc - offset;
        }

        public override Loc GetDrawSize()
        {
            return new Loc(GraphicsManager.TileSize);
        }



#if EDITORS
        protected override void LoadClassControls(TableLayoutPanel control)
        {
            int initialHeight = control.Height;
            base.LoadClassControls(control);

            int totalHeight = control.Height - initialHeight;

            Button btnTest = new Button();
            btnTest.Name = "btnTest";
            btnTest.Dock = DockStyle.Fill;
            btnTest.Size = new System.Drawing.Size(0, 29);
            btnTest.TabIndex = 0;
            btnTest.Text = "Test";
            btnTest.UseVisualStyleBackColor = true;
            btnTest.Click += new System.EventHandler(btnTest_Click);
            control.Controls.Add(btnTest);
        }


        protected virtual void btnTest_Click(object sender, EventArgs e)
        {
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;

                ColumnAnim data = (ColumnAnim)Activator.CreateInstance(this.GetType());
                data.SaveClassControls((TableLayoutPanel)((Button)sender).Parent);
                data.SetupEmitted(player.MapLoc, 0, player.CharDir);
                DungeonScene.Instance.CreateAnim(data, DrawLayer.Normal);
            }
        }

#endif //WINDOWS
    }
}