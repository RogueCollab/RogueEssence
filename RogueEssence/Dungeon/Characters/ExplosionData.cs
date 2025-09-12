using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;

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
    public class ExplosionData
    {
        /// <summary>
        /// Which entities are targeted by the explosion.
        /// </summary>
        public Alignment TargetAlignments;

        /// <summary>
        /// Determines if the explosion targets tiles or not.
        /// </summary>
        public bool HitTiles;

        /// <summary>
        /// The range of the explosion in Tiles.
        /// Skills that do not have a splash effect use 0.
        /// </summary>
        public int Range;

        /// <summary>
        /// Speed to Spread from a radius of 0 to a radius of the explosion's max range.
        /// In Tiles Per Second
        /// </summary>
        public int Speed;

        /// <summary>
        /// The Particle FX that plays on each tile covered by the explosion.
        /// </summary>
        public FiniteEmitter TileEmitter;

        /// <summary>
        /// The Particle FX for the explosion that scales in radius to the explosion's radius.
        /// </summary>
        public CircleSquareEmitter Emitter;

        /// <summary>
        /// VFX that plays before the explosion goes off.
        /// </summary>
        public List<BattleFX> IntroFX;

        /// <summary>
        /// VFX that plays when the explosion goes off.
        /// </summary>
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
                fxEmitter.SetupEmit(tile * GraphicsManager.TileSize + new Loc(GraphicsManager.TileSize / 2), user.MapLoc, user.CharDir);
                DungeonScene.Instance.CreateAnim(fxEmitter, DrawLayer.NoDraw);
                DungeonScene.Instance.SetScreenShake(new ScreenMover(fx.ScreenMovement));
                if (fx.AbsoluteDelay)
                    yield return new WaitForFrames(fx.Delay);
                else
                    yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(fx.Delay, tile));
            }

            GameManager.Instance.BattleSE(ExplodeFX.Sound);
            FiniteEmitter emitter = (FiniteEmitter)ExplodeFX.Emitter.Clone();
            emitter.SetupEmit(tile * GraphicsManager.TileSize + new Loc(GraphicsManager.TileSize / 2), user.MapLoc, user.CharDir);
            DungeonScene.Instance.CreateAnim(emitter, DrawLayer.NoDraw);
            DungeonScene.Instance.SetScreenShake(new ScreenMover(ExplodeFX.ScreenMovement));

            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ReleaseHitboxes(user,
                new CircleSquareHitbox(user, TargetAlignments, HitTiles, TileAlignment.None, tile, TileEmitter, Emitter, Range, Speed, ExplodeFX.Delay, Hitbox.AreaLimit.Full, user.CharDir),
                effect, hitEffect));
        }

        public IEnumerable<Loc> IterateTargetedTiles(Loc origin)
        {
            for (int ii = -Range; ii <= Range; ii++)
            {
                for (int jj = -Range; jj <= Range; jj++)
                    yield return new Loc(origin.X + ii, origin.Y + jj);
            }
        }

        public override string ToString()
        {
            return Text.FormatKey("RANGE_AREA_FULL", Range, CombatAction.GetTargetsString(true, TargetAlignments)); ;
        }
    }
}
