using Base.Core;
using Base.UI;
using Harmony;
using PhoenixPoint.Common.Core;
using PhoenixPoint.Common.Entities.Addons;
using PhoenixPoint.Common.Entities.GameTags;
using PhoenixPoint.Common.Entities.Items;
using PhoenixPoint.Geoscape.Entities;
using PhoenixPoint.Geoscape.View.ViewControllers.AugmentationScreen;
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
            Patch(harmonyInstance, typeof(UIModuleMutate), "InitCharacterInfo", "Override_InitCharacterInfo");
        }

        // ******************************************************************************************************************
        // ******************************************************************************************************************
        // Patched methods
        // ******************************************************************************************************************
        // ******************************************************************************************************************

        private const int MAX_AUGMENTATIONS = 3;

        public static bool Override_InitCharacterInfo(UIModuleMutate __instance,
            ref int ____currentCharacterAugmentsAmount,
			Dictionary<AddonSlotDef, UIModuleMutationSection> ____augmentSections,
			GameTagDef ____bionicsTag,
			GameTagDef ____mutationTag)
        {
			int i;
			bool flag;
			bool flag1;
			____currentCharacterAugmentsAmount = AugmentScreenUtilities.GetNumberOfAugments(__instance.CurrentCharacter);
			bool flag2 = ____currentCharacterAugmentsAmount < MAX_AUGMENTATIONS;
			foreach (KeyValuePair<AddonSlotDef, UIModuleMutationSection> _augmentSection in ____augmentSections)
			{
				AugumentSlotState augumentSlotState = AugumentSlotState.Available;
				string localizationKey = null;
				ItemDef augmentAtSlot = AugmentScreenUtilities.GetAugmentAtSlot(__instance.CurrentCharacter, _augmentSection.Key);
				if (augmentAtSlot != null)
				{
					flag = augmentAtSlot.Tags.Contains(____bionicsTag);
				}
				else
				{
					flag = false;
				}
				bool flag3 = flag;
				if (augmentAtSlot != null)
				{
					flag1 = augmentAtSlot.Tags.Contains(____mutationTag);
				}
				else
				{
					flag1 = false;
				}
				bool flag4 = flag1;
				if (flag3)
				{
					localizationKey = __instance.LockedDueToBionicsKey.LocalizationKey;
					augumentSlotState = AugumentSlotState.BlockedByPermenantAugument;
				}
				else if (!flag2 && !flag4)
				{
					localizationKey = __instance.LockedDueToLimitKey.LocalizationKey;
					augumentSlotState = AugumentSlotState.AugumentationLimitReached;
				}
				_augmentSection.Value.ResetContainer(augumentSlotState, localizationKey);
			}
			foreach (GeoItem armourItem in __instance.CurrentCharacter.ArmourItems)
			{
				if (!armourItem.ItemDef.Tags.Contains(____mutationTag))
				{
					continue;
				}
				AddonDef.RequiredSlotBind[] requiredSlotBinds = armourItem.ItemDef.RequiredSlotBinds;
				for (i = 0; i < (int)requiredSlotBinds.Length; i++)
				{
					AddonDef.RequiredSlotBind requiredSlotBind = requiredSlotBinds[i];
					if (____augmentSections.ContainsKey(requiredSlotBind.RequiredSlot))
					{
						____augmentSections[requiredSlotBind.RequiredSlot].SetMutationUsed(armourItem.ItemDef);
					}
				}
			}
			__instance.MutationsAvailableValue.text = __instance.XoutOfY.Localize(null)
				.Replace("{0}", ____currentCharacterAugmentsAmount.ToString())
				.Replace("{1}", MAX_AUGMENTATIONS.ToString()); ;
			__instance.MutationsAvailableValue.GetComponent<UIColorController>().SetWarningActive(MAX_AUGMENTATIONS <= ____currentCharacterAugmentsAmount, false);
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