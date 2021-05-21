using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AABB;
using RogueEssence.Script;
using System.Runtime.Serialization;

namespace RogueEssence.Ground
{
    [Serializable]
    public class GroundObject : BaseTaskUser, IDrawableSprite, IObstacle
    {
        //Moved script events to their own structure, to avoid duplicates and other issues
        private Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent> ScriptEvents;

        public ObjAnimData ObjectAnim;
        public bool Solid;

        public uint Tags
        {
            get
            {
                if (!EntEnabled)
                    return 0u;

                if (TriggerType == EEntityTriggerTypes.Touch)
                    return 2u;
                if (TriggerType == EEntityTriggerTypes.Action || Solid)
                    return 1u;

                return 0u;
            }
        }
        public int LocHeight { get { return 0; } }
        public Loc DrawOffset;

        public override Color DevEntColoring => Color.Chartreuse;

        public override EThink ThinkType => EThink.Never;


        public GroundObject()
        {
            ScriptEvents = new Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent>();
            ObjectAnim = new ObjAnimData();
            EntName = "GroundObject" + ToString(); //!#FIXME : Give a default unique name please fix this when we have editor/template names!
            SetTriggerType(EEntityTriggerTypes.Action);
        }
        public GroundObject(ObjAnimData anim, Rect collider, bool contact, string entname)
            : this(anim, collider, new Loc(), contact, entname)
        { }

        public GroundObject(ObjAnimData anim, Rect collider, Loc drawOffset, bool solid, EEntityTriggerTypes triggerty, string entname)
        {
            ScriptEvents = new Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent>();
            ObjectAnim = anim;
            Collider = collider;
            DrawOffset = drawOffset;
            SetTriggerType(triggerty);
            EntName = entname;
        }

        public GroundObject(ObjAnimData anim, Rect collider, EEntityTriggerTypes triggerty, string entname)
            :this(anim, collider, new Loc(), true, triggerty, entname)
        {}

        public GroundObject(ObjAnimData anim, Rect collider, Loc drawOffset, bool contact, string entname)
            : this(anim, collider, drawOffset, true, contact ? EEntityTriggerTypes.Touch : EEntityTriggerTypes.Action, entname)
        {}

        protected GroundObject(GroundObject other) : base(other)
        {
            ScriptEvents = new Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent>();
            foreach (LuaEngine.EEntLuaEventTypes ev in other.ScriptEvents.Keys)
                ScriptEvents.Add(ev, (ScriptEvent)other.ScriptEvents[ev].Clone());
            ObjectAnim = new ObjAnimData(other.ObjectAnim);
            DrawOffset = other.DrawOffset;
            Solid = other.Solid;
        }

        public override GroundEntity Clone() { return new GroundObject(this); }


        public override void DoCleanup()
        {
            foreach (var entry in ScriptEvents)
                entry.Value.DoCleanup();
            ScriptEvents.Clear();
        }

        public override IEnumerator<YieldInstruction> Interact(GroundEntity activator) //PSY: Set this value to get the entity that touched us/activated us
        {
            if (!EntEnabled)
                yield break;

            //Run script events
            if (GetTriggerType() == EEntityTriggerTypes.Action)
                yield return CoroutineManager.Instance.StartCoroutine(RunEvent(LuaEngine.EEntLuaEventTypes.Action, activator));
            else if (GetTriggerType() == EEntityTriggerTypes.Touch)
                yield return CoroutineManager.Instance.StartCoroutine(RunEvent(LuaEngine.EEntLuaEventTypes.Touch, activator));

        }

        public void DrawDebug(SpriteBatch spriteBatch, Loc offset) { }
        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            if (ObjectAnim.AnimIndex != "")
            {
                Loc drawLoc = GetDrawLoc(offset);

                DirSheet sheet = GraphicsManager.GetObject(ObjectAnim.AnimIndex);
                sheet.DrawDir(spriteBatch, drawLoc.ToVector2(), ObjectAnim.GetCurrentFrame(GraphicsManager.TotalFrameTick, sheet.TotalFrames), ObjectAnim.GetDrawDir(Dir8.None), Color.White);
            }
        }


        public Loc GetDrawLoc(Loc offset)
        {
            return MapLoc - offset - DrawOffset;
        }

        public Loc GetDrawSize()
        {
            DirSheet sheet = GraphicsManager.GetObject(ObjectAnim.AnimIndex);

            return new Loc(sheet.TileWidth, sheet.TileHeight);
        }

        public override EEntTypes GetEntityType()
        {
            return EEntTypes.Object;
        }

        public override bool DevHasGraphics()
        {
            if (ObjectAnim != null && ObjectAnim.AnimIndex != "")
                return true;
            else
                return false;
        }

        public override void ReloadEvents()
        {
            foreach (var entry in ScriptEvents)
                entry.Value.ReloadEvent();
        }

        public override bool HasScriptEvent(LuaEngine.EEntLuaEventTypes ev)
        {
            return ScriptEvents.ContainsKey(ev);
        }

        public override void SyncScriptEvents()
        {
            foreach (var ev in ScriptEvents.Keys)
                ScriptEvents[ev].SetLuaFunctionPath(LuaEngine.MakeLuaEntityCallbackName(EntName, ev));
        }

        public override bool IsEventSupported(LuaEngine.EEntLuaEventTypes ev)
        {
            return ev != LuaEngine.EEntLuaEventTypes.Invalid && ev != LuaEngine.EEntLuaEventTypes.Think;
        }

        public override void AddScriptEvent(LuaEngine.EEntLuaEventTypes ev)
        {
            DiagManager.Instance.LogInfo(String.Format("GroundObject.AddScriptEvent({0}): Added script event to {1}!", ev.ToString(), EntName));
            if (!IsEventSupported(ev))
                return;
            ScriptEvents.Add(ev, new ScriptEvent(LuaEngine.MakeLuaEntityCallbackName(EntName, ev)));
        }

        public override void LuaEngineReload()
        {
            foreach (ScriptEvent scriptEvent in ScriptEvents.Values)
                scriptEvent.LuaEngineReload();
        }

        public override void RemoveScriptEvent(LuaEngine.EEntLuaEventTypes ev)
        {
            DiagManager.Instance.LogInfo(String.Format("GroundObject.RemoveScriptEvent({0}): Removed script event from {1}!", ev.ToString(), EntName));

            if (ScriptEvents.ContainsKey(ev))
                ScriptEvents.Remove(ev);
        }

        public override IEnumerator<YieldInstruction> RunEvent(LuaEngine.EEntLuaEventTypes ev, params object[] parameters)
        {
            //Since ScriptEvent.Apply takes a single variadic table, we have to concatenate our current variadic argument table
            // with the extra parameter we want to pass. Otherwise "parameters" will be passed as a table instead of its
            // individual elements, and things will crash left and right.
            List<object> partopass = new List<object>();
            partopass.Add(this);
            partopass.AddRange(parameters);

            if (ScriptEvents.ContainsKey(ev))
                yield return CoroutineManager.Instance.StartCoroutine(ScriptEvents[ev].Apply(partopass.ToArray()));
            else
                yield break;
        }
    }
}
