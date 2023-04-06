using System.Collections.Generic;
using System.IO;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace DeathRecap.Game;

public class DeathNotificationPayload : Payload {
    private const byte CustomPayloadId = 191;
    public long DeathTimestamp { get; private set; }
    public uint PlayerId { get; private set; }

    public override PayloadType Type => PayloadType.Unknown;

    private DeathNotificationPayload() {
    }

    public DeathNotificationPayload(long deathTimestamp, uint playerId) {
        DeathTimestamp = deathTimestamp;
        PlayerId = playerId;
    }

    public static DeathNotificationPayload? Decode(RawPayload rawPayload) {
        var data = rawPayload.Data;
        if (data[1] != CustomPayloadId)
            return null;
        using var reader = new BinaryReader(new MemoryStream(data));
        reader.BaseStream.Position = 3;
        var payload = new DeathNotificationPayload();
        payload.DecodeImpl(reader, reader.BaseStream.Length - 1);
        return payload;
    }

    protected override byte[] EncodeImpl() {
        var p1 = MakeInteger((uint)(DeathTimestamp >> 32));
        var p2 = MakeInteger((uint)DeathTimestamp);
        var p3 = MakeInteger(PlayerId);
        var result = new List<byte> { 2, CustomPayloadId, (byte)(p1.Length + p2.Length + p3.Length + 1) };
        result.AddRange(p1);
        result.AddRange(p2);
        result.AddRange(p3);
        result.Add(3);
        return result.ToArray();
    }

    protected override void DecodeImpl(BinaryReader reader, long endOfStream) {
        var p1 = GetInteger(reader);
        var p2 = GetInteger(reader);
        var p3 = GetInteger(reader);
        DeathTimestamp = ((long)p1 << 32) | p2;
        PlayerId = p3;
    }
}
