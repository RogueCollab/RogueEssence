using System;
using System.IO;
using RogueEssence.Data;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace RogueEssence.Dev
{
    public static class DevHelper
    {
        public static void ReserializeData(string dataPath, SerializationBinder binder)
        {
            foreach (string dir in Directory.GetFiles(dataPath, "*.bin"))
            {
                IEntryData data = (IEntryData)DataManager.LoadData(dir, binder);
                DataManager.SaveData(dir, data);
            }
        }


        public static void IndexNamedData(string dataPath)
        {
            try
            {
                EntryDataIndex fullGuide = new EntryDataIndex();

                foreach (string dir in Directory.GetFiles(dataPath, "*.bin"))
                {
                    string file = Path.GetFileNameWithoutExtension(dir);
                    int num = Convert.ToInt32(file);
                    IEntryData data = (IEntryData)DataManager.LoadData(dir);
                    while (fullGuide.Entries.Count <= num)
                        fullGuide.Entries.Add(new EntrySummary());
                    fullGuide.Entries[num] = data.GenerateEntrySummary();
                }

                using (Stream stream = new FileStream(dataPath + "index.idx", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(stream, fullGuide);
                    }
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new Exception("Error importing index at " + dataPath + "\n", ex));
            }
        }


    }
}
