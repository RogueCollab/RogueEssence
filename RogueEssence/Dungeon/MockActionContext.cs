using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.Dungeon
{
    public class MockActionContext : IActionContext
    {
        public Character User { get; set; }
        public Character Target { get; set; }
        public Loc StrikeEndTile { get; set; }
        public List<Loc> StrikeLandTiles { get; set; }
        public BattleActionType ActionType { get; set; }

        public CombatAction HitboxAction { get; set; }
        public ExplosionData Explosion { get; set; }
        public BattleData Data { get; set; }

        public MockActionContext(Character user, CombatAction hitboxAction)
        {
            User = user;
            StrikeLandTiles = new List<Loc>();
            HitboxAction = hitboxAction;
        }

        public MockActionContext(Character user, CombatAction hitboxAction, ExplosionData explosion, BattleData data)
        {
            User = user;
            StrikeLandTiles = new List<Loc>();
            HitboxAction = hitboxAction;
            Explosion = explosion;
            Data = data;
        }

        public IEnumerator<YieldInstruction> TargetTileWithExplosion(Loc target)
        {
            //release explosion
            yield return CoroutineManager.Instance.StartCoroutine(Explosion.ReleaseExplosion(target, User, ProcessHitLoc, ProcessHitTile));
        }

        public IEnumerator<YieldInstruction> ProcessHitLoc(Loc loc)
        {
            Character charTarget = ZoneManager.Instance.CurrentMap.GetCharAtLoc(loc);
            if (charTarget != null && DungeonScene.Instance.IsTargeted(User, charTarget, Explosion.TargetAlignments))
            {
                Target = charTarget;
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessEndAnim(User, Target, Data));//hit the character
            }
        }

        public IEnumerator<YieldInstruction> ProcessHitTile(Loc loc)
        {
            yield break;
        }
    }
}
