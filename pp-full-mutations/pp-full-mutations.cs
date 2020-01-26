using Base.Core;
using Base.UI;
using Harmony;
using PhoenixPoint.Common.Core;
using PhoenixPoint.Common.Entities.Addons;
using PhoenixPoint.Common.Entities.GameTags;
using PhoenixPoint.Geoscape.Entities;
using PhoenixPoint.Geoscape.View.ViewModules;
using PhoenixPointModLoader;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace pantolomin.phoenixPoint.fullMutation
{
    public class Mod: IPhoenixPointMod
    {
        public ModLoadPriority Priority => ModLoadPriority.Low;

        public void Initialize()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create(typeof(Mod).Namespace);
            // Dirty: We re-execute the method
            Patch(harmonyInstance, typeof(UIModuleMutate), "InitCharacterInfo", "Override_InitCharacterInfo");
        }

        // ******************************************************************************************************************
        // ******************************************************************************************************************
        // Patched methods
        // ******************************************************************************************************************
        // ******************************************************************************************************************

        private const int MAX_MUTATIONS = 3;

        public static bool Override_InitCharacterInfo(UIModuleMutate __instance,
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

            // Skip the method
            return false;
        }

        // ******************************************************************************************************************
        // ******************************************************************************************************************
        // Utility methods
        // ******************************************************************************************************************
        // ******************************************************************************************************************

        private void Patch(HarmonyInstance harmony, Type target, string toPatch, string prefix, string postfix = null)
        {
            harmony.Patch(target.GetMethod(toPatch, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
                ToHarmonyMethod(prefix), ToHarmonyMethod(postfix), null);
        }

        private HarmonyMethod ToHarmonyMethod(string name)
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
    }
}