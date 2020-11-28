using System;
using System.Collections.Generic;
using RogueEssence.Data;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using RogueEssence.Dungeon;
using RogueEssence.Script;
using AABB;
using System.Linq;

namespace RogueEssence.Ground
{

    [Serializable]
    public class GroundChar : GroundAIUser, ICharSprite, IBox
    {
        public CharData Data;
        public MonsterID CurrentForm { get { return Data.BaseForm; } }
        public string Nickname { get { return Data.Nickname; } }

        [NonSerialized]
        public IWorld Map;

        [NonSerialized]
        public GameAction CurrentCommand;

        [NonSerialized]
        private GroundAction currentCharAction;

        /// <summary>
        /// Just a little test, to see if we couldn't store script event this way. It would avoid accidental duplicates and
        /// a lot of issues..
        /// </summary>
        public Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent> ScriptEvents;


        public uint Tags
        {
            get
            {
                if (!EntEnabled)
                    return 0u;

                return 1u;
            }
        }

        public override Color DevEntColoring => Color.Aqua;
        public override EThink ThinkType => EThink.Always;


        //DRAWING LOGIC

        const int STATUS_FRAME_LENGTH = 5;

        [NonSerialized]
        private Emote currentEmote;

        private Loc drawOffset { get { return currentCharAction.DrawOffset; } }
        public override Loc MapLoc
        {
            get { return currentCharAction.MapLoc; }
        }
        public override Loc Position
        {
            get { return currentCharAction.MapLoc; }
            set { currentCharAction.MapLoc = value; }
        }

        public override void SetMapLoc(Loc loc)
        {
            Rect orig = currentCharAction.Collider;
            currentCharAction.MapLoc = loc;
            this.Map.Update(this, orig);
        }

        public int LocHeight { get { return currentCharAction.LocHeight; } }

        //determining direction
        public Dir8 CharDir
        {
            get { return currentCharAction.CharDir; }
            set { currentCharAction.CharDir = value; }
        }

        public override Dir8 Direction
        {
            get { return currentCharAction.CharDir; }
            set { currentCharAction.CharDir = value; }
        }

        public override Rect Bounds
        {
            get { return currentCharAction.Collider; }
        }


        public GroundChar() : this(new MonsterID(), new Loc(), Dir8.Down, "GroundChar")
        {
            //!#FIXME : Give a default unique name please fix this when we have editor/template names!
        }

        public GroundChar(CharData baseChar, Loc newLoc, Dir8 charDir, string instancename)
        {
            Data = baseChar;
            //Events = new List<GroundEvent>();
            //ThinkEvents = new List<GroundEvent>();

            currentCharAction = new IdleGroundAction(newLoc, charDir);
            CurrentCommand = new GameAction(GameAction.ActionType.None, Dir8.None);
            EntName = instancename;
            TriggerType = EEntityTriggerTypes.Action;
            ScriptEvents = new Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent>();

            //By default all groundcharacters think
            AIEnabled = true;
            IsInteracting = false;
        }

        public GroundChar(MonsterID appearance, Loc newLoc, Dir8 charDir, string instancename) : this(appearance, newLoc, charDir, "", instancename)
        {

        }
        public GroundChar(MonsterID appearance, Loc newLoc, Dir8 charDir, string nickname, string instancename) : this(new CharData(), newLoc, charDir, instancename)
        {
            Data.BaseForm = appearance;
            Data.Nickname = nickname;
        }
        protected GroundChar(GroundChar other) : base(other)
        {
            ScriptEvents = new Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent>();
            foreach (LuaEngine.EEntLuaEventTypes ev in other.ScriptEvents.Keys)
                ScriptEvents.Add(ev, (ScriptEvent)other.ScriptEvents[ev].Clone());
            Data = new CharData(other.Data);

            currentCharAction = new IdleGroundAction(Loc.Zero, Dir8.Down);
            CurrentCommand = new GameAction(GameAction.ActionType.None, Dir8.None);
        }
        public override GroundEntity Clone() { return new GroundChar(this); }


        public void StartEmote(EmoteData data, int cycles)
        {
            if (data == null)
                currentEmote = null;
            else
                currentEmote = new Emote(data.Anim, data.LocHeight, cycles);
        }

        public void StartAction(GroundAction action)
        {
            action.Begin(CurrentForm);
            currentCharAction = action;

            UpdateFrame();
        }

