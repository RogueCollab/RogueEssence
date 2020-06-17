using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
#if EDITORS
using System.Windows.Forms;
#endif

namespace RogueEssence.Dungeon
{
    //note: preliminary hitboxes only need to keep track of which tiles they're meant to hit
    //then they ALL spawn an explosion that affects a target with specified alignments
    //in other words, the first hitbox only decides what to explode on
    //the explosion's alignments decide what to hit
    //keep in mind: there is at most only one explode condition, so parhaps it would be okay to have all hitboxes cause "explosions"

    //NOTE: there is still some strangeness that will happen when boomerang and explosion combine: tiles and enemies that are "returned" to are hit
    //regardless of whether the character or wall is there anymore
    [Serializable]
    public class ExplosionData : Dev.EditorData
    {
        public Alignment TargetAlignments;

        public bool HitTiles;

        /// <summary>
        /// In Tiles
        /// </summary>
        public int Range;

        /// <summary>
        /// Speed to Spread from 0 to Range in Tiles Per Second
        /// </summary>
        public int Speed;

        public FiniteEmitter TileEmitter;


        public CircleSquareEmitter Emitter;

        public List<BattleFX> IntroFX;

        public BattleFX ExplodeFX;

        public ExplosionData()
        {
            ExplodeFX = new BattleFX();
            IntroFX = new List<BattleFX>();
            Emitter = new EmptyCircleSquareEmitter();
            TileEmitter = new EmptyFiniteEmitter();
        }
        public ExplosionData(ExplosionData other)
        {
            TargetAlignments = other.TargetAlignments;
            HitTiles = other.HitTiles;
            Range = other.Range;
            Speed = other.Speed;
            ExplodeFX = new BattleFX(other.ExplodeFX);
            IntroFX = new List<BattleFX>();
            foreach (BattleFX fx in other.IntroFX)
                IntroFX.Add(new BattleFX(fx));
            Emitter = (CircleSquareEmitter)other.Emitter.Clone();
            TileEmitter = (FiniteEmitter)other.TileEmitter.Clone();
        }

        public IEnumerator<YieldInstruction> ReleaseExplosion(Loc tile, Character user, DungeonScene.HitboxEffect effect, DungeonScene.HitboxEffect hitEffect)
        {
            foreach (BattleFX fx in IntroFX)
            {
                //play sound
                GameManager.Instance.BattleSE(fx.Sound);
                //the animation
                FiniteEmitter fxEmitter = (FiniteEmitter)fx.Emitter.Clone();
                fxEmitter.SetupEmit(tile * GraphicsManager.TileSize, user.MapLoc, user.CharDir);
                DungeonScene.Instance.CreateAnim(fxEmitter, DrawLayer.NoDraw);
                DungeonScene.Instance.SetScreenShake(new ScreenMover(fx.ScreenMovement));
                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(fx.Delay, tile));
            }

            GameManager.Instance.BattleSE(ExplodeFX.Sound);
            FiniteEmitter emitter = (FiniteEmitter)ExplodeFX.Emitter.Clone();
            emitter.SetupEmit(tile * GraphicsManager.TileSize, user.MapLoc, user.CharDir);
            DungeonScene.Instance.CreateAnim(emitter, DrawLayer.NoDraw);
            DungeonScene.Instance.SetScreenShake(new ScreenMover(ExplodeFX.ScreenMovement));

            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ReleaseHitboxes(user,
                new CircleSquareHitbox(user, TargetAlignments, HitTiles, TileAlignment.None, tile, TileEmitter, Emitter, Range, Speed, ExplodeFX.Delay, Hitbox.AreaLimit.Full, user.CharDir),
                effect, hitEffect));
        }

        public void AddTargetedTiles(Loc origin, HashSet<Loc> hitTiles)
        {
            for (int ii = -Range; ii <= Range; ii++)
            {
                for (int jj = -Range; jj <= Range; jj++)
                    hitTiles.Add(new Loc(origin.X + ii, origin.Y + jj));
            }
        }

        public override string ToString()
        {
            return Text.FormatKey("RANGE_AREA_FULL", Range, CombatAction.GetTargetsString(true, TargetAlignments)); ;
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

                ExplosionData data = new ExplosionData();
                data.SaveClassControls((TableLayoutPanel)((Button)sender).Parent);

                DungeonScene.Instance.PendingDevEvent = data.ReleaseExplosion(player.CharLoc, player, DungeonScene.Instance.MockHitLoc, DungeonScene.Instance.MockHitLoc);
            }
        }

#endif //WINDOWS

    }

}
