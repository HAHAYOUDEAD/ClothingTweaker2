namespace CT2
{
    public class DataOperations
    {


        public static string dumpFilePath = @$"{modsPath}{modName}\dump.json";

        public static string Serialize()
        {
            return JSON.Dump(MakeIntoProxy(bigData), EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints);
        }

        public static Dictionary<string, ClothingData> Deserialize(string serializedString)
        {
            Dictionary<string, ClothingDataSaveProxy> proxy = new();

            if (!string.IsNullOrEmpty(serializedString))
            {
                JSON.Load(serializedString).Make(out proxy);
            }

            return MakeFromProxy(proxy);
        }

        public static Dictionary<string, ClothingData> MakeFromProxy(Dictionary<string, ClothingDataSaveProxy> proxy)
        {
            Dictionary<string, ClothingData> dict = new();
            if (proxy.Count < 1) return dict;
            foreach (KeyValuePair<string, ClothingDataSaveProxy> e in proxy)
            {
                dict[e.Key] = new ClothingData()
                {
                    warmth = e.Value.warmth < 0f ? null : e.Value.warmth,
                    warmthWet = e.Value.warmthWet < 0f ? null : e.Value.warmthWet,
                    windproof = e.Value.windproof < 0f ? null : e.Value.windproof,
                    waterproof = e.Value.waterproof < 0f ? null : e.Value.waterproof,
                    protection = e.Value.protection < 0f ? null : e.Value.protection,
                    mobility = e.Value.mobility < 0f ? null : e.Value.mobility,
                    weight = e.Value.weight < 0f ? null : e.Value.weight,
                };
            }

            return dict;
        }

        public static Dictionary<string, ClothingDataSaveProxy> MakeIntoProxy(Dictionary<string, ClothingData> dict)
        {
            Dictionary<string, ClothingDataSaveProxy> proxy = new();
            if (dict.Count < 1) return proxy;
            foreach (KeyValuePair<string, ClothingData> e in dict)
            {
                proxy[e.Key] = new ClothingDataSaveProxy()
                { 
                    warmth = e.Value.warmth ?? -1f,
                    warmthWet = e.Value.warmthWet ?? -1f,
                    windproof = e.Value.windproof ?? -1f,
                    waterproof = e.Value.waterproof ?? -1f,
                    protection = e.Value.protection ?? -1f,
                    mobility = e.Value.mobility ?? -1f,
                    weight = e.Value.weight ?? -1f,
                };
            }

            return proxy;
        }

        public static void ExportDataToFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dumpFilePath));
            File.WriteAllText(dumpFilePath, Serialize());
        }
        
        public static void ReadDataFromFile()
        {
            string serializedData = File.ReadAllText(dumpFilePath);
            bigData = Deserialize(serializedData);
        }
    }
}
