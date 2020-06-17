using System;
#if EDITORS
using System.Windows.Forms;
#endif
using RogueEssence.Dungeon;

namespace RogueEssence.Data
{
    [Serializable]
    public class SkillData : Dev.EditorData, IDescribedData
    {
        public override string ToString()
        {
            return Name.DefaultText;
        }

        public LocalText Name { get; set; }

        [Dev.Multiline(0)]
        public LocalText Desc { get; set; }

        public bool Released { get; set; }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }


        public int BaseCharges;

        [Dev.NumberRange(0, 1, Int32.MaxValue)]
        public int Strikes;
        public CombatAction HitboxAction;
        public ExplosionData Explosion;

        public BattleData Data;


        public SkillData()
        {
            Name = new LocalText();
            Desc = new LocalText();
            Comment = "";

            Data = new BattleData();
            Explosion = new ExplosionData();

            Strikes = 1;
            HitboxAction = new AttackAction();
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

                SkillData data = new SkillData();
                data.SaveClassControls((TableLayoutPanel)((Button)sender).Parent);

                DungeonScene.Instance.PendingDevEvent = player.MockCharAction(data);
            }
        }

#endif //WINDOWS
    }


}
