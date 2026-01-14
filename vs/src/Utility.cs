global using CC = System.ConsoleColor;
global using Il2Cpp;
global using System;
global using MelonLoader;
global using HarmonyLib;
global using UnityEngine;
global using System.Reflection;
global using System.Collections;
global using System.Collections.Generic;
global using Il2CppVLB;
global using Utils = Il2Cpp.Utils;
global using ModData;
global using Il2CppInterop.Runtime;
global using Il2CppTLD.IntBackedUnit;
global using UnityEngine.Events;
global using UnityEngine.UI;
global using UnityEngine.EventSystems;
global using TLD.TinyJSON;

global using static CT2.Utility;
global using static CT2.Main;
global using static CT2.Control;

namespace CT2
{
    internal class Utility
    {

        public static bool IsScenePlayable()
        {
            return !(string.IsNullOrEmpty(GameManager.m_ActiveScene) || GameManager.m_ActiveScene.Contains("MainMenu") || GameManager.m_ActiveScene == "Boot" || GameManager.m_ActiveScene == "Empty");
        }

        public static bool IsScenePlayable(string scene)
        {
            return !(string.IsNullOrEmpty(scene) || scene.Contains("MainMenu") || scene == "Boot" || scene == "Empty");
        }

        public static bool IsMainMenu(string scene)
        {
            return !string.IsNullOrEmpty(scene) && scene.Contains("MainMenu");
        }

        public static AssetBundle? LoadEmbeddedAssetBundle(string name)
        {
            using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Main.resourcesFolder + name))
            {
                MemoryStream? memory = new((int)stream.Length);
                stream!.CopyTo(memory);

                Il2CppSystem.IO.MemoryStream memoryStream = new(memory.ToArray());
                return AssetBundle.LoadFromStream(memoryStream);
            };
        }

        public static void Log(CC color, string message)
        {
            if (Settings.options.log)
            {
                MelonLogger.Msg(color, message);
            }
        }

    }
}
