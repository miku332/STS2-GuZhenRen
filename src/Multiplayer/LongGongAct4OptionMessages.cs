using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace GuZhenRen.Multiplayer;

internal struct LongGongAct4OptionRequestMessage : INetMessage, IPacketSerializable
{
    public bool ShouldBroadcast => false;

    public NetTransferMode Mode => NetTransferMode.Reliable;

    public LogLevel LogLevel => LogLevel.VeryDebug;

    public bool ShouldBuffer => true;

    public void Serialize(PacketWriter writer)
    {
    }

    public void Deserialize(PacketReader reader)
    {
    }
}

internal struct LongGongAct4OptionStateMessage : INetMessage, IPacketSerializable
{
    public bool Enabled;

    public bool ShouldBroadcast => false;

    public NetTransferMode Mode => NetTransferMode.Reliable;

    public LogLevel LogLevel => LogLevel.VeryDebug;

    public bool ShouldBuffer => true;

    public void Serialize(PacketWriter writer) => writer.WriteBool(Enabled);

    public void Deserialize(PacketReader reader) => Enabled = reader.ReadBool();
}
