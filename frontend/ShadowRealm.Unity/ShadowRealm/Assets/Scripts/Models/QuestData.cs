#nullable enable
using System;

namespace Models
{
    [Serializable]
    public class QuestData
    {
        public int id;
        public string title = "";
        public string description = "";
        public int rewardXP;
    }
}