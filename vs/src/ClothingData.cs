using System.ComponentModel;

namespace CT2
{
    public class ClothingData
    {
        public object? coroutine;
        public int updateCounter;
        public bool doUpdate;
        public Stat lastChangedStat;
        public float? warmth;
        public float? warmthWet;
        public float? windproof;
        public float? waterproof;
        public float? protection;
        public float? mobility;
        public float? weight;
    }

    public class ClothingDataVanilla
    {
        public float warmth;
        public float warmthWet;
        public float windproof;
        public float waterproof;
        public float protection;
        public float mobility;
        public float weight;
    }

    public class ClothingDataSaveProxy
    {
        public float warmth;
        public float warmthWet;
        public float windproof;
        public float waterproof;
        public float protection;
        public float mobility;
        public float weight;
    }
}
