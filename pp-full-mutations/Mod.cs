using Harmony;
using System;
using System.Reflection;

namespace pantolomin.phoenixPoint.fullMutation
{
    public class Mod
    {
		internal static ModConfig Config;

		public static void Init()
		{
			new Mod().MainMod();
		}

		public void MainMod(Func<string, object, object> api = null)
		{
			Config = api("config", null) as ModConfig ?? new ModConfig();
			HarmonyInstance.Create("phoenixpoint.FullAugments").PatchAll(Assembly.GetExecutingAssembly());
			api("log verbose", "Mod Initialised.");
		}

		internal static int getMaxAugments()
		{
			return Math.Max(Math.Min(Config.maxAugments, 3), 1);
		}
    }
}