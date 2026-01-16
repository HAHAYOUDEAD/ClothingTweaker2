namespace CT2
{
    public class Control
    {
        public static GameObject CTUIRoot;
        public static Button MainButton;
        public static Button ResetButton;
        public static GameObject MainWindow;

        public static bool initialSetup = true;
        public static bool resetPressed;
        public static bool liveClothingPanelUpdate;

        public static Vector2 mainWindowAnchor;
        public static Vector2 mainButtonAnchor;
        public static Vector2 resetButtonAnchor;

        public static void SetupUI()
        {
            if (CTUIRoot == null)
            {
                CTUIRoot = GameObject.Instantiate(UIBundle.LoadAsset<GameObject>("CTUICanvas"));
                GameObject.DontDestroyOnLoad(CTUIRoot);
                MainButton = CTUIRoot.transform.Find("MainButton").GetComponent<Button>();
                ResetButton = CTUIRoot.transform.Find("ResetButton").GetComponent<Button>();
                MainWindow = CTUIRoot.transform.Find("Window").gameObject;
                MainButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(MainButtonAction)));
                ResetButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(ResetButtonAction)));
                MainWindow.active = false;
                ResetButton.gameObject.active = false;
                mainWindowAnchor = MainWindow.GetComponent<RectTransform>().anchoredPosition;
                mainButtonAnchor = MainButton.GetComponent<RectTransform>().anchoredPosition;
                resetButtonAnchor = ResetButton.GetComponent<RectTransform>().anchoredPosition;

            }

            if (IsUltraWide(out _))
            {
                float ultraWideOffset = CalculateUltraWideOffset();
                OffsetUIForUltraWide(ultraWideOffset);
                AdjustForUltraWide(true);
            }
            else
            {
                OffsetUIForUltraWide(0f);
                AdjustForUltraWide(false);
            }
        }

        //public static GearItem GetSelectedClothing() => InterfaceManager.GetPanel<Panel_Clothing>().GetCurrentlySelectedGearItem();

        public static bool IsWindowEnabled()
        {
            if (CTUIRoot)
            {
                if (MainWindow)
                {
                    return MainWindow.active;
                }
                else
                {
                    return false;
                }
            }

            Log(CC.Red, "CTUI not initialized. WindowEnabled");
            return false;
        }

        public static void ToggleWindow(bool enable)
        {
            if (CTUIRoot)
            {
                // Clothing Panel
                /* 
                Panel_Clothing pc = InterfaceManager.GetPanel<Panel_Clothing>();
                Transform rs = pc.transform.Find("NonPaperDoll/Right side");
                MelonCoroutines.Start(NudgeVanillaUI(rs.Find("Coverflow/ChangeItemButtons"), 227f, !enable));
                MelonCoroutines.Start(NudgeVanillaUI(rs.Find("Coverflow/ItemScrollList"), 30f, !enable));
                rs.Find("NumItemsLabel").gameObject.active = !enable;
                rs.Find("GearStatsBlock(Clone)/ItemDescriptionLabel").gameObject.active = !enable;
                */

                // Inventory Panel
                Panel_Inventory pi = InterfaceManager.GetPanel<Panel_Inventory>();
                Transform rs = pi.transform.Find("Right Side");
                MelonCoroutines.Start(NudgeVanillaUI(rs.Find("GearItem"), 30f, !enable));
                rs.Find("GearStatsBlock(Clone)/ItemDescriptionLabel").gameObject.active = !enable;

                MainWindow.active = enable;
                ResetButton.gameObject.active = enable;
            }
        }

        public static IEnumerator NudgeVanillaUI(Transform element, float amount, bool reset)
        {
            if (reset)
            {
                element.localPosition = Vector3.zero;
                yield break;
            }

            if (Settings.options.potatoMode == 2)
            {
                element.localPosition = Vector3.up * amount;
                yield break;
            }

            float t = 0f;
            Vector3 startpos = Vector3.zero;
            Vector3 endpos = Vector3.up * amount;

            while (t <= 1f)
            {
                t += Time.deltaTime / 0.1f;
                element.localPosition = Vector3.Lerp(startpos, endpos, Mathf.Pow(t - 1, 3f) + 1);
                yield return new WaitForEndOfFrame();
            }
            yield break;
        }

        public static void MainButtonAction() => ToggleWindow(!IsWindowEnabled());

        public static void ResetButtonAction()
        {
            GearItem gi = InterfaceManager.GetPanel<Panel_Inventory>().GetCurrentlySelectedItem().m_GearItem;

            MelonCoroutines.Start(ResetButtonCoroutine(gi));
        }

        public static IEnumerator ResetButtonCoroutine(GearItem gi)
        {
            bigData[gi.name] = new();
            liveClothingPanelUpdate = true;
            resetPressed = true;
            bigData[gi.name].doUpdate = true;
            float timer = 0f; 

            foreach (Stat st in Enum.GetValues(typeof(Stat)))
            {
                if (st == Stat.Undefined) continue;
                Sliders.SetSliderValue(st, Sliders.GetSliderByType(st, true).value, false);
            }

            yield return new WaitForEndOfFrame();

            while (bigData[gi.name].updateCounter > 0 || coroutineRunning > 0)
            {
                timer += Time.deltaTime;
                if (timer > 1f) break;
                yield return null;
            }

            resetPressed = false;
            liveClothingPanelUpdate = false;
            bigData[gi.name].doUpdate = false;
            bigData.Remove(Utils.SanitizePrefabName(gi.name));
            yield break;
        }

        public static float GetSpecificVanillaValue(string gearName, Stat st)
        {
            GearItem giPrefab = GearItem.LoadGearItemPrefab(gearName);
            ClothingItem ci = giPrefab.GetComponent<ClothingItem>();
            if (ci == null) return 0f;

            if (st == Stat.Warmth) return ci.m_Warmth;
            if (st == Stat.WarmthWet) return ci.m_WarmthWhenWet;
            if (st == Stat.Windproof) return ci.m_Windproof;
            if (st == Stat.Waterproof) return ci.m_Waterproofness;
            if (st == Stat.Protection) return ci.m_Toughness;
            if (st == Stat.Mobility) return ci.m_SprintBarReductionPercent;
            if (st == Stat.Weight) return giPrefab.GearItemData.BaseWeightKG.ToQuantity(1f);

            return 0f;
        }

        public static IEnumerator LetAllInstancesUpdate(string name)
        {
            bigData[name].updateCounter = 0;
            bigData[name].doUpdate = true;
            yield return new WaitForEndOfFrame();
            while (bigData[name].updateCounter > 0)
            {
                yield return null;
            }
            bigData[name].doUpdate = false;
            resetPressed = false;
            yield break;
        }

        public static void UpdateDecayRates(ClothingItem ci)
        {
            ci.m_GearItem.GearItemData.m_DailyHPDecay *= Settings.options.clothingDecayDaily;
            // ^ check if applied to prefab, because it would then be applied multiple times
            ci.m_DailyHPDecayWhenWornInside *= Settings.options.clothingDecayIndoors;
            ci.m_DailyHPDecayWhenWornOutside *= Settings.options.clothingDecayOutdoors;
        }


        public static void AdjustForUltraWide(bool isUltraWide)
        {
            CTUIRoot.GetComponent<CanvasScaler>().m_MatchWidthOrHeight = isUltraWide ? 1f : 0f;
        }

        public static float CalculateUltraWideOffset()
        {
            if (IsUltraWide(out float aspect))
            {
                float refWidth = 1920f;
                float refHeight = 1080f;

                float canvasWidth = refHeight * aspect;

                return (canvasWidth - refWidth) / 2f;
            }
            return 0f;
        }

        public static bool IsUltraWide(out float aspect)
        {
            float screenRatio = (float)Screen.width / (float)Screen.height;
            aspect = screenRatio;
            return screenRatio > 2.2f;
        }

        public static void OffsetUIForUltraWide(float offset)
        {
            Vector2 pos1 = mainWindowAnchor;
            Vector2 pos2 = mainButtonAnchor;
            Vector2 pos3 = resetButtonAnchor;

            pos1.x -= offset;
            pos2.x -= offset;
            pos3.x -= offset;

            MainWindow.GetComponent<RectTransform>().anchoredPosition = pos1;
            MainButton.GetComponent<RectTransform>().anchoredPosition = pos2;
            ResetButton.GetComponent<RectTransform>().anchoredPosition = pos3;
        }

    }
}




