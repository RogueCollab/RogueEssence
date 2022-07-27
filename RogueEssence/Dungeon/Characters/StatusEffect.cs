using System;
using Newtonsoft.Json;
using RogueEssence.Data;
using RogueEssence.Dev;

namespace RogueEssence.Dungeon
{

    public enum DrawEffect
    {
        /// <summary>
        /// No draw effect
        /// </summary>
        None = -1,
        /// <summary>
        /// The character uses its sleeping animation.
        /// </summary>
        Sleeping = 0,
        /// <summary>
        /// The character uses only the first frame of its idle animation
        /// </summary>
        Stopped,
        /// <summary>
        /// Animates normally, but shaking
        /// </summary>
        Shaking,
        /// <summary>
        /// In a charging pose.
        /// </summary>
        Charging,
        /// <summary>
        /// Not drawn, but the shadow is still there.
        /// </summary>
        Absent,
        /// <summary>
        /// Constantly spinning
        /// </summary>
        Spinning,
        /// <summary>
        /// Constantly in pain
        /// </summary>
        Hurt,
        /// <summary>
        /// Semi-transparent
        /// </summary>
        Transparent
    }

    
    [Serializable]
    public class StatusEffect : PassiveActive
    {
        public override GameEventPriority.EventCause GetEventCause()
        {
            return GameEventPriority.EventCause.Status;
        }
        public override PassiveData GetData() { return DataManager.Instance.GetStatus(ID); }
        public override string GetDisplayName() { return DataManager.Instance.GetStatus(ID).GetColoredName(); }

        public override string GetID() { return ID; }

        [JsonConverter(typeof(StatusConverter))]
        [DataType(0, DataManager.DataType.Status, false)]
        public string ID { get; set; }
        //handles stuff like stacking, sealing, movement speed, etc.
        public StateCollection<StatusState> StatusStates;

        [NonSerialized]
        public Character TargetChar;

        public StatusEffect() : base()
        {
            ID = "";
            StatusStates = new StateCollection<StatusState>();
        }
        public StatusEffect(string index)
            : this()
        {
            ID = index;
        }
        //TODO: String Assets
        protected StatusEffect(StatusEffect other)// : base(other)
        {
            //TODO: String Assets
            ID = other.ID;
            StatusStates = other.StatusStates.Clone();
        }
        public StatusEffect Clone() { return new StatusEffect(this); }

        public void LoadFromData()
        {
            StatusData entry = DataManager.Instance.GetStatus(ID);
            foreach (StatusState state in entry.StatusStates)
            {
                if (!StatusStates.Contains(state.GetType()))
                    StatusStates.Set(state.Clone<StatusState>());
            }
        }

    }

    public class StatusRef
    {
        public StatusData GetStatusEntry() { return DataManager.Instance.GetStatus(ID); }

        public string ID;

        public Character TargetChar;

        public StatusRef(string index, Character targetChar)
        {
            ID = index;
            TargetChar = targetChar;
        }
    }


}

