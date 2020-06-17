using System;
#if EDITORS
using System.Windows.Forms;
#endif
using RogueEssence.Dungeon;

namespace RogueEssence.Content
{
    [Serializable]
    public class BattleFX : Dev.EditorData
    {
        public int Delay;

        [Dev.Sound(0)]
        public string Sound;

        [Dev.SubGroup]
        public FiniteEmitter Emitter;

        [Dev.SubGroup]
        public ScreenMover ScreenMovement;

        public BattleFX()
        {
            Emitter = new EmptyFiniteEmitter();
            ScreenMovement = new ScreenMover();
            Sound = "";
        }
        public BattleFX(FiniteEmitter emitter, string sound, int delay)
        {
            Emitter = emitter;
            Sound = sound;
            Delay = delay;
            ScreenMovement = new ScreenMover();
        }

        public BattleFX(BattleFX other)
        {
            Delay = other.Delay;
            Emitter = (FiniteEmitter)other.Emitter.Clone();
            ScreenMovement = new ScreenMover(other.ScreenMovement);
            Sound = other.Sound;
        }


        public override string ToString()
        {
            string result = Emitter.ToString();
            if (Sound != "")
                result += ", SE:" + Sound;
            if (Delay > 0)
                result += " +" + Delay;
            return result;
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




        private void btnTest_Click(object sender, EventArgs e)
        {
            if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
            {
                Character player = DungeonScene.Instance.FocusedCharacter;

                BattleFX data = new BattleFX();
                data.SaveClassControls((TableLayoutPanel)((Button)sender).Parent);

                DungeonScene.Instance.PendingDevEvent = DungeonScene.Instance.ProcessBattleFX(player, player, data);
            }
        }

#endif //WINDOWS

    }

}
