using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using RogueEssence.Data;
using RogueEssence.Ground;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class ZoneManager
    {

        private static ZoneManager instance;
        public static void InitInstance()
        {
            instance = new ZoneManager();
        }
        public static ZoneManager Instance { get { return instance; } }
        
        public static void SaveToState(BinaryWriter writer, GameState state)
        {
            state.Zone.SaveLua();
            using (MemoryStream classStream = new MemoryStream())
            {
                IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(classStream, state.Zone);
                writer.Write(classStream.Position);
                classStream.WriteTo(writer.BaseStream);
            }
        }
        
        public static void LoadFromState(ZoneManager state)
        {
            instance = state;
            instance.LoadLua();
        }
        public static void LoadToState(BinaryReader reader, GameState state)
        {
            try
            {
                long length = reader.ReadInt64();
                using (MemoryStream classStream = new MemoryStream(reader.ReadBytes((int)length)))
                {
                    IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    state.Zone = (ZoneManager)formatter.Deserialize(classStream);
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
                LoadDefaultState(state);
            }
        }
        public static void LoadDefaultState(GameState state)
        {
            state.Save.NextDest = DataManager.Instance.StartMap;

            ZoneData zone = DataManager.Instance.GetZone(DataManager.Instance.StartMap.ID);
            state.Zone = new ZoneManager();
            state.Zone.CurrentZone = zone.CreateActiveZone(0, DataManager.Instance.StartMap.ID);
            state.Zone.CurrentZone.SetCurrentMap(DataManager.Instance.StartMap.StructID);
        }

        public Zone CurrentZone { get; private set; }
        public int CurrentZoneID { get; private set; }
        public SegLoc CurrentMapID { get { return CurrentZone.CurrentMapID; } }
        public Map CurrentMap { get { return (CurrentZone == null) ? null : CurrentZone.CurrentMap; } }
        public GroundMap CurrentGround { get { return (CurrentZone == null) ? null : CurrentZone.CurrentGround; } }

        public ZoneManager()
        {
            CurrentZoneID = -1;
        }

        //include a current groundmap, with moveto methods included

        public void MoveToZone(int zoneIndex, string mapname, ulong seed)
        {
            if (CurrentZone != null)
                CurrentZone.DoCleanup();
            CurrentZoneID = zoneIndex;
            ZoneData zone = DataManager.Instance.GetZone(zoneIndex);
            if (zone != null)
            {
                CurrentZone = zone.CreateActiveZone(seed, zoneIndex);
                CurrentZone.SetCurrentGround(mapname);
            }
        }

        public void MoveToZone(int zoneIndex, SegLoc mapId, ulong seed)
        {
            if (CurrentZone != null)
                CurrentZone.DoCleanup();
            CurrentZoneID = zoneIndex;
            ZoneData zone = DataManager.Instance.GetZone(zoneIndex);
            if (zone != null)
            {
                CurrentZone = zone.CreateActiveZone(seed, zoneIndex);
                CurrentZone.SetCurrentMap(mapId);
            }
        }

        public void Cleanup()
        {
            CurrentZoneID = -1;
            if (CurrentZone != null)
                CurrentZone.DoCleanup();
            CurrentZone = null;
        }

        public void MoveToDevZone(bool newGround, string name)
        {
            CurrentZoneID = -1;
            CurrentZone = new Zone(0, -1);
            if (newGround)
            {
                if (!String.IsNullOrEmpty(name))
                    ZoneManager.Instance.CurrentZone.DevLoadGround(name);
                else
                    ZoneManager.Instance.CurrentZone.DevNewGround();
            }
            else
            {
                if (!String.IsNullOrEmpty(name))
                    ZoneManager.Instance.CurrentZone.DevLoadMap(name);
                else
                    ZoneManager.Instance.CurrentZone.DevNewMap();
            }
        }
        public bool InDevZone
        {
            get
            {
                if (CurrentZoneID > -1)
                    return false;
                if (CurrentZone == null)
                    return false;
                return CurrentZone.CurrentMapID.ID == 0;
            }
        }

        public void LuaEngineReload()
        {
            if (CurrentZone != null)
                CurrentZone.LuaEngineReload();
        }
        public void SaveLua()
        {
            if (CurrentZone != null)
                CurrentZone.SaveLua();
        }
        public void LoadLua()
        {
            if (CurrentZone != null)
                CurrentZone.LoadLua();
        }
    }
}
