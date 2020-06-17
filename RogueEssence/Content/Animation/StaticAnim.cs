using System;
using RogueElements;
#if EDITORS
using System.Windows.Forms;
using RogueEssence.Dungeon;
#endif

namespace RogueEssence.Content
{
    [Serializable]
    public class StaticAnim : LoopingAnim, IEmittable
    {
        public StaticAnim() { }
        public StaticAnim(AnimData anim) : this(anim, 1, 0) { }
        public StaticAnim(AnimData anim, int cycles) : this(anim, cycles, 0) { }
        public StaticAnim(AnimData anim, int cycles, int totalTime)
            : base(anim, totalTime, cycles) { }


        protected StaticAnim(StaticAnim other) : base(other) { }
        public virtual IEmittable CloneIEmittable() { return new StaticAnim(this); }

        public IEmittable CreateStatic(Loc startLoc, int startHeight, Dir8 dir)
        {
            StaticAnim anim = (StaticAnim)CloneIEmittable();
            anim.SetupEmitted(startLoc, startHeight, dir);
            return anim;
        }

        public virtual void SetupEmitted(Loc startLoc, int startHeight, Dir8 dir)
        {
            mapLoc = startLoc;
            locHeight = startHeight;
            Direction = dir;
            SetUp();
        }


#if EDITORS
        protected override void LoadClassControls(TableLayoutPanel control)
        {
            int initialHeight = control.Height;
            base.LoadClassControls(control);

            int totalHeight = control.Height - initialHeight;

            Button btnTest = new System.Windows.Forms.Button();
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

                StaticAnim data = (StaticAnim)Activator.CreateInstance(this.GetType());
                data.SaveClassControls((TableLayoutPanel)((Button)sender).Parent);
                data.SetupEmitted(player.MapLoc, 0, player.CharDir);
                DungeonScene.Instance.CreateAnim(data, DrawLayer.Normal);
            }
        }

#endif //WINDOWS
    }

}