using BepInEx.Bootstrap;

using Fika.Core.Main.Utils;

using HarmonyLib;



namespace PocketRouletteFikaFix

{

    internal static class FikaSession

    {

        private const string FikaBackendUtilsType = "Fika.Core.Main.Utils.FikaBackendUtils";

        private const string FikaHeadlessGuid = "com.fika.headless";



        public static bool IsFikaLoaded()

        {

            return AccessTools.TypeByName(FikaBackendUtilsType) != null;

        }



        public static bool IsDedicatedCoopClient()

        {

            if (!IsFikaLoaded())

            {

                return false;

            }



            try

            {

                return FikaBackendUtils.IsClient && !FikaBackendUtils.IsServer;

            }

            catch

            {

                return false;

            }

        }



        public static bool IsHeadlessHostInstance()

        {

            return Chainloader.PluginInfos.ContainsKey(FikaHeadlessGuid);

        }



        public static bool NeedsHostInventoryBridge()

        {

            if (!IsFikaLoaded() || IsDedicatedCoopClient())

            {

                return false;

            }



            if (IsHeadlessHostInstance())

            {

                return true;

            }



            try

            {

                if (FikaBackendUtils.IsHeadless || FikaBackendUtils.IsHeadlessGame)

                {

                    return true;

                }

            }

            catch

            {

                // ignored

            }



            try

            {

                return FikaBackendUtils.IsServer;

            }

            catch

            {

                return false;

            }

        }

    }

}


