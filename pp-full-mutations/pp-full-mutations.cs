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
		public Mod() {
		}

        public ModLoadPriority Priority => ModLoadPriority.Low;

        public void Initialize()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create(typeof(Mod).Namespace);
            Patch(harmonyInstance, typeof(UIModuleMutate), "InitCharacterInfo", "Override_InitCharacterInfo_Mutate");
            Patch(harmonyInstance, typeof(UIModuleBionics), "InitCharacterInfo", "Override_InitCharacterInfo_Augment");
        }

        // ******************************************************************************************************************
        // ******************************************************************************************************************
        // Patched methods
        // ******************************************************************************************************************
        // ******************************************************************************************************************

        private const int MAX_AUGMENTATIONS = 3;

		public static bool Override_InitCharacterInfo_Mutate(UIModuleMutate __instance,
            ref int ____currentCharacterAugmentsAmount,
			Dictionary<AddonSlotDef, UIModuleMutationSection> ____augmentSections,
			GameTagDef ____bionicsTag,
			GameTagDef ____mutationTag)
        {
			____currentCharacterAugmentsAmount = 0;
			____currentCharacterAugmentsAmount = AugmentScreenUtilities.GetNumberOfAugments(__instance.CurrentCharacter);
			bool flag = ____currentCharacterAugmentsAmount < MAX_AUGMENTATIONS;
			foreach (KeyValuePair<AddonSlotDef, UIModuleMutationSection> augmentSection in ____augmentSections)
			{
				AugumentSlotState slotState = AugumentSlotState.Available;
				string lockedReasonKey = null;
				ItemDef augmentAtSlot = AugmentScreenUtilities.GetAugmentAtSlot(__instance.CurrentCharacter, augmentSection.Key);
				bool flag2 = augmentAtSlot?.Tags.Contains(____bionicsTag) ?? false;
				bool flag3 = augmentAtSlot?.Tags.Contains(____mutationTag) ?? false;
				if (flag2)
				{
					lockedReasonKey = __instance.LockedDueToBionicsKey.LocalizationKey;
					slotState = AugumentSlotState.BlockedByPermenantAugument;
				}
				else if (!flag && !flag3)
				{
					lockedReasonKey = __instance.LockedDueToLimitKey.LocalizationKey;
					slotState = AugumentSlotState.AugumentationLimitReached;
				}
				augmentSection.Value.ResetContainer(slotState, lockedReasonKey);
			}
			foreach (GeoItem armourItem in __instance.CurrentCharacter.ArmourItems)
			{
				if (!armourItem.ItemDef.Tags.Contains(____mutationTag))
				{
					continue;
				}
				AddonDef.RequiredSlotBind[] requiredSlotBinds = armourItem.ItemDef.RequiredSlotBinds;
				for (int i = 0; i < requiredSlotBinds.Length; i++)
				{
					AddonDef.RequiredSlotBind requiredSlotBind = requiredSlotBinds[i];
					if (____augmentSections.ContainsKey(requiredSlotBind.RequiredSlot))
					{
						____augmentSections[requiredSlotBind.RequiredSlot].SetMutationUsed(armourItem.ItemDef);
					}
				}
			}
			string text = __instance.XoutOfY.Localize();
			text = text.Replace("{0}", ____currentCharacterAugmentsAmount.ToString());
			text = text.Replace("{1}", MAX_AUGMENTATIONS.ToString());
			__instance.MutationsAvailableValue.text = text;
			__instance.MutationsAvailableValue.GetComponent<UIColorController>().SetWarningActive(MAX_AUGMENTATIONS <= ____currentCharacterAugmentsAmount);
			return false;
		}

		public static bool Override_InitCharacterInfo_Augment(UIModuleBionics __instance,
            ref int ____currentCharacterAugmentsAmount,
			Dictionary<AddonSlotDef, UIModuleMutationSection> ____augmentSections,
			GameTagDef ____bionicsTag,
			GameTagDef ____mutationTag)
		{
			____currentCharacterAugmentsAmount = 0;
			____currentCharacterAugmentsAmount = AugmentScreenUtilities.GetNumberOfAugments(__instance.CurrentCharacter);
			bool flag = ____currentCharacterAugmentsAmount < MAX_AUGMENTATIONS;
			foreach (KeyValuePair<AddonSlotDef, UIModuleMutationSection> augmentSection in ____augmentSections)
			{
				AugumentSlotState slotState = AugumentSlotState.Available;
				string lockedReasonKey = null;
				ItemDef augmentAtSlot = AugmentScreenUtilities.GetAugmentAtSlot(__instance.CurrentCharacter, augmentSection.Key);
				bool flag2 = augmentAtSlot?.Tags.Contains(____bionicsTag) ?? false;
				if (augmentAtSlot?.Tags.Contains(____mutationTag) ?? false)
				{
					lockedReasonKey = __instance.LockedDueToMutationKey.LocalizationKey;
					slotState = AugumentSlotState.BlockedByPermenantAugument;
				}
				else if (!flag && !flag2)
				{
					lockedReasonKey = __instance.LockedDueToLimitKey.LocalizationKey;
					slotState = AugumentSlotState.AugumentationLimitReached;
				}
				augmentSection.Value.ResetContainer(slotState, lockedReasonKey);
			}
			foreach (GeoItem armourItem in __instance.CurrentCharacter.ArmourItems)
			{
				if (!armourItem.ItemDef.Tags.Contains(____bionicsTag))
				{
					continue;
				}
				AddonDef.RequiredSlotBind[] requiredSlotBinds = armourItem.ItemDef.RequiredSlotBinds;
				for (int i = 0; i < requiredSlotBinds.Length; i++)
				{
					AddonDef.RequiredSlotBind requiredSlotBind = requiredSlotBinds[i];
					if (____augmentSections.ContainsKey(requiredSlotBind.RequiredSlot))
					{
						____augmentSections[requiredSlotBind.RequiredSlot].SetMutationUsed(armourItem.ItemDef);
					}
				}
			}
			string text = __instance.XoutOfY.Localize();
			text = text.Replace("{0}", ____currentCharacterAugmentsAmount.ToString());
			text = text.Replace("{1}", MAX_AUGMENTATIONS.ToString());
			__instance.AugmentsAvailableValue.text = text;
			__instance.AugmentsAvailableValue.GetComponent<UIColorController>().SetWarningActive(flag);
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