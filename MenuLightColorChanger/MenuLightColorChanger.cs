﻿using BS_Utils.Utilities;
using HMUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRUIControls;

namespace MenuLightColorChanger
{
    public class MenuLightColorChanger
    {
        public static bool isInited = false;

        private static ColorScheme currentColorScheme;

        public static ColorSchemesSettings colorSchemesSettings;

        public static ColorManager colorManager;
        private static MenuLightsManager menuLightsManager;
        private static LightWithIdManager bsLightManager;

        public static void Init()
        {
            var cospc = Resources.FindObjectsOfTypeAll<ColorsOverrideSettingsPanelController>().FirstOrDefault();
            var colorSchemeDropDown = cospc.GetPrivateField<ColorSchemeDropdownWithTableView>("_colorSchemeDropDown");
            var editColorSchemeController = cospc.GetPrivateField<EditColorSchemeController>("_editColorSchemeController");
            var overrideColorsToggle = cospc.GetPrivateField<Toggle>("_overrideColorsToggle");

            colorSchemeDropDown.didSelectCellWithIdxEvent += ColorChangeEvent;
            editColorSchemeController.didFinishEvent += ColorChangeEvent;
            editColorSchemeController.didChangeColorSchemeEvent += ColorChangeEvent;
            overrideColorsToggle.onValueChanged.AddListener(new UnityAction<bool>(ColorChangeEvent));

            HarmonyPatches.SongBrowerCreateUIPatch.SongBrowerUICreated += ChangeColors;
        }

        static IEnumerator ChangeColorsCoroutine(float time)
        {
            yield return new WaitForSeconds(time);

            ChangeColors();
        }

        private static void ColorChangeEvent(DropdownWithTableView dropDownWithTableView, int idx)
        {
            PersistentSingleton<SharedCoroutineStarter>.instance.StartCoroutine(ChangeColorsCoroutine(0.01f));
        }
        private static void ColorChangeEvent()
        {
            PersistentSingleton<SharedCoroutineStarter>.instance.StartCoroutine(ChangeColorsCoroutine(0.01f));
        }
        private static void ColorChangeEvent(bool flag)
        {
            PersistentSingleton<SharedCoroutineStarter>.instance.StartCoroutine(ChangeColorsCoroutine(0.01f));
        }
        private static void ColorChangeEvent(ColorScheme cs)
        {
            PersistentSingleton<SharedCoroutineStarter>.instance.StartCoroutine(ChangeColorsCoroutine(0.01f));
        }

        public static void LoadResources()
        {
            if (isInited)
                return;

            Init();

            colorManager = Resources.FindObjectsOfTypeAll<ColorManager>().FirstOrDefault();

            var playerDataModel = Resources.FindObjectsOfTypeAll<PlayerDataModelSO>().FirstOrDefault();
            colorSchemesSettings = playerDataModel.playerData.colorSchemesSettings;

            menuLightsManager = Resources.FindObjectsOfTypeAll<MenuLightsManager>().FirstOrDefault();
            bsLightManager = Resources.FindObjectsOfTypeAll<LightWithIdManager>().FirstOrDefault();

            var colorGradientSliders = Resources.FindObjectsOfTypeAll<ColorGradientSlider>();
            foreach (var cgs in colorGradientSliders)
            {
                var cgsChildren = cgs.transform.GetAllChildren();
                foreach (var cgsChild in cgsChildren)
                {
                    cgsChild.name += "_ColorGradientSlider";
                }
            }

            var editButton = Resources.FindObjectsOfTypeAll<Button>().Where(x => x.transition != Selectable.Transition.None && x.name == "EditButton").FirstOrDefault();
            editButton.transition = Selectable.Transition.ColorTint;
            editButton.image.color = Color.white.ColorWithAlpha(editButton.image.color.a);

            var editButtonObject = editButton.gameObject;
            GameObject.DestroyImmediate(editButtonObject.GetComponent<SignalOnUIButtonClick>());
            GameObject.DestroyImmediate(editButtonObject.GetComponent<Animator>());
            GameObject.DestroyImmediate(editButtonObject.GetComponents<HoverHint>().Last());


            isInited = true;
            Logger.log.Info("initialized!");
        }

        private static void SetMenuEnvironmentColors(ColorScheme cs)
        {
            var menuEnvLight3 = Resources.FindObjectsOfTypeAll<SimpleColorSO>().Where(x => x.name == "MenuEnvLight3").FirstOrDefault();
            var menuEnvLight1 = Resources.FindObjectsOfTypeAll<SimpleColorSO>().Where(x => x.name == "MenuEnvLight1").FirstOrDefault();

            menuEnvLight3.SetColor(cs.environmentColor1);
            menuEnvLight1.SetColor(cs.environmentColor0);

            Logger.log.Info("applied Environment colors");
        }

