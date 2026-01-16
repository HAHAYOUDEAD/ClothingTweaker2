using UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

namespace CT2
{
    public class Main : MelonMod
    {
        public const string modVersion = "1.0.5";
        public const string modName = "ClothingTweaker2";
        public const string modAuthor = "Waltz";

        public const string resourcesFolder = "ClothingTweaker2.Resources."; // root is project name

        public static string modsPath;
        public static AssetBundle UIBundle;

        public static ModDataManager dataManager = new ModDataManager(modName);
        public static readonly string saveDataTag = "alltweaks";

        public static Dictionary<string, ClothingData> bigData = new();

        public static Dictionary<string, GearItem> tempPrefabList = new();

        public static int coroutineRunning;
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
        public static IEnumerator SetInstanceClothingValuesToTweaked(ClothingItem ci)
        {
            MelonLogger.Msg("reset 0");
            if (ci == null) yield break;
            MelonLogger.Msg("reset 1");
            while (coroutineRunning > 0)
            {
                yield return new WaitForEndOfFrame();
            }
            MelonLogger.Msg("reset 2");
            string name = Utils.SanitizePrefabName(ci.name);
            GearItem giPrefab;
            if (tempPrefabList.ContainsKey(name) && tempPrefabList[name]?.gameObject != null) giPrefab = tempPrefabList[name];
            else
            {
                coroutineRunning++;
                giPrefab = GearItem.LoadGearItemPrefab(name); // running 2+ of those with same gear name at the same time causes issues
                //giPrefab.gameObject.SetActive(false);
                float f = 0f;
                while (!giPrefab || f > 1f)
                {
                    f += Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
                coroutineRunning--;
                if (!giPrefab)
                {
                    Log(CC.Red, $"Can't load {name} refab, tweaks were not loaded");
                    yield break;
                }
                tempPrefabList[name] = giPrefab;
            }
            MelonLogger.Msg("reset 3");
            ClothingItem ciPrefab = giPrefab.GetComponent<ClothingItem>();
            ci.m_Warmth = bigData[name].warmth ?? ciPrefab.m_Warmth;
            ci.m_WarmthWhenWet = bigData[name].warmthWet ?? ciPrefab.m_WarmthWhenWet;
            ci.m_Windproof = bigData[name].windproof ?? ciPrefab.m_Windproof;
            ci.m_Waterproofness = bigData[name].waterproof ?? ciPrefab.m_Waterproofness;
            ci.m_Toughness = bigData[name].protection ?? ciPrefab.m_Toughness;
            ci.m_SprintBarReductionPercent = bigData[name].mobility ?? ciPrefab.m_SprintBarReductionPercent;
            if (bigData[name].weight != null) ci.GetComponent<GearItem>().WeightKG = ItemWeight.FromKilograms((float)bigData[name].weight);
            else ci.GetComponent<GearItem>().WeightKG = giPrefab.GearItemData.BaseWeightKG;
            MelonLogger.Msg("reset 4");
            yield break;
        }
    }
}
