using Base.UI;
using Harmony;
using PhoenixPoint.Common.Entities.Addons;
using PhoenixPoint.Common.Entities.GameTags;
using PhoenixPoint.Common.Entities.Items;
using PhoenixPoint.Geoscape.Entities;
using PhoenixPoint.Geoscape.View.ViewControllers.AugmentationScreen;
using PhoenixPoint.Geoscape.View.ViewModules;
using System.Collections.Generic;

namespace pantolomin.phoenixPoint.fullMutation
{
    [HarmonyPatch(typeof(UIModuleMutate), "InitCharacterInfo")]
    class Mutate_InitCharacterInfo
	{
        [HarmonyPrefix]
        private static bool Prefix(UIModuleMutate __instance,
            ref int ____currentCharacterAugmentsAmount,
            Dictionary<AddonSlotDef, UIModuleMutationSection> ____augmentSections,
            GameTagDef ____bionicsTag,
            GameTagDef ____mutationTag)
        {
			int maxAugments = Mod.getMaxAugments();
			____currentCharacterAugmentsAmount = AugmentScreenUtilities.GetNumberOfAugments(__instance.CurrentCharacter);
			bool canStillAugment = ____currentCharacterAugmentsAmount < maxAugments;
			foreach (KeyValuePair<AddonSlotDef, UIModuleMutationSection> augmentSection in ____augmentSections)
			{
				AugumentSlotState slotState = AugumentSlotState.Available;
				string lockedReasonKey = null;
				ItemDef augmentAtSlot = AugmentScreenUtilities.GetAugmentAtSlot(__instance.CurrentCharacter, augmentSection.Key);
				bool isMutated = augmentAtSlot?.Tags.Contains(____mutationTag) ?? false;
				bool isBionic = augmentAtSlot?.Tags.Contains(____bionicsTag) ?? false;
				if (isBionic)
				{
					if (!Mod.Config.allowBionicToMutated)
					{
						lockedReasonKey = __instance.LockedDueToBionicsKey.LocalizationKey;
						slotState = AugumentSlotState.BlockedByPermenantAugument;
					}
				}
				else if (!canStillAugment && !isMutated)
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
			text = text.Replace("{1}", maxAugments.ToString());
			__instance.MutationsAvailableValue.text = text;
			__instance.MutationsAvailableValue.GetComponent<UIColorController>().SetWarningActive(!canStillAugment);
			return false;
		}
	}
}