        private static void SetFlickeringNeonColor(FlickeringNeonSign flicker, Color color)
        {
            var prePassLight = flicker.GetPrivateField<TubeBloomPrePassLight>("_light");
            var lightType = prePassLight.GetBasePrivateField<BloomPrePassLightTypeSO>("_lightType");
            prePassLight.SetBasePrivateField("_registeredWithLightType", lightType);

            prePassLight.color = color;

            var sprite = flicker.GetPrivateField<SpriteRenderer>("_flickeringSprite");
            sprite.color = color;

            flicker.Start();

            foreach (Transform child in flicker.transform)
            {
                if (child.name.Contains("Sparks"))
                {
                    var particle = child.GetComponent<ParticleSystem>();

                    ParticleSystem.MinMaxGradient particleColor = new ParticleSystem.MinMaxGradient();
                    particleColor.mode = ParticleSystemGradientMode.Color;
                    particleColor.color = color;
                    ParticleSystem.MainModule main = particle.main;
                    main.startColor = particleColor;
                }
            }
        }

        private static void SetLogoColors(ColorScheme cs)
        {
            var logo = FindUnityObjectsHelper.GetAllGameObjectsInLoadedScenes().Find(x => x.name == "Logo");

            foreach (Transform t in logo.transform)
            {
                //Logger.log.Debug(t.name);
                //Logger.log.Debug(t.GetType().ToString());

                if (t.name == "BATNeon" || t.name == "SaberNeon")
                {
                    var prePassLight = t.GetComponent<TubeBloomPrePassLight>();

                    var lightType = prePassLight.GetBasePrivateField<BloomPrePassLightTypeSO>("_lightType");
                    prePassLight.SetBasePrivateField("_registeredWithLightType", lightType);

                    if (t.name == "BATNeon")
                        prePassLight.color = cs.environmentColor0;
                    if (t.name == "SaberNeon")
                        prePassLight.color = cs.environmentColor1;
                }

                if (t.name == "BatLogo" || t.name == "SaberLogo")
                {
                    var sprite = t.GetComponent<SpriteRenderer>();

                    if (t.name == "BatLogo")
                        sprite.color = cs.environmentColor0;

                    if (t.name == "SaberLogo")
                        sprite.color = cs.environmentColor1;
                }

                if (t.name == "EFlickering")
                {
                    var flicker = t.GetComponent<FlickeringNeonSign>();

                    SetFlickeringNeonColor(flicker, cs.environmentColor0);
                }
            }

            Logger.log.Info("applied Logo colors");
        }

        private static void SetMenuPlayersPlaceColors(ColorScheme cs)
        {
            var menuPlayersPlace = FindUnityObjectsHelper.GetAllGameObjectsInLoadedScenes().Find(x => x.name == "MenuPlayersPlace");

            foreach (Transform child in menuPlayersPlace.transform)
            {
                if (child.name == "Feet")
                {
                    var sprite = child.GetComponent<SpriteRenderer>();
                    sprite.color = cs.environmentColor1;
                }
                if (child.name == "RectangleFakeGlow")
                {
                    var fakeglow = child.GetComponent<RectangleFakeGlow>();
                    fakeglow.color = cs.environmentColor1.ColorWithAlpha(0.5f);
                }
            }

            Logger.log.Info("applied PlayerPlace colors");
        }

