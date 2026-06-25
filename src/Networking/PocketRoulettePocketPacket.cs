using System.Text;

using EFT.InventoryLogic;

using Fika.Core.Networking;

using Fika.Core.Networking.LiteNetLib.Utils;



namespace PocketRoulette

{

    /// <summary>

    /// Must match PocketRoulette client packet type name/hash for Fika packet id 30014 on headless.

    /// </summary>

    internal struct PocketPacket : INetSerializable

    {

        public int OwnerNetId;

        public string OwnerProfileId;

        public Item Item;

        public string SlotId;

        public int X;

        public int Y;

        public int Rotation;



        public void Serialize(NetDataWriter writer)

        {

            writer.Put(OwnerNetId);

            writer.PutBytesWithLength(Encoding.UTF8.GetBytes(OwnerProfileId ?? string.Empty));

            writer.PutItem(Item);

            writer.PutBytesWithLength(Encoding.UTF8.GetBytes(SlotId ?? string.Empty));

            writer.Put(X);

            writer.Put(Y);

            writer.Put(Rotation);

        }



        public void Deserialize(NetDataReader reader)

        {

            OwnerNetId = reader.GetInt();

            OwnerProfileId = Encoding.UTF8.GetString(reader.GetBytesWithLength());

            Item = reader.GetItem();

            SlotId = Encoding.UTF8.GetString(reader.GetBytesWithLength());

            X = reader.GetInt();

            Y = reader.GetInt();

            Rotation = reader.GetInt();

        }

    }

}


