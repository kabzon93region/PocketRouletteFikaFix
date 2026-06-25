using System.Reflection;

using BepInEx.Logging;

using Fika.Core.Networking;

using HarmonyLib;

using PocketRouletteFikaFix.Networking;



namespace PocketRouletteFikaFix.Patches

{

    internal static class FikaServerRegisterPacketsPatch

    {

        private static bool _applied;



        internal static bool TryApply(Harmony harmony, ManualLogSource logger)

        {

            if (_applied)

            {

                return true;

            }



            var serverType = ModAssemblyTypes.FindType("Fika.Core.Networking.FikaServer");

            var registerMethod = serverType?.GetMethod(

                "RegisterPacketsAndTypes",

                BindingFlags.Instance | BindingFlags.NonPublic);



            if (registerMethod == null)

            {

                logger.LogWarning("[POCKET_FIKA] FikaServer.RegisterPacketsAndTypes not found");

                return false;

            }



            harmony.Patch(

                registerMethod,

                postfix: new HarmonyMethod(typeof(FikaServerRegisterPacketsPatch), nameof(RegisterPacketsAndTypesPostfix)));



            _applied = true;

            logger.LogInfo("[POCKET_FIKA] Patched FikaServer.RegisterPacketsAndTypes for host pocket bridge");

            return true;

        }



        private static void RegisterPacketsAndTypesPostfix(IFikaNetworkManager __instance)

        {

            if (!FikaSession.NeedsHostInventoryBridge())

            {

                return;

            }



            PocketRouletteHostBridge.BindManager(__instance);

            PocketRouletteHostBridge.EnsurePacketRegistration(PluginCore.FixLogger);

        }

    }

}


