using System.Collections.Generic;

namespace RogueEssence.Data
{
    public class RecordHeaderData
    {
        public const int MAX_HIGH_SCORES = 12;

        public string Name;
        public string DateTimeString;
        public int Score;
        public string Path;
        public bool Valid;

        public RecordHeaderData(string path)
        {
            Path = path;
            Name = "";
            DateTimeString = "";
        }




        public static List<RecordHeaderData> LoadHighScores()
        {
            List<RecordHeaderData> highScores = new List<RecordHeaderData>();

            //generate the high scores in real time by going through all the replays
            List<RecordHeaderData> records = DataManager.Instance.GetRecordHeaders(Data.DataManager.REPLAY_PATH);
            
            foreach (RecordHeaderData record in records)
            {
                if (record.Valid)
                {
                    int placing = 0;
                    for (int ii = 0; ii < highScores.Count; ii++)
                    {
                        if (highScores[ii].Score <= record.Score)
                            break;
                        placing++;
                    }
                    highScores.Insert(placing, record);
                    if (highScores.Count > RecordHeaderData.MAX_HIGH_SCORES)
                        highScores.RemoveRange(RecordHeaderData.MAX_HIGH_SCORES, highScores.Count - RecordHeaderData.MAX_HIGH_SCORES);
                }
            }

            return highScores;
        }
    }
}
