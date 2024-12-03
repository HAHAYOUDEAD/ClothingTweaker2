using static CT2.Control;

namespace CT2
{
    internal class UnbreakablePatches
    {
        [HarmonyPatch(typeof(ClothingItem), nameof(ClothingItem.Awake))]
        private static class ClothingItemAwake
        {
            internal static void Postfix(ref ClothingItem __instance)
            {
                if (__instance.gameObject.scene.name == null) return; // prefab, we don't touch that

                string name = Utils.SanitizePrefabName(__instance.name);

                UpdateDecayRates(__instance);

                if (bigData.ContainsKey(name))
                {
                    Log(CC.Gray, $"Loaded {name} tweaks");
                    SetInstanceClothingValuesToTweaked(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(ClothingItem), nameof(ClothingItem.Update))]
        private static class ClothingItemUpdate
        {
            internal static void Postfix(ref ClothingItem __instance)
            {
                if (__instance.gameObject.scene.name == null) return; // prefab, we don't touch that

                string name = __instance.name;

                if (bigData.ContainsKey(name) && bigData[name].doUpdate)
                {
                    bigData[name].updateCounter++;

                    if (resetPressed)
                    {
                        SetInstanceClothingValuesToTweaked(__instance);
                    }
                    else
                    {
                        Stat st = bigData[name].lastChangedStat;

                        switch (st)
                        {
                            default:
                                SetInstanceClothingValuesToTweaked(__instance);
                                break;
                            case Stat.Warmth:
                                __instance.m_Warmth = bigData[name].warmth ?? GetSpecificVanillaValue(name, st);
                                break;
                            case Stat.WarmthWet:
                                __instance.m_WarmthWhenWet = bigData[name].warmthWet ?? GetSpecificVanillaValue(name, st);
                                break;
                            case Stat.Windproof:
                                __instance.m_Windproof = bigData[name].windproof ?? GetSpecificVanillaValue(name, st);
                                break;
                            case Stat.Waterproof:
                                __instance.m_Waterproofness = bigData[name].waterproof ?? GetSpecificVanillaValue(name, st);
                                break;
                            case Stat.Protection:
                                __instance.m_Toughness = bigData[name].protection ?? GetSpecificVanillaValue(name, st);
                                break;
                            case Stat.Mobility:
                                __instance.m_SprintBarReductionPercent = bigData[name].mobility ?? GetSpecificVanillaValue(name, st);
                                break;
                            case Stat.Weight:
                                if (bigData[name].weight != null) __instance.GetComponent<GearItem>().WeightKG = ItemWeight.FromKilograms((float)bigData[name].weight);
                                else __instance.GetComponent<GearItem>().WeightKG = ItemWeight.FromKilograms(GetSpecificVanillaValue(name, st));
                                break;
                        }
                    }

                    Log(CC.Yellow, $"Live updating {name}, counter: {bigData[name].updateCounter}");

                    bigData[name].updateCounter--;

                    if (liveClothingPanelUpdate)
                    {
                        /* Clothing Panel
                        InterfaceManager.GetPanel<Panel_Clothing>().UpdateGlobalStatBars();
                        InterfaceManager.GetPanel<Panel_Clothing>().UpdateGearStatsBlock();
                        */
                        // Inventory Panel
                        InterfaceManager.GetPanel<Panel_Inventory>().UpdateGearStatsBlock();
                        liveClothingPanelUpdate = false;
                    }


                }
            }
        }

        /* Clothing Panel
        [HarmonyPatch(typeof(Panel_Clothing), nameof(Panel_Clothing.Enable))]
        private static class ClothingPanelTracker
        {
            public static bool startTracking;

            internal static void Postfix(ref Panel_Clothing __instance, ref bool enable)
            {
                startTracking = enable;

                if (enable)
                {
                    // instantiate and hide UI
                    SetupUI();
                    CTUIRoot.active = false;
                    ToggleWindow(false);

                }
                else
                {
                    // hide UI
                    UIInjection.currentlySelected = null;
                    CTUIRoot.active = false;

                    if (Settings.options.potatoMode > 0)
                    {
                        foreach (KeyValuePair<string, ClothingData> e in bigData)
                        {
                            e.Value.doUpdate = true;
                        }
                    }
                } 
            }
        }


        [HarmonyPatch(typeof(Panel_Clothing), nameof(Panel_Clothing.Update))]
        private static class UIInjection
        {
            public static GearItem? currentlySelected;

            internal static void Postfix(ref Panel_Clothing __instance)
            {
                if (!ClothingPanelTracker.startTracking) return;

                if (__instance.GetCurrentlySelectedGearItem() == currentlySelected) return;

                currentlySelected = __instance.GetCurrentlySelectedGearItem();

                if (currentlySelected != null)
                {
                    // show UI
                    CTUIRoot.active = true;

                    GearItem giPrefab = GearItem.LoadGearItemPrefab(currentlySelected.name);
                    //Control.WriteVanillaClothingData(giPrefab);
                    Sliders.SetUpSliders(giPrefab);
                }
                else
                {
                    // hide UI
                    CTUIRoot.active = false;
                }
            }
        }
        */

        // Inventory Panel
        [HarmonyPatch(typeof(Panel_Inventory), nameof(Panel_Inventory.Enable), new Type[] { typeof(bool), typeof(bool) })]
        private static class InventoryPanelTracker
        {
            public static bool startTracking;

            internal static void Postfix(ref Panel_Inventory __instance, ref bool enable)
            {
                //if (__instance.m_FilterName != "GAMEPLAY_FilterClothing") return;
                startTracking = enable;

                if (enable)
                {
                    // instantiate and hide UI
                    SetupUI();
                    CTUIRoot.active = false;
                    ToggleWindow(false);

                }
                else
                {
                    // hide UI
                    UIInjection.currentlySelected = null;
                    CTUIRoot.active = false;

                    if (Settings.options.potatoMode > 0)
                    {
                        foreach (KeyValuePair<string, ClothingData> e in bigData)
                        {
                            e.Value.doUpdate = true;
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Panel_Inventory), nameof(Panel_Inventory.Update))]
        private static class UIInjection
        {
            public static GearItem? currentlySelected;

            internal static void Postfix(ref Panel_Inventory __instance)
            {
                if (!InventoryPanelTracker.startTracking) return;

                if (__instance.GetCurrentlySelectedItem()?.m_GearItem == currentlySelected) return;

                currentlySelected = __instance.GetCurrentlySelectedItem()?.m_GearItem;

                if (currentlySelected != null && currentlySelected.m_ClothingItem)
                {
                    // show UI
                    CTUIRoot.active = true;

                    GearItem giPrefab = GearItem.LoadGearItemPrefab(currentlySelected.name);
                    //Control.WriteVanillaClothingData(giPrefab);
                    Sliders.SetUpSliders(giPrefab);
                }
                else
                {
                    // hide UI
                    ToggleWindow(false);
                    CTUIRoot.active = false;
                }
            }
        }

        [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.SaveSceneData))]
        private static class SaveTweaks
        {
            internal static void Prefix(ref SlotData slot)
            {
                dataManager.Save(DataOperations.Serialize(), saveDataTag);
            }
        }


        [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.RestoreGlobalData))]
        private static class LoadTweaks
        {
            internal static void Prefix(ref string name)
            {
                string? serializedSaveData = dataManager.Load(saveDataTag);

                bigData = DataOperations.Deserialize(serializedSaveData);
            }
        }
    }
}
 