        //Psy's notes: needed that to be able to replace the current action and set it back after we're done turning and etc..
        public GroundAction GetCurrentAction()
        {
            return currentCharAction;
        }

        public Loc GetFront()
        {
            DirH horiz;
            DirV vert;
            CharDir.Separate(out horiz, out vert);

            Loc resultLoc = Bounds.Center;
            if (horiz == DirH.Left)
                resultLoc.X = Bounds.Left;
            if (horiz == DirH.Right)
                resultLoc.X = Bounds.Right;

            if (vert == DirV.Up)
                resultLoc.Y = Bounds.Top;
            if (vert == DirV.Down)
                resultLoc.Y = Bounds.Bottom;

            return resultLoc;
        }


        public IMovement Simulate(int x, int y, Func<ICollision, ICollisionResponse> filter)
        {
            return Map.Simulate(this, x, y, filter);
        }

        public IMovement Move(int x, int y, Func<ICollision, ICollisionResponse> filter)
        {
            var movement = this.Simulate(x, y, filter);
            currentCharAction.MapLoc = new Loc(movement.Destination.X, movement.Destination.Y);
            this.Map.Update(this, movement.Origin);
            return movement;
        }


        /// <summary>
        /// Update for editor view
        /// </summary>
        /// <param name="elapsedTime"></param>
        public void UpdateView(FrameTick elapsedTime)
        {
            //the update method can only specify its own relative movement
            currentCharAction.UpdateTime(elapsedTime);
            currentCharAction.UpdateFrame();
        }

        //first pass: determine nextaction via everyone's inputs, change to that next action where applicable
        //second pass: and then update, check for next action, change to next action where applicable, checking for ties

        public void Update(FrameTick elapsedTime)
        {
            //the update method can only specify its own relative movement
            currentCharAction.UpdateTime(elapsedTime);

            currentCharAction.UpdateInput(CurrentCommand);
            CurrentCommand = new GameAction(GameAction.ActionType.None, Dir8.None);

            //transition to the new action
            if (currentCharAction.NextAction != null)
            {
                currentCharAction.NextAction.Begin(CurrentForm);
                currentCharAction = currentCharAction.NextAction;
            }

            //prepare movement
            currentCharAction.Update(elapsedTime);

            if (currentEmote != null)
                currentEmote.Update(GroundScene.Instance, elapsedTime);

        }

        public void Collide()
        {
            //move the character with collision check
            IMovement move = Move(MapLoc.X + currentCharAction.Move.X, MapLoc.Y + currentCharAction.Move.Y, basicCollision);

            //process the collisions and determine if a new action is needed (here, it can be forced)
            if (GroundScene.Instance.FocusedCharacter == this)
            {
                IHit first = move.Hits.FirstOrDefault((c) => c.Box.Tags == 2);
                if (first != null)
                    GroundScene.Instance.PendingLeaderAction = ((GroundObject)first.Box).Interact(this);
            }

            UpdateFrame();
        }

        private ICollisionResponse basicCollision(ICollision collision)
        {
            if (DataManager.Instance.Save.CutsceneMode)
                return null;

            if (collision.Other.Tags == 0)
                return null;
            else if (collision.Other.Tags == 1)
                return new SlideResponse(collision);
            else if (collision.Other.Tags == 2)
                return new TouchResponse(collision);
            return new CrossResponse(collision);
        }

        public void UpdateFrame()
        {
            currentCharAction.UpdateFrame();
        }

        public void DrawShadow(SpriteBatch spriteBatch, Loc offset)
        {
            CharSheet sheet = GraphicsManager.GetChara(CurrentForm);
            int teamShadow = 1;
            currentCharAction.DrawShadow(spriteBatch, offset, sheet, new Loc(1, teamShadow * 2));
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            CharSheet sheet = GraphicsManager.GetChara(CurrentForm);
            currentCharAction.Draw(spriteBatch, offset, sheet);

            if (currentEmote != null)
                currentEmote.Draw(spriteBatch, offset - MapLoc - drawOffset);
        }


        public void GetCurrentSprite(out MonsterID currentForm, out Loc currentOffset, out int currentHeight, out int currentAnim, out int currentTime, out int currentFrame)
        {
            currentForm = CurrentForm;
            currentOffset = drawOffset;
            currentHeight = LocHeight;
            CharSheet sheet = GraphicsManager.GetChara(CurrentForm);
            currentCharAction.GetCurrentSprite(sheet, out currentAnim, out currentTime, out currentFrame);
        }

