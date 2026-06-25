using System.Collections;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PocketRouletteFikaFix.Networking;
using PocketRouletteFikaFix.Patches;
using UnityEngine;

namespace PocketRouletteFikaFix
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.fika.headless", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.kairo.pocketroulette", BepInDependency.DependencyFlags.SoftDependency)]
    public class PluginCore : BaseUnityPlugin
    {
        internal static ManualLogSource FixLogger { get; private set; }

        private Harmony _harmony;
        private Coroutine _hostBridgeCoroutine;
        private bool _hostNetInitPatchApplied;

        private void Awake()
        {
            FixLogger = Logger;
            _harmony = new Harmony(PluginInfo.GUID);

            if (!FikaSession.IsFikaLoaded())
            {
                Logger.LogWarning("[POCKET_FIKA] Fika.Core not detected — mod inactive");
                return;
            }

            Logger.LogInfo($"[POCKET_FIKA] {PluginInfo.NAME} v{PluginInfo.VERSION} loading");
            TryApplyHostPatches();

            if (FikaSession.NeedsHostInventoryBridge() || FikaSession.IsHeadlessHostInstance())
            {
                _hostBridgeCoroutine = StartCoroutine(HostBridgeLifetimeLoop());
            }
            else if (FikaSession.IsDedicatedCoopClient())
            {
                Logger.LogInfo("[POCKET_FIKA] Dedicated client — native PocketRoulette SendPocketItem flow (no client patches)");
            }
        }

        private void Start()
        {
            TryApplyHostPatches();

            if (FikaSession.NeedsHostInventoryBridge())
            {
                PocketRouletteHostBridge.Initialize(FixLogger);
                PocketRouletteHostBridge.EnsurePacketRegistration(FixLogger);
            }
        }

        private IEnumerator HostBridgeLifetimeLoop()
        {
            var wait = new WaitForSeconds(0.5f);
            var logged = false;

            while (true)
            {
                if (FikaSession.IsHeadlessHostInstance() || FikaSession.NeedsHostInventoryBridge())
                {
                    if (!logged)
                    {
                        logged = true;
                        FixLogger.LogInfo("[POCKET_FIKA] Headless host bridge lifetime loop active");
                    }

                    TryApplyHostPatches();
                    PocketRouletteHostBridge.Initialize(FixLogger);
                    PocketRouletteHostBridge.EnsurePacketRegistration(FixLogger);
                }

                yield return wait;
            }
        }

        private void TryApplyHostPatches()
        {
            if (_harmony == null || _hostNetInitPatchApplied)
            {
                return;
            }

            _hostNetInitPatchApplied = FikaServerRegisterPacketsPatch.TryApply(_harmony, FixLogger);
        }

        private void OnDestroy()
        {
            if (_hostBridgeCoroutine != null)
            {
                StopCoroutine(_hostBridgeCoroutine);
            }

            PocketRouletteHostBridge.Shutdown();
            _harmony?.UnpatchSelf();
        }
    }
}
