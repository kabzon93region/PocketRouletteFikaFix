using System;

using System.Linq;

using BepInEx.Logging;

using Comfort.Common;

using EFT;

using EFT.InventoryLogic;

using Fika.Core.Modding;

using Fika.Core.Modding.Events;

using Fika.Core.Networking;

using Fika.Core.Networking.LiteNetLib;

using Fika.Core.Networking.Packets;

using PocketRoulette;



namespace PocketRouletteFikaFix.Networking

{

    /// <summary>

    /// Headless host does not load PocketRoulette (batch mode), so PocketPacket handlers are never registered.

    /// Relay legacy PocketRoulette packets into authoritative host inventory adds.

    /// </summary>

    internal static class PocketRouletteHostBridge

    {

        private static bool _subscribed;

        private static bool _packetsRegistered;

        private static IFikaNetworkManager _manager;

        private static ManualLogSource _logger;



        internal static bool ArePacketsRegistered => _packetsRegistered;



        internal static void Initialize(ManualLogSource logger)

        {

            _logger = logger;



            if (_subscribed || !FikaSession.NeedsHostInventoryBridge())

            {

                EnsurePacketRegistration(logger);

                return;

            }



            _subscribed = true;

            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated);

            _logger?.LogInfo("[POCKET_FIKA] Host PocketRoulette bridge subscribed (headless/listen server)");

            EnsurePacketRegistration(logger);

        }



        internal static void BindManager(IFikaNetworkManager manager)

        {

            if (manager != null)

            {

                _manager = manager;

            }

        }



        internal static void EnsurePacketRegistration(ManualLogSource logger)

        {

            if (logger != null)

            {

                _logger = logger;

            }



            if (_packetsRegistered || !FikaSession.NeedsHostInventoryBridge())

            {

                return;

            }



            if (!Singleton<IFikaNetworkManager>.Instantiated)

            {

                return;

            }



            _manager = Singleton<IFikaNetworkManager>.Instance;

            TryRegisterPackets();

        }



        internal static void Shutdown()

        {

            if (!_subscribed)

            {

                return;

            }



            FikaEventDispatcher.UnsubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated);

            _subscribed = false;

            _packetsRegistered = false;

            _manager = null;

        }



        private static void OnNetworkManagerCreated(FikaNetworkManagerCreatedEvent createdEvent)

        {

            _manager = createdEvent.Manager;

            TryRegisterPackets();

        }



        private static void TryRegisterPackets()

        {

            if (_packetsRegistered || _manager == null || !FikaSession.NeedsHostInventoryBridge())

            {

                return;

            }



            try

            {

                _manager.RegisterPacket<PocketPacket, NetPeer>(OnServerPocketItem);

                _packetsRegistered = true;

                _logger?.LogInfo("[POCKET_FIKA] Headless PocketRoulette pocket packet handler registered");

            }

            catch (Exception ex)

            {

                _logger?.LogWarning($"[POCKET_FIKA] Pocket packet registration skipped: {ex.Message}");

            }

        }



        private static void OnServerPocketItem(PocketPacket packet, NetPeer peer)

        {

            if (!TryAddPocketItem(packet))

            {

                _logger?.LogWarning(

                    $"[POCKET_FIKA] Host pocket packet ignored for profile={packet.OwnerProfileId} netId={packet.OwnerNetId} item={packet.Item?.TemplateId}");

                return;

            }



            if (Singleton<IFikaNetworkManager>.Instance is FikaServer server)

            {

                server.SendData(ref packet, DeliveryMethod.ReliableOrdered, peer);

            }

        }



        private static bool TryAddPocketItem(PocketPacket packet)

        {

            try

            {

                if (packet.Item == null)

                {

                    _logger?.LogWarning("[POCKET_FIKA] Host pocket packet missing item");

                    return false;

                }



                var player = FindPlayer(packet.OwnerNetId, packet.OwnerProfileId);

                if (player?.InventoryController == null)

                {

                    _logger?.LogWarning(

                        $"[POCKET_FIKA] Host pocket packet player not found netId={packet.OwnerNetId} profile={packet.OwnerProfileId}");

                    return false;

                }



                if (player.InventoryController.Inventory.Equipment.GetAllItems()

                    .Any(existing => existing.Id == packet.Item.Id))

                {

                    return true;

                }



                var pocketsSlot = player.Profile.Inventory.Equipment.GetSlot(EquipmentSlot.Pockets);

                if (pocketsSlot?.ContainedItem is not CompoundItem pocketsItem)

                {

                    _logger?.LogWarning($"[POCKET_FIKA] Host pocket packet: no pockets for {packet.OwnerProfileId}");

                    return false;

                }



                foreach (var container in pocketsItem.Containers)

                {

                    if (container is not StashGridClass grid || grid.ID != packet.SlotId)

                    {

                        continue;

                    }



                    var location = new LocationInGrid(packet.X, packet.Y, (ItemRotation)packet.Rotation);

                    var address = grid.CreateItemAddress(location);



                    player.InventoryController.AddAndRaiseEvents(packet.Item, address);

                    _logger?.LogInfo(

                        $"[POCKET_FIKA] Host applied PocketRoulette item {packet.Item.TemplateId} ({packet.Item.Id}) for {packet.OwnerProfileId}");



                    return true;

                }



                _logger?.LogWarning(

                    $"[POCKET_FIKA] Host pocket packet: slot {packet.SlotId} not found for {packet.OwnerProfileId}");

            }

            catch (Exception ex)

            {

                _logger?.LogWarning($"[POCKET_FIKA] Host pocket packet failed: {ex.Message}");

            }



            return false;

        }



        private static Player FindPlayer(int netId, string profileId)

        {

            if (!Singleton<IFikaNetworkManager>.Instantiated)

            {

                return null;

            }



            var networkManager = Singleton<IFikaNetworkManager>.Instance;

            if (networkManager.CoopHandler?.Players != null

                && networkManager.CoopHandler.Players.TryGetValue(netId, out var player))

            {

                return player;

            }



            return Singleton<GameWorld>.Instantiated

                ? Singleton<GameWorld>.Instance.AllPlayersEverExisted?

                    .FirstOrDefault(existing => existing.ProfileId == profileId)

                : null;

        }

    }

}


