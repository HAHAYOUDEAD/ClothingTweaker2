namespace CT2
{
    public class Sliders
    {

        public static Dictionary<Stat, Slider> slidersDict = new();
        public static Dictionary<Stat, Slider> slidersDefDict = new();

        public static readonly Color colorWhite = new Color(0.98f, 0.98f, 0.98f, 1f);
        public static readonly Color colorGreen = new Color(0.431f, 0.514f, 0.486f, 1f);

        public static readonly float tempMultiplier = 10f;
        public static readonly float wetMultiplier = 100f;
        public static readonly float weightMultiplier = 20f;

        public static readonly float maxWarmth = 10f;
        public static readonly float maxWarmthWet = 10f;
        public static readonly float maxWindproof = 10f;
        public static readonly float maxWaterproof = 100f;
        public static readonly float maxProtection = 50f;
        public static readonly float maxMobility = 25f;
        public static readonly float maxWeight = 6f;

        public static Slider GetSliderByType(Stat st, bool def = false)
        {
            if (def)
            {
                bool exists = slidersDefDict.ContainsKey(st) && slidersDefDict[st] != null;

                if (!exists)
                {
                    slidersDefDict[st] = MainWindow.transform.Find("HLayout/" + st.ToString() + "/SliderDefault").GetComponent<Slider>();
                }

                return slidersDefDict[st];
            }

            else
            {
                bool exists = slidersDict.ContainsKey(st) && slidersDict[st] != null;

                if (!exists)
                {
                    slidersDict[st] = MainWindow.transform.Find("HLayout/" + st.ToString() + "/Slider").GetComponent<Slider>();
                }

                return slidersDict[st];
            }
        }

        public static void InitializeSlider(Stat st, float min, float max, GearItem gi, bool isInt = true)
        {
            Slider slider = GetSliderByType(st);
            Slider sliderDefault = GetSliderByType(st, true);
            slider.minValue = min;
            sliderDefault.minValue = min;
            slider.maxValue = max;
            sliderDefault.maxValue = max;
            slider.wholeNumbers = isInt;
            sliderDefault.wholeNumbers = isInt;

            slider.transform.GetParent().Find("Label").GetComponent<Il2CppTMPro.TMP_Text>().font = GameManager.GetFontManager().GetTMPFontForCharacterSet(Localization.GetCharacterSet());

            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(DelegateSupport.ConvertDelegate<UnityAction<float>>(new Action<float>(delegate (float value) { SetLabelFromSliderValue(st); })));
            slider.onValueChanged.AddListener(DelegateSupport.ConvertDelegate<UnityAction<float>>(new Action<float>(delegate (float value) { SetSliderHandleColor(st); })));
            slider.onValueChanged.AddListener(DelegateSupport.ConvertDelegate<UnityAction<float>>(new Action<float>(delegate (float value) { WriteTweakedClothingData(st, gi); })));
        }


        public static void SetUpSliders(GearItem gi)
        {
            if (CTUIRoot == null)
            {
                Log(CC.Red, "CTUI not initialized. SetUpSliders");
                return;
            }

            Stat st;
            string name = gi.name;
            bool fromPrefab = !bigData.ContainsKey(name);

            SetUpIndividualSlider(gi, Stat.Warmth, 0f, maxWarmth * tempMultiplier, tempMultiplier);
            SetUpIndividualSlider(gi, Stat.WarmthWet, 0f, maxWarmthWet * tempMultiplier, tempMultiplier);
            SetUpIndividualSlider(gi, Stat.Windproof, 0f, maxWindproof * tempMultiplier, tempMultiplier);
            SetUpIndividualSlider(gi, Stat.Waterproof, 0f, maxWaterproof, wetMultiplier);
            SetUpIndividualSlider(gi, Stat.Protection, 0f, maxProtection);
            SetUpIndividualSlider(gi, Stat.Mobility, 0f, maxMobility);
            SetUpIndividualSlider(gi, Stat.Weight, 1f, maxWeight * weightMultiplier, weightMultiplier);

            if (initialSetup) initialSetup = false;
        }

        public static void SetUpIndividualSlider(GearItem gi, Stat st, float min, float max, float multiplier = 1f)
        {
            string name = gi.name;

            InitializeSlider(st, min, max, gi);
            float valueVanilla = GetSpecificVanillaValue(name, st);
            float value = 0f;

            switch (st)
            {
                case Stat.Warmth:
                    value = bigData.ContainsKey(name) ? bigData[name].warmth ?? valueVanilla : valueVanilla; // if key exists and has tweaked value - use it, otherwise vanilla
                    break;
                case Stat.WarmthWet:
                    value = bigData.ContainsKey(name) ? bigData[name].warmthWet ?? valueVanilla : valueVanilla;
                    break;
                case Stat.Windproof:
                    value = bigData.ContainsKey(name) ? bigData[name].windproof ?? valueVanilla : valueVanilla;
                    break;
                case Stat.Waterproof:
                    value = bigData.ContainsKey(name) ? bigData[name].waterproof ?? valueVanilla : valueVanilla;
                    break;
                case Stat.Protection:
                    value = bigData.ContainsKey(name) ? bigData[name].protection ?? valueVanilla : valueVanilla;
                    break;
                case Stat.Mobility:
                    value = bigData.ContainsKey(name) ? bigData[name].mobility ?? valueVanilla : valueVanilla;
                    break;
                case Stat.Weight:
                    value = bigData.ContainsKey(name) ? bigData[name].weight ?? valueVanilla : valueVanilla;
                    break;
            }

            valueVanilla *= multiplier;
            value *= multiplier;

            SetSliderValue(st, valueVanilla, true, initialSetup);
            SetSliderValue(st, value, false, initialSetup);
        }

        public static void SetSliderValue(Stat st, float value, bool def, bool initial = false) => MelonCoroutines.Start(FancySliderMovement(st, value, def, initial));
        public static IEnumerator FancySliderMovement(Stat st, float to, bool def, bool initial)
        {
            Slider slider = GetSliderByType(st, def);

            if (Settings.options.potatoMode == 2 || initial)
            {
                //slider.value = to;
                slider.SetValueWithoutNotify(to);
                SetLabelFromSliderValue(st);
            }
            else
            {
                float from = slider.value;

                float t = 0f;

                while (t <= 1f)
                {
                    t += Time.deltaTime / 0.1f;
                    //slider.value = Mathf.Lerp(from, to, Mathf.Pow(t - 1, 3f) + 1);
                    slider.SetValueWithoutNotify(Mathf.Lerp(from, to, Mathf.Pow(t - 1, 3f) + 1));
                    SetLabelFromSliderValue(st);
                    yield return new WaitForEndOfFrame();
                }
            }

            if (!def) SetSliderHandleColor(st);

            yield break;
        }

        public static void SetLabelFromSliderValue(Stat st)
        {
            Slider slider = GetSliderByType(st);
            string label = "? ? ?";
            switch (st)
            {
                case Stat.Waterproof:
                    label = slider.value + "%";
                    break;
                case Stat.Protection:
                    label = slider.value + "%";
                    break;
                case Stat.Mobility:
                    label = (slider.value > 0 ? "-" : "") + slider.value + "%";
                    break;
                case Stat.Weight:
                    label = ItemWeight.FromKilograms(slider.value / weightMultiplier).ToFormattedStringWithUnits();
                    break;

                default:
                    label = Utils.GetTemperatureString(slider.value / tempMultiplier, true, false, slider.value / tempMultiplier > 0);
                    break;
            }

            slider.transform.GetParent().Find("Label").GetComponent<Il2CppTMPro.TMP_Text>().text = label;
        }

        public static void SetSliderHandleColor(Stat st)
        {
            Slider slider = GetSliderByType(st);
            Slider sliderDef = GetSliderByType(st, true);

            if ((int)slider.value == (int)sliderDef.value)
            {
                ColorBlock cb = slider.colors;
                cb.normalColor = colorGreen;
                cb.pressedColor = colorGreen;
                slider.colors = cb;
            }
            else
            {
                ColorBlock cb = slider.colors;
                cb.normalColor = colorWhite;
                cb.pressedColor = colorWhite;
                slider.colors = cb;
            }
        }

        public static void WriteTweakedClothingData(Stat st, GearItem gi)
        {
            if (initialSetup) return;
            if (!IsWindowEnabled()) return;

            Slider slider = GetSliderByType(st);
            Slider sliderDefault = GetSliderByType(st, true);

            string name = gi.name;

            if (!bigData.ContainsKey(name)) // create new dict entry on first slider change
            {
                if (slider.value == sliderDefault.value) return; // don't write unnecessary data
                bigData[name] = new ClothingData();
            }

            switch (st)
            {
                case Stat.Warmth:
                    if (slider.value == sliderDefault.value) bigData[name].warmth = default; // erase data when value is vanilla
                    else bigData[name].warmth = slider.value / tempMultiplier; // save changed value
                    break;
                case Stat.WarmthWet:
                    if (slider.value == sliderDefault.value) bigData[name].warmthWet = default;
                    else bigData[name].warmthWet = slider.value / tempMultiplier;
                    break;
                case Stat.Windproof:
                    if (slider.value == sliderDefault.value) bigData[name].windproof = default;
                    else bigData[name].windproof = slider.value / tempMultiplier;
                    break;
                case Stat.Waterproof:
                    if (slider.value == sliderDefault.value) bigData[name].waterproof = default;
                    else bigData[name].waterproof = slider.value / wetMultiplier;
                    break;
                case Stat.Protection:
                    if (slider.value == sliderDefault.value) bigData[name].protection = default;
                    else bigData[name].protection = slider.value;
                    break;
                case Stat.Mobility:
                    if (slider.value == sliderDefault.value) bigData[name].mobility = default;
                    else bigData[name].mobility = slider.value;
                    break;
                case Stat.Weight:
                    if (slider.value == sliderDefault.value) bigData[name].weight = default;
                    else bigData[name].weight = slider.value / weightMultiplier;
                    break;
            }

            if (Settings.options.potatoMode == 0)
            {
                bigData[name].lastChangedStat = st;
                if (bigData[name].coroutine != null)
                {
                    MelonCoroutines.Stop(bigData[name].coroutine);
                }
                bigData[name].coroutine = MelonCoroutines.Start(LetAllInstancesUpdate(name));
                //bigData[name].doUpdate = true;
                liveClothingPanelUpdate = true;
            }
        }
    }
}
