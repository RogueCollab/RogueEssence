using System;
using RogueElements;
using AABB;
using RogueEssence.Dungeon;
using System.Collections.Generic;
using RogueEssence.Content;
using System.Runtime.Serialization;

namespace RogueEssence.Ground
{

    [Serializable]
    public class EntityLayer : IMapLayer
    {
        public string Name { get; set; }
        public bool Visible { get; set; }

        public List<GroundChar> MapChars;
        /// <summary>
        /// Field for character entities that should not be serialized
        /// </summary>
        [NonSerialized]
        public List<GroundChar> TemporaryChars;

        public List<GroundObject> GroundObjects;

        /// <summary>
        /// Field for object entities that should not be serialized
        /// </summary>
        [NonSerialized]
        public List<GroundObject> TemporaryObjects;

        /// <summary>
        /// Contains a list of all the NPCs spawners on this map
        /// </summary>
        public List<GroundSpawner> Spawners;
        /// <summary>
        /// A list of ground markers.
        /// </summary>
        public List<GroundMarker> Markers;


        public EntityLayer(string name)
        {
            Name = name;
            Visible = true;

            GroundObjects = new List<GroundObject>();
            Markers = new List<GroundMarker>();
            MapChars = new List<GroundChar>();
            Spawners = new List<GroundSpawner>();
            TemporaryChars = new List<GroundChar>();
            TemporaryObjects = new List<GroundObject>();
        }

        protected EntityLayer(EntityLayer other)
        {
            Name = other.Name;
            Visible = other.Visible;

            //just dont copy the contents
            GroundObjects = new List<GroundObject>();
            Markers = new List<GroundMarker>();
            MapChars = new List<GroundChar>();
            Spawners = new List<GroundSpawner>();
            TemporaryChars = new List<GroundChar>();
            TemporaryObjects = new List<GroundObject>();
        }

        public IMapLayer Clone() { return new EntityLayer(this); }

        public void Merge(IMapLayer other)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Name;
        }

        public IEnumerable<GroundChar> IterateCharacters()
        {
            foreach (GroundChar player in MapChars)
                yield return player;

            foreach (GroundChar temp in TemporaryChars)
                yield return temp;
        }

        public IEnumerable<GroundObject> IterateObjects()
        {
            foreach (GroundObject v in GroundObjects)
                yield return v;

            foreach (GroundObject v in TemporaryObjects)
                yield return v;

        }


        /// <summary>
        /// Allow iterating through all entities on the map,
        /// characters, objects, markers
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GroundEntity> IterateEntities()
        {
            foreach (GroundEntity v in IterateCharacters())
                yield return v;

            foreach (GroundEntity v in GroundObjects)
                yield return v;

            foreach (GroundEntity v in TemporaryObjects)
                yield return v;

            foreach (GroundEntity v in Markers)
                yield return v;

            foreach (GroundEntity s in Spawners)
                yield return s;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            //Make sure the temp char array is instantiated
            TemporaryChars = new List<GroundChar>();
        }
    }
}