        public Loc GetDrawLoc(Loc offset)
        {
            return currentCharAction.GetDrawLoc(offset, GraphicsManager.GetChara(CurrentForm));
        }

        public Loc GetDrawSize()
        {
            return new Loc(GraphicsManager.GetChara(CurrentForm).TileWidth, GraphicsManager.GetChara(CurrentForm).TileHeight);
        }

        internal void ReloadPosition()
        {
            //restore idle position and direction
            currentCharAction = new IdleGroundAction(serializationLoc, serializationDir);
        }

        /// <summary>
        /// Returns the localized nickname if there's one, or the specie name.
        /// </summary>
        /// <returns></returns>
        public string GetDisplayName()
        {
            if (String.IsNullOrEmpty(Nickname))
                return DataManager.Instance.GetMonster(CurrentForm.Species).Name.ToLocal();
            else
                return Nickname;
        }

        public override EEntTypes GetEntityType()
        {
            return EEntTypes.Character;
        }

        public override bool DevHasGraphics()
        {
            if (CurrentForm.IsValid())
                return true;
            else
                return false;
        }

        public override void ReloadEvents()
        {
            foreach (var entry in ScriptEvents)
                entry.Value.ReloadEvent();
        }

        public override void DoCleanup()
        {
            foreach (var entry in ScriptEvents)
                entry.Value.DoCleanup();
            ScriptEvents.Clear();
        }

        /// <summary>
        /// Called by the map owning this entity after being deserialized to have it set itself up.
        /// </summary>
        /// <param name="map"></param>
        public override void OnDeserializeMap(GroundMap map)
        {
            //DiagManager.Instance.LogInfo(String.Format("GroundChar.OnDeserializeMap(): Handling {0}..", EntName));
            ReloadPosition();
        }

        public override ScriptEvent FindEvent(string eventname)
        {
            foreach (var entry in ScriptEvents)
            {
                if (entry.Value.EventName().Contains(eventname))
                    return entry.Value;
            }
            return null;
        }

        public override bool HasScriptEvent(LuaEngine.EEntLuaEventTypes ev)
        {
            return ScriptEvents.ContainsKey(ev);
        }

        public override bool IsEventSupported(LuaEngine.EEntLuaEventTypes ev)
        {
            return ev == LuaEngine.EEntLuaEventTypes.Action ||
                   ev == LuaEngine.EEntLuaEventTypes.Think;
        }

        public override void AddScriptEvent(LuaEngine.EEntLuaEventTypes ev)
        {
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
            if (ScriptEvents.ContainsKey(ev))
                ScriptEvents.Remove(ev);
        }

        public override void SetTriggerType(EEntityTriggerTypes triggerty)
        {
            //do nothing
        }

        public override IEnumerator<YieldInstruction> RunEvent(LuaEngine.EEntLuaEventTypes ev, params object[] arguments)
        {
            if (ScriptEvents.ContainsKey(ev))
            {
                //Since ScriptEvent.Apply takes a single variadic table, we have to concatenate our current variadic argument table
                // with the extra parameter we want to pass. Otherwise "parameters" will be passed as a table instead of its
                // individual elements, and things will crash left and right.
                List<object> partopass = new List<object>();
                partopass.Add(this);
                partopass.AddRange(arguments);
                yield return CoroutineManager.Instance.StartCoroutine(ScriptEvents[ev].Apply(partopass.ToArray()));
            }
            else
                yield break;
        }

        public override IEnumerator<YieldInstruction> Interact(GroundEntity activator)
        {
            if (!activator.EntEnabled)
                yield break;

            //Cast the entity to its actual type before passing it, so lua can use it properly
            var ent = activator.GetEntityType() == EEntTypes.Character ? (GroundChar)activator :
                      (activator.GetEntityType() == EEntTypes.Object) ? (GroundObject)activator : activator;

            //Run script event
            if (GetTriggerType() == EEntityTriggerTypes.Action)
            {
                IsInteracting = true;
                yield return CoroutineManager.Instance.StartCoroutine(RunEvent(LuaEngine.EEntLuaEventTypes.Action, ent));
                IsInteracting = false;
            }
        }


        public override void Think()
        {
            //Run the base class AI/task stuff
            base.Think();
        }


        private Loc serializationLoc;
        private Dir8 serializationDir;

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            serializationLoc = MapLoc;
            serializationDir = CharDir;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            CurrentCommand = new GameAction(GameAction.ActionType.None, Dir8.None);
        }
    }
}

