using Base.Core;
using Base.UI;
using Harmony;
using PhoenixPoint.Common.Core;
using PhoenixPoint.Common.Entities.Addons;
using PhoenixPoint.Common.Entities.GameTags;
using PhoenixPoint.Geoscape.Entities;
using PhoenixPoint.Geoscape.View.ViewModules;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace pantolomin.phoenixPoint.fullMutation
{
    public class Mod
    {
        public static void Init()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create(typeof(Mod).Namespace);
            // Dirty: We re-execute the method
            Mod.Patch(harmonyInstance, typeof(UIModuleMutate), "InitCharacterInfo", "Prevent_InitCharacterInfo", "Override_InitCharacterInfo");
        }

        // ******************************************************************************************************************
        // ******************************************************************************************************************
        // Patched methods
        // ******************************************************************************************************************
        // ******************************************************************************************************************

        private const int MAX_MUTATIONS = 3;

        public static bool Prevent_InitCharacterInfo()
        {
            // Skip the method completely -> Postfix replaces the body
            return false;
        }

        public static void Override_InitCharacterInfo(UIModuleMutate __instance,
            GeoCharacter ____currentCharacter,
            ref int ____currentCharacterMutations,
            Dictionary<AddonSlotDef, UIModuleMutationSection> ____mutationSections)
        {
            int i;
            ____currentCharacterMutations = 0;
            GameTagDef anuMutationTag = GameUtl.GameComponent<SharedData>().SharedGameTags.AnuMutationTag;
            foreach (GeoItem armourItem in ____currentCharacter.ArmourItems)
            {
                if (!armourItem.ItemDef.Tags.Contains(anuMutationTag))
                {
                    continue;
                }
                ____currentCharacterMutations++;
            }
            foreach (KeyValuePair<AddonSlotDef, UIModuleMutationSection> mutationSection in ____mutationSections)
            {
                mutationSection.Value.ResetContainer(____currentCharacterMutations < MAX_MUTATIONS);
            }
            foreach (GeoItem geoItem in ____currentCharacter.ArmourItems)
            {
                if (!geoItem.ItemDef.Tags.Contains(anuMutationTag))
                {
                    continue;
                }
                AddonDef.RequiredSlotBind[] requiredSlotBinds = geoItem.ItemDef.RequiredSlotBinds;
                for (i = 0; i < (int)requiredSlotBinds.Length; i++)
                {
                    AddonDef.RequiredSlotBind requiredSlotBind = requiredSlotBinds[i];
                    if (____mutationSections.ContainsKey(requiredSlotBind.RequiredSlot))
                    {
                        ____mutationSections[requiredSlotBind.RequiredSlot].SetMutationUsed(geoItem.ItemDef);
                    }
                }
            }
            string str = __instance.XoutOfY.Localize(null);
            str = str.Replace("{0}", ____currentCharacterMutations.ToString());
            str = str.Replace("{1}", MAX_MUTATIONS.ToString());
            __instance.MutationsAvailableValue.text = str;
            __instance.MutationsAvailableValue.GetComponent<UIColorController>().SetWarningActive(MAX_MUTATIONS <= ____currentCharacterMutations, false);
        }

        // ******************************************************************************************************************
        // ******************************************************************************************************************
        // Utility methods
        // ******************************************************************************************************************
        // ******************************************************************************************************************

        private static void Patch(HarmonyInstance harmony, Type target, string toPatch, string prefix, string postfix = null, string transpiler = null)
        {
            harmony.Patch(target.GetMethod(toPatch, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
                Mod.ToHarmonyMethod(prefix), Mod.ToHarmonyMethod(postfix), Mod.ToHarmonyMethod(transpiler));
        }

        private static HarmonyMethod ToHarmonyMethod(string name)
        {
            if (name == null)
            {
                return null;
            }
            MethodInfo method = typeof(Mod).GetMethod(name);
            if (method == null)
            {
                throw new NullReferenceException(string.Concat("No method for name: ", name));
            }
            return new HarmonyMethod(method);
        }

        private static object getFieldValue(Type clazz, string fieldName, object instance)
        {
            FieldInfo field = clazz.GetField(fieldName);
            if (field == null)
            {
                throw new NullReferenceException(string.Concat("No field for name: ", fieldName));
            }
            return field.GetValue(instance);
        }
    }
}