using System.Collections.Generic;

namespace RogueEssence.Data
{
    public class RecordHeaderData
    {
        public const int MAX_HIGH_SCORES = 12;

        public string Name;
        public string DateTimeString;
        public string LocationString;
        public int Zone;
        public int Score;
        public string Path;
        public bool IsRogue;
        public bool IsSeeded;
        public ulong Seed;
        public GameProgress.ResultType Result;

        public RecordHeaderData(string path)
        {
            Path = path;
            Name = "";
            DateTimeString = "";
            LocationString = "";
        }




        /// <summary>
        /// Generate all high score tables in real time, organized by dungeon, by going through all replays
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, List<RecordHeaderData>> LoadHighScores()
        {
            Dictionary<int, List<RecordHeaderData>> highScores = new Dictionary<int, List<RecordHeaderData>>();

            List<RecordHeaderData> records = DataManager.Instance.GetRecordHeaders(DataManager.REPLAY_PATH, DataManager.REPLAY_EXTENSION);
            
            foreach (RecordHeaderData record in records)
            {
                if (record.IsRogue && !record.IsSeeded)
                {
                    if (!highScores.ContainsKey(record.Zone))
                        highScores[record.Zone] = new List<RecordHeaderData>();

                    List<RecordHeaderData> dungeonHighScore = highScores[record.Zone];

                    int placing = 0;
                    for (int ii = 0; ii < dungeonHighScore.Count; ii++)
                    {
                        if (dungeonHighScore[ii].Score <= record.Score)
                            break;
                        placing++;
                    }
                    dungeonHighScore.Insert(placing, record);
                    if (dungeonHighScore.Count > RecordHeaderData.MAX_HIGH_SCORES)
                        dungeonHighScore.RemoveRange(RecordHeaderData.MAX_HIGH_SCORES, dungeonHighScore.Count - RecordHeaderData.MAX_HIGH_SCORES);
                }
            }

            return highScores;
        }
    }
}
