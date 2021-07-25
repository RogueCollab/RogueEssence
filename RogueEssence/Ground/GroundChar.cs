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
using NLua;

namespace RogueEssence.Ground
{

    [Serializable]
    public class GroundChar : GroundAIUser, ICharSprite, IBox, IEntityWithLuaData
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

        public LuaTable LuaData
        {
            get { return Data.LuaDataTable; }
            set { Data.LuaDataTable = value; }
        }

        public uint Tags
        {
            get
            {
                if (!EntEnabled || CollisionDisabled)
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

        public bool CollisionDisabled;

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
        protected GroundChar(GroundChar other)
        {
            Data = new CharData(other.Data);

            currentCharAction = new IdleGroundAction(Loc.Zero, Dir8.Down);
            CurrentCommand = new GameAction(GameAction.ActionType.None, Dir8.None);

            //from base
            EntEnabled = other.EntEnabled;
            Collider = other.Collider;
            EntName = other.EntName;
            Direction = other.Direction;
            triggerType = other.triggerType;
        }
        public override GroundEntity Clone() { return new GroundChar(this); }


        public void StartEmote(Emote emote)
        {
            currentEmote = emote;
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
            if (DataManager.Instance.Save.CutsceneMode || CollisionDisabled)
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

            Loc shadowType = new Loc(0, 2 + sheet.ShadowSize * 3);
            Loc shadowPoint = currentCharAction.GetActionPoint(sheet, ActionPointType.Shadow);

            GraphicsManager.Shadows.DrawTile(spriteBatch,
                (shadowPoint - offset).ToVector2() - new Vector2(GraphicsManager.Shadows.TileWidth / 2, GraphicsManager.Shadows.TileHeight / 2),
                shadowType.X, shadowType.Y);
        }

        public void DrawDebug(SpriteBatch spriteBatch, Loc offset) { }
        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            CharSheet sheet = GraphicsManager.GetChara(CurrentForm);
            currentCharAction.Draw(spriteBatch, offset, sheet);

            if (currentEmote != null)
            {
                Loc head = currentCharAction.GetActionPoint(sheet, ActionPointType.Head);
                currentEmote.Draw(spriteBatch, offset - head - drawOffset);
            }
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

        /// <summary>
        /// Returns the localized nickname if there's one, or the specie name, fully colored.
        /// </summary>
        /// <returns></returns>
        public string GetDisplayName()
        {
            string name = Nickname;
            if (String.IsNullOrEmpty(Nickname))
                name = DataManager.Instance.GetMonster(CurrentForm.Species).Name.ToLocal();

            if (Data is Character)
            {
                Team team = ((Character)Data).MemberTeam;
                if (team == DataManager.Instance.Save.ActiveTeam)
                {
                    if (Data == team.Leader)
                        return String.Format("[color=#009CFF]{0}[color]", name);
                    return String.Format("[color=#FFFF00]{0}[color]", name);
                }
            }
            return String.Format("[color=#00FFFF]{0}[color]", name);
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

        /// <summary>
        /// Called by the map owning this entity after being deserialized to have it set itself up.
        /// </summary>
        /// <param name="map"></param>
        public override void OnDeserializeMap(GroundMap map)
        {
            //DiagManager.Instance.LogInfo(String.Format("GroundChar.OnDeserializeMap(): Handling {0}..", EntName));
            ReloadPosition();
        }
        public override void OnSerializeMap(GroundMap map)
        {
            //DiagManager.Instance.LogInfo(String.Format("GroundChar.OnDeserializeMap(): Handling {0}..", EntName));
            SavePosition();
        }


        public override bool IsEventSupported(LuaEngine.EEntLuaEventTypes ev)
        {
            return ev == LuaEngine.EEntLuaEventTypes.Action ||
                   ev == LuaEngine.EEntLuaEventTypes.Think;
        }

        public override void LuaEngineReload()
        {
            ReloadEvents();
        }

        public override void SetTriggerType(EEntityTriggerTypes triggerty)
        {
            //do nothing
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


        internal void ReloadPosition()
        {
            //restore idle position and direction
            currentCharAction = new IdleGroundAction(serializationLoc, serializationDir);
        }

        internal void SavePosition()
        {
            serializationLoc = MapLoc;
            serializationDir = CharDir;
        }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            SavePosition();
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            CurrentCommand = new GameAction(GameAction.ActionType.None, Dir8.None);
        }
    }
}

