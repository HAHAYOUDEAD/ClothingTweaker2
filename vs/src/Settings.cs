using ModSettings;

namespace CT2
{
    internal static class Settings
    {
        public static void OnLoad()
        {
            options = new TweakerSettings();
            options.AddToModSettings("Clothing Tweaker 2", MenuType.Both);
        }

        public static TweakerSettings options;
    }

    internal class TweakerSettings : JsonModSettings
    {
        [Section("Global Decay Rates")]

        [Name("     While Not Wearing")]
        [Description("Those are global options, across all saveslots, and only updated after reload\n\nDefault: 100%")]
        [Slider(0f, 1.5f, 151, NumberFormat = "{0:P0}")]
        public float clothingDecayDaily = 1f;

        [Name("     While Wearing Indoors")]
        [Description("Those are global options, across all saveslots, and only updated after reload\n\nDefault: 100%")]
        [Slider(0f, 1.5f, 151, NumberFormat = "{0:P0}")]
        public float clothingDecayIndoors = 1f;

        [Name("     While Wearing Outdoors")]
        [Description("Those are global options, across all saveslots, and only updated after reload\n\nDefault: 100%")]
        [Slider(0f, 1.5f, 151, NumberFormat = "{0:P0}")]
        public float clothingDecayOutdoors = 1f;

        [Section("Export / Import")]

        [Name("Export Settings")]
        [Description($"Used to transfer settings between saveslots, or if you want to share your preset with other people. Settings will be exported to ../Mods/{Main.modName}/dump.json\n\nConfirm to export")]
        public bool export = false;

        [Name("Import Settings")]
        [Description($"Settings will be imported from ../Mods/{Main.modName}/dump.json\n\nConfirm to import\n\nSave the game for changes to stay")]
        public bool import = false;

        [Section("Reset")]

        [Name("Reset All Tweaks")]
        [Description("Quit without saving to undo, but better have your settings exported")]
        public bool reset = false;

        [Section("Other Settings")]

        [Name("Potato Mode")]
        [Description("Doesn't work for now, leave at Disabled\n\nGaming potato - only disable live slider updates, updates still happen when closing clothing panel\n\nPotato - disable animations and only apply updates on game reload")]
        [Choice(new string[]
        {
            "Disabled",
            "Gaming potato",
            "Potato"
        })]
        public int potatoMode;

        [Name("Debug Log")]
        [Description("Log debug info into console. Only enable when asked by modder")]
        public bool log = false;
        

        protected override void OnConfirm()
        {
            if (export)
            {
                DataOperations.ExportDataToFile();
                export = false;
                if (import)
                {
                    import = false;
                }
            }

            if (import)
            {
                DataOperations.ReadDataFromFile();
                import = false;
            }

            if (reset)
            {
                Main.ResetAllCustomData();
                reset = false;
            }

            base.OnConfirm();
        }
    }
}
