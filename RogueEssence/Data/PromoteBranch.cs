using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;

namespace RogueEssence.Data
{
    [Serializable]
    public class PromoteBranch : Dev.EditorData
    {
        [Dev.DataType(0, DataManager.DataType.Monster, false)]
        public int Result;
        public List<PromoteDetail> Details;

        public PromoteBranch()
        {
            Details = new List<PromoteDetail>();
        }

        public bool IsQualified(Character character, bool inDungeon)
        {
            foreach (PromoteDetail detail in Details)
            {
                if (!inDungeon && !detail.GetGroundReq(character) || inDungeon && !detail.GetReq(character))
                    return false;
            }
            return true;
        }

        public void OnPromote(Character character, bool inDungeon)
        {
            foreach (PromoteDetail detail in Details)
            {
                if (inDungeon)
                    detail.OnPromote(character);
                else
                    detail.OnGroundPromote(character);
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
    public abstract class PromoteDetail : Dev.EditorData
    {
        public virtual int GiveItem { get { return -1; } }
        public virtual string GetReqString() { return ""; }
        public virtual bool IsHardReq() { return false; }
        public virtual bool GetGroundReq(Character character) { return GetReq(character); }
        public virtual bool GetReq(Character character) { return true; }
        public virtual void OnGroundPromote(Character character) { OnPromote(character); }
        public virtual void OnPromote(Character character) { }
    }

}
