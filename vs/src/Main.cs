namespace CT2
{
    public class Main : MelonMod
    {
        public const string modVersion = "1.0.0";
        public const string modName = "ClothingTweaker2";
        public const string modAuthor = "Waltz";

        public const string resourcesFolder = "ClothingTweaker2.Resources."; // root is project name

        public static string modsPath;
        public static AssetBundle UIBundle;

        public static ModDataManager dataManager = new ModDataManager(modName);
        public static readonly string saveDataTag = "alltweaks";

        public static Dictionary<string, ClothingData> bigData = new();
        public enum Stat
        {
            Undefined,
            Warmth,
            WarmthWet,
            Windproof,
            Waterproof,
            Protection,
            Mobility,
            Weight
        }
        public override void OnInitializeMelon()
        {
            modsPath = Path.GetFullPath(typeof(MelonMod).Assembly.Location + "/../../../Mods/");

            UIBundle = LoadEmbeddedAssetBundle("ctui");

            Settings.OnLoad();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (IsScenePlayable(sceneName))
            {
                GameObject eventCam = GameManager.GetMainCamera().gameObject;
                eventCam.GetOrAddComponent<EventSystem>();
                eventCam.GetOrAddComponent<StandaloneInputModule>();
            }
        }

        public static void ResetAllCustomData() => bigData.Clear();
        public static void SetInstanceClothingValuesToTweaked(ClothingItem ci)
        {
            string name = Utils.SanitizePrefabName(ci.name);

            GearItem giPrefab = GearItem.LoadGearItemPrefab(name);
            ClothingItem ciPrefab = giPrefab.GetComponent<ClothingItem>();
            ci.m_Warmth = bigData[name].warmth ?? ciPrefab.m_Warmth;
            ci.m_WarmthWhenWet = bigData[name].warmthWet ?? ciPrefab.m_WarmthWhenWet;
            ci.m_Windproof = bigData[name].windproof ?? ciPrefab.m_Windproof;
            ci.m_Waterproofness = bigData[name].waterproof ?? ciPrefab.m_Waterproofness;
            ci.m_Toughness = bigData[name].protection ?? ciPrefab.m_Toughness;
            ci.m_SprintBarReductionPercent = bigData[name].mobility ?? ciPrefab.m_SprintBarReductionPercent;
            if (bigData[name].weight != null) ci.GetComponent<GearItem>().WeightKG = ItemWeight.FromKilograms((float)bigData[name].weight);
            else ci.GetComponent<GearItem>().WeightKG = giPrefab.GearItemData.BaseWeightKG;
        }
    }
}
