using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.Dungeon
{
    public interface IActionContext
    {
        Character User { get; set; }
        Character Target { get; set; }
        Loc StrikeEndTile { get; set; }
        List<Loc> StrikeLandTiles { get; set; }
        BattleActionType ActionType { get; set; }

        CombatAction HitboxAction { get; set; }
        ExplosionData Explosion { get; set; }
        BattleData Data { get; set; }

        IEnumerator<YieldInstruction> TargetTileWithExplosion(Loc target);

        IEnumerator<YieldInstruction> ProcessHitLoc(Loc loc);
    }
}
