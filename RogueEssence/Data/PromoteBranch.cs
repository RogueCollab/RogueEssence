using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RogueEssence.Dev;
using RogueEssence.Dungeon;

namespace RogueEssence.Data
{
    [Serializable]
    public class PromoteBranch
    {
        [JsonConverter(typeof(MonsterConverter))]
        [Dev.DataType(0, DataManager.DataType.Monster, false)]
        public string Result;
        public List<PromoteDetail> Details;

        public PromoteBranch()
        {
            Details = new List<PromoteDetail>();
        }

        public bool IsQualified(Character character, bool inDungeon)
        {
            foreach (PromoteDetail detail in Details)
            {
                if (!detail.GetReq(character, inDungeon))
                    return false;
            }
            return true;
        }

        public void BeforePromote(Character character, bool inDungeon, ref MonsterID result)
        {
            foreach (PromoteDetail detail in Details)
            {
                detail.BeforePromote(character, inDungeon, ref result);
            }
        }

        public void OnPromote(Character character, bool inDungeon, bool noGive)
        {
            foreach (PromoteDetail detail in Details)
            {
                if (noGive && !String.IsNullOrEmpty(detail.GiveItem))
                    continue;
                if (inDungeon)
                    detail.OnPromote(character, inDungeon);
            }
        }

        public string GetReqString()
        {
            string reqList = "";
            foreach (PromoteDetail detail in Details)
            {
                if (!detail.IsHardReq() && detail.GetReqString() != "")
                {
                    if (reqList != "")
                        reqList += ", ";
                    reqList += detail.GetReqString();
                }
            }
            if (reqList != "")
                reqList = (reqList[0].ToString()).ToUpper() + reqList.Substring(1);
            return reqList;
        }
    }

    [Serializable]
    public abstract class PromoteDetail
    {
        public virtual string GiveItem { get { return ""; } }
        public virtual string GetReqString() { return ""; }
        public virtual bool IsHardReq() { return false; }
        public virtual bool GetReq(Character character, bool inDungeon) { return true; }
        public virtual void BeforePromote(Character character, bool inDungeon, ref MonsterID result) { }
        public virtual void OnPromote(Character character, bool inDungeon) { }
    }

}