        private static void SetColoredTextIconColors(ColorScheme cs)
        {
            var coloredTextIcons = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(obj => obj.GetComponent<SegmentedControlCell>() != null)
                .Select(obj => obj.GetComponent<SegmentedControlCell>())
                .ToArray();

            foreach (var ct in coloredTextIcons)
            {
                //Logger.log.Debug(ct.name);

                if (ct.GetType() == typeof(TextSegmentedControlCellNew))
                {
                    //Color c;
                    //c = cp.GetPrivateField<Color>("_selectedTextColor");
                    ct.SetPrivateField("_selectedTextColor", cs.environmentColor1.makeLight(0.2f));
                    //c = cp.GetPrivateField<Color>("_highlightTextColor");
                    //cp.SetPrivateField("_highlightTextColor", overrideColorScheme.environmentColor1);
                    //c = cp.GetPrivateField<Color>("_selectedHighlightTextColor");
                    ct.SetPrivateField("_selectedHighlightTextColor", cs.environmentColor1.makeLight(0.2f));
                }

                if (ct.GetType() == typeof(IconSegmentedControlCell))
                {
                    //Color c;
                    //c = cp.GetPrivateField<Color>("_selectedIconColor");
                    ct.SetPrivateField("_selectedIconColor", cs.environmentColor1.makeLight(0.2f));
                    //c = cp.GetPrivateField<Color>("_highlightIconColor");
                    //cp.SetPrivateField("_highlightIconColor", overrideColorScheme.environmentColor1);
                    //c = cp.GetPrivateField<Color>("_selectedHighlightIconColor");
                    ct.SetPrivateField("_selectedHighlightIconColor", cs.environmentColor1.makeLight(0.2f));
                }

                //c = cp.GetPrivateField<Color>("_selectedBGColor");
                //cp.SetPrivateField("_selectedBGColor", overrideColorScheme.environmentColor1);
                //c = cp.GetPrivateField<Color>("_highlightBGColor");
                ct.SetPrivateField("_highlightBGColor", cs.environmentColor1.makeLight(0.2f).ColorWithAlpha(0.25f));
                //c = cp.GetPrivateField<Color>("_selectedHighlightBGColor");
                ct.SetPrivateField("_selectedHighlightBGColor", cs.environmentColor1.makeLight(0.2f).ColorWithAlpha(0.25f));


                ct.InvokePrivateMethod("RefreshVisuals", new object[] { });
            }

            Logger.log.Info("applied Text and Icon colors");
        }

        private static void SetColoredImageColors(ColorScheme cs)
        {
            var coloredImages = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Image>()
                .Where(image => (image.name == "Arrow" || image.name == "Icon" || image.name == "Highlight" || image.name == "Checkmark" || image.name == "Selection" || image.name == "Handle")
                                && image.color.ColorWithAlpha(1f) != Color.white && image.color.ColorWithAlpha(1f) != Color.black || (image.name == "Glow" && image.transform.parent.name != "GlowContainer"))
                .ToArray();

            foreach (var ci in coloredImages)
            {
                ci.color = cs.environmentColor1.makeLight(0.2f).ColorWithAlpha(ci.color.a);

                if (ci.name == "Glow")
                    ci.color = cs.environmentColor1.makeLight(0.1f).ColorWithAlpha(ci.color.a);
            }

            Logger.log.Info("applied Image colors");
        }

        private static void SetPointerColors(ColorScheme cs)
        {
            var pointer = Resources.FindObjectsOfTypeAll<VRPointer>().FirstOrDefault();
            pointer.InvokePrivateMethod("DestroyLaserAndHit", Array.Empty<object>());
            pointer.InvokePrivateMethod("CreateLaserPointerAndLaserHit", Array.Empty<object>());
            Logger.log.Info("applied Pointer colors");
        }

        private static void SetSliderColors(ColorScheme cs)
        {
            var sliders = Resources.FindObjectsOfTypeAll<TextSlider>();
            foreach (var slider in sliders)
            {
                var cb = slider.colors;
                cb.highlightedColor = cs.environmentColor1.ColorWithAlpha(cb.highlightedColor.a);
                cb.pressedColor = cs.environmentColor1.ColorWithAlpha(cb.highlightedColor.a);
                slider.colors = cb;
            }
            Logger.log.Info("applied Slider colors");
        }

        private static void SetToggleColors(ColorScheme cs)
        {
            var toggles = Resources.FindObjectsOfTypeAll<Toggle>()
                .Where(x => (x.image.name == "BG" || x.image.name == "Background") && x.transform.parent.parent?.name != "LevelDetail(Clone)").ToArray();
            foreach (var toggle in toggles)
            {
                var cb = toggle.colors;
                cb.highlightedColor = cs.environmentColor1.ColorWithAlpha(cb.highlightedColor.a);
                cb.pressedColor = cs.environmentColor1.ColorWithAlpha(cb.highlightedColor.a);
                toggle.colors = cb;
            }
            Logger.log.Info("applied Toggle colors");
        }

        private static void SetAnimationClipColors(ColorScheme cs)
        {
            var animationClips = Resources.FindObjectsOfTypeAll<AnimationClip>().Where(x => x.name.Contains("Highlight")).ToArray();
            foreach (var animationClip in animationClips)
            {
                Utils.SetAnimationCurveColor(animationClip, cs.environmentColor1, typeof(UnityEngine.UI.Image), "m_Color", "BG");
            }
            Logger.log.Info("applied AnimationClip colors");
        }

