﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IPA;
using IPA.Config;
using IPA.Utilities;
using Harmony;
using UnityEngine.SceneManagement;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using BS_Utils.Utilities;

namespace MenuLightColorChanger
{
    public class Plugin : IBeatSaberPlugin
    {
        // TODO: Change YourGitHub to the name of your GitHub account, or use the form "com.company.project.product"
        public const string HarmonyId = "com.github.kurenai3110.MenuLightColorChanger";
        public const string SongCoreHarmonyId = "com.kyle1413.BeatSaber.SongCore";
        internal static HarmonyInstance harmony;
        internal static string Name => "MenuLightColorChanger";
        internal static Ref<PluginConfig> config;
        internal static IConfigProvider configProvider;

        public void Init(IPALogger logger, [IPA.Config.Config.Prefer("json")] IConfigProvider cfgProvider)
        {
            Logger.log = logger;
            Logger.log.Debug("Logger initialized.");

            configProvider = cfgProvider;

            config = configProvider.MakeLink<PluginConfig>((p, v) =>
            {
                // Build new config file if it doesn't exist or RegenerateConfig is true
                if (v.Value == null || v.Value.RegenerateConfig)
                {
                    Logger.log.Debug("Regenerating PluginConfig");
                    p.Store(v.Value = new PluginConfig()
                    {
                        // Set your default settings here.
                        RegenerateConfig = false
                    });
                }
                config = v;
            });
            harmony = HarmonyInstance.Create(HarmonyId);
        }

        /// <summary>
        /// Attempts to apply all the Harmony patches in this assembly.
        /// </summary>
        public static void ApplyHarmonyPatches()
        {
            try
            {
                Logger.log.Debug("Applying Harmony patches.");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Logger.log.Critical("Error applying Harmony patches: " + ex.Message);
                Logger.log.Debug(ex);
            }
        }

        /// <summary>
        /// Attempts to remove all the Harmony patches that used our HarmonyId.
        /// </summary>
        public static void RemoveHarmonyPatches()
        {
            try
            {
                // Removes all patches with this HarmonyId
                harmony.UnpatchAll(HarmonyId);
            }
            catch (Exception ex)
            {
                Logger.log.Critical("Error removing Harmony patches: " + ex.Message);
                Logger.log.Debug(ex);
            }
        }

        /// <summary>
        /// Called when the active scene is changed.
        /// </summary>
        /// <param name="prevScene">The scene you are transitioning from.</param>
        /// <param name="nextScene">The scene you are transitioning to.</param>
        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {

        }

        /// <summary>
        /// Called when the a scene's assets are loaded.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="sceneMode"></param>
        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {



        }


        public void OnApplicationQuit()
        {
            Logger.log.Debug("OnApplicationQuit");

        }

        /// <summary>
        /// Runs at a fixed intervalue, generally used for physics calculations. 
        /// </summary>
        public void OnFixedUpdate()
        {

        }

        /// <summary>
        /// This is called every frame.
        /// </summary>
        public void OnUpdate()
        {

        }


        public void OnSceneUnloaded(Scene scene)
        {

        }


        /// <summary>
        /// This should not be used with an IDisablable plugin. 
        /// It will not be called if the plugin starts disabled and is enabled while the game is running.
        /// </summary>
        public void OnApplicationStart()
        {
            BSEvents.OnLoad();
            BSEvents.menuSceneLoadedFresh += MenuSceneLoadedFresh;
        }

        private void MenuSceneLoadedFresh()
        {
            MenuLightColorChanger.LoadResources();
            MenuLightColorChanger.ChangeColors();
        }
    }
}