using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Data;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class EffectTile : GameEventOwner, IDrawableSprite, ISpawnable
    {
        public override GameEventPriority.EventCause GetEventCause()
        {
            return GameEventPriority.EventCause.Tile;
        }

        public enum TileOwner
        {
            None,
            Player,
            Enemy
        }

        public override int GetID() { return ID; }
        public TileData GetData() { return DataManager.Instance.GetTile(ID); }
        public override string GetDisplayName() { return GetData().GetColoredName(); }


        public bool Exposed { get { return true; } }
        public bool Revealed;
        public bool Danger;
        public TileOwner Owner;

        public int ID;

        public StateCollection<TileState> TileStates;

        //[NonSerialized]
        //redundant, but no need to remove from serialization...
        public Loc TileLoc { get; private set; }
        public Loc MapLoc { get { return TileLoc * GraphicsManager.TileSize; } }
        public int LocHeight { get { return 0; } }

        public EffectTile()
        {
            ID = -1;
            TileStates = new StateCollection<TileState>();
        }
        public EffectTile(int index, bool revealed) : this()
        {
            ID = index;
            Revealed = revealed;
        }
        public EffectTile(int index, bool revealed, Loc loc) : this(index, revealed)
        {
            TileLoc = loc;
        }
        public EffectTile(Loc loc)
        {
            ID = -1;
            TileStates = new StateCollection<TileState>();
            TileLoc = loc;
        }
        public EffectTile(EffectTile other)
        {
            ID = other.ID;
            Revealed = other.Revealed;
            Danger = other.Danger;
            TileStates = other.TileStates.Clone();
            TileLoc = other.TileLoc;
        }
        public EffectTile(EffectTile other, Loc loc)
        {
            ID = other.ID;
            Revealed = other.Revealed;
            Danger = other.Danger;
            TileStates = other.TileStates.Clone();
            TileLoc = loc;
        }
        public ISpawnable Copy() { return new EffectTile(this); }

        public void UpdateTileLoc(Loc loc)
        {
            TileLoc = loc;
        }


        public IEnumerator<YieldInstruction> InteractWithTile(Character character)
        {
            DungeonScene.EventEnqueueFunction<SingleCharEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<SingleCharEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                TileData entry = DataManager.Instance.GetTile(ID);
                AddEventsToQueue<SingleCharEvent>(queue, maxPriority, ref nextPriority, entry.InteractWithTiles, character);
            };
            foreach (EventQueueElement<SingleCharEvent> effect in DungeonScene.IterateEvents<SingleCharEvent>(function))
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, character));
        }

        public IEnumerator<YieldInstruction> LandedOnTile(Character character)
        {
            DungeonScene.EventEnqueueFunction<SingleCharEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<SingleCharEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                TileData entry = DataManager.Instance.GetTile(ID);
                AddEventsToQueue<SingleCharEvent>(queue, maxPriority, ref nextPriority, entry.LandedOnTiles, character);
            };
            foreach (EventQueueElement<SingleCharEvent> effect in DungeonScene.IterateEvents<SingleCharEvent>(function))
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, character));
        }

        public void DrawDebug(SpriteBatch spriteBatch, Loc offset) { }
        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            Loc drawLoc = GetDrawLoc(offset);
            TileData entry = DataManager.Instance.GetTile(ID);
            if (!Revealed) //draw texture
                entry = DataManager.Instance.GetTile(0);

            if (entry.Anim.AnimIndex != "")
            {
                DirSheet sheet = GraphicsManager.GetObject(entry.Anim.AnimIndex);
                sheet.DrawDir(spriteBatch, drawLoc.ToVector2(), entry.Anim.GetCurrentFrame(GraphicsManager.TotalFrameTick, sheet.TotalFrames), entry.Anim.AnimDir, Color.White * ((Owner == TileOwner.Player) ? 0.70f : 1f));
            }
        }
        

        public Loc GetDrawLoc(Loc offset)
        {
            TileData entry = DataManager.Instance.GetTile(ID);
            if (!Revealed)
                entry = DataManager.Instance.GetTile(0);
            DirSheet sheet = GraphicsManager.GetObject(entry.Anim.AnimIndex);

            return new Loc(MapLoc.X + GraphicsManager.TileSize / 2 - sheet.TileWidth / 2,
            MapLoc.Y + GraphicsManager.TileSize / 2 - sheet.TileHeight / 2) - offset;
        }

        public Loc GetDrawSize()
        {
            TileData entry = DataManager.Instance.GetTile(ID);
            if (!Revealed)
                entry = DataManager.Instance.GetTile(0);
            DirSheet sheet = GraphicsManager.GetObject(entry.Anim.AnimIndex);

            return new Loc(sheet.TileWidth, sheet.TileHeight);
        }

        public override string ToString()
        {
            string local = (ID > -1) ? DataManager.Instance.DataIndices[DataManager.DataType.Tile].Entries[ID].Name.ToLocal() : "NULL";
            return string.Format("{0}: {1}", this.GetType().Name, local);
        }
    }
}