        private static void SetButtonColors(ColorScheme cs)
        {
            var buttons = Resources.FindObjectsOfTypeAll<Button>().Where(x => x.transition != Selectable.Transition.None).ToArray();
            foreach (var button in buttons)
            {
                var cb = button.colors;
                cb.highlightedColor = cs.environmentColor1.ColorWithAlpha(cb.highlightedColor.a);
                cb.pressedColor = cs.environmentColor1.ColorWithAlpha(cb.pressedColor.a);
                button.colors = cb;
            }
            Logger.log.Info("applied Button colors");
        }

        private static void SetTMPUGUIColors(ColorScheme cs)
        {
            var TMPUGUIes = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>()
            .Where(x => x.color.ColorWithAlpha(1f) != Color.white && x.color.ColorWithAlpha(1f) != Color.black)
            .Where(y => y.name == "ScoreText" || y.name == "RankText" || y.transform.parent?.name == "DropDown" || y.transform.parent?.name == "HeaderPanel")
            .ToArray();
            foreach (var TMPUGUI in TMPUGUIes)
            {
                TMPUGUI.color = cs.environmentColor1.ColorWithAlpha(TMPUGUI.color.a);

                if (TMPUGUI.transform.parent.name == "HeaderPanel")
                    TMPUGUI.color = cs.environmentColor1.makeLight(0.294f).ColorWithAlpha(TMPUGUI.color.a);
            }
            Logger.log.Info("applied TextMeshProUGUI colors");
        }

        private static void SetMissionToggleColors(ColorScheme cs)
        {
            var missionToggles = Resources.FindObjectsOfTypeAll<MissionToggle>();
            foreach (var missionToggle in missionToggles)
            {
                missionToggle.SetPrivateField("_highlightColor", cs.environmentColor1);
            }
            Logger.log.Info("applied MissionToggle colors");
        }

        private static void SetTableCellColors(ColorScheme cs)
        {
            var tableCells = Resources.FindObjectsOfTypeAll<TableCell>().Where(x => x.ContainPrivateField<Color>("_selectedHighlightElementsColor")).ToArray();
            foreach (var tableCell in tableCells)
            {
                tableCell.SetPrivateField("_selectedHighlightElementsColor", cs.environmentColor1);
            }
            Logger.log.Info("applied TableCell colors");
        }

        private static void SetFireWorkColors(ColorScheme cs)
        {
            var fireWorkItemControllers = Resources.FindObjectsOfTypeAll<FireworkItemController>();
            foreach (var fireworks in fireWorkItemControllers)
            {
                fireworks.SetPrivateField("_lightsColor", cs.environmentColor1);
                var particle = fireworks.GetPrivateField<ParticleSystem>("_particleSystem");

                var main = particle.main;
                main.startColor = new ParticleSystem.MinMaxGradient(main.startColor.colorMin, cs.environmentColor1);
            }
            Logger.log.Info("applied Firework colors");
        }

        public static void ChangeColors()
        {
            colorManager = Resources.FindObjectsOfTypeAll<ColorManager>().FirstOrDefault();

            var playerDataModel = Resources.FindObjectsOfTypeAll<PlayerDataModelSO>().FirstOrDefault();
            colorSchemesSettings = playerDataModel.playerData.colorSchemesSettings;

            menuLightsManager = Resources.FindObjectsOfTypeAll<MenuLightsManager>().FirstOrDefault();
            bsLightManager = Resources.FindObjectsOfTypeAll<LightWithIdManager>().FirstOrDefault();

            var overrideColorScheme = colorSchemesSettings.overrideDefaultColors ? colorSchemesSettings.GetSelectedColorScheme() : colorManager.GetField<ColorSchemeSO>("_defaultColorScheme").colorScheme;

            Logger.log.Info("selected:" + overrideColorScheme.colorSchemeName);

            colorManager.SetPrivateField("_colorScheme", overrideColorScheme);

            SetMenuEnvironmentColors(overrideColorScheme);

            SetLogoColors(overrideColorScheme);
            SetMenuPlayersPlaceColors(overrideColorScheme);

            SetColoredTextIconColors(overrideColorScheme);
            SetColoredImageColors(overrideColorScheme);

            SetSliderColors(overrideColorScheme);
            SetToggleColors(overrideColorScheme);
            SetAnimationClipColors(overrideColorScheme);
            SetButtonColors(overrideColorScheme);
            SetTMPUGUIColors(overrideColorScheme);
            SetMissionToggleColors(overrideColorScheme);
            SetTableCellColors(overrideColorScheme);
            SetFireWorkColors(overrideColorScheme);

            colorManager.Start();
            menuLightsManager.RefreshColors();

            currentColorScheme = overrideColorScheme;

            Logger.log.Info("applied all colors");
        }
    }
}