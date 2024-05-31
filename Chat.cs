using System;
using System.Runtime.InteropServices;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace DeathRecap;

public class Chat
{
    private static class Signatures
    {
        internal const string SendChat = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9";
        internal const string SanitiseString = "E8 ?? ?? ?? ?? EB 0A 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8D 8D";
    }
    private delegate void ProcessChatBoxDelegate(IntPtr uiModule, IntPtr message, IntPtr unused, byte a4);
    private ProcessChatBoxDelegate ProcessChatBox { get; }
    private readonly unsafe delegate* unmanaged<Utf8String*, int, IntPtr, void> _sanitiseString = null!;

    internal static Chat instance;
    public static Chat Instance
    {
        get
        {
            instance ??= new();
            return instance;
        }
    }

    public Chat()
    {
        if (Service.SigScanner.TryScanText(Signatures.SendChat, out var processChatBoxPtr))
        {
            ProcessChatBox = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(processChatBoxPtr);
        }
        unsafe
        {
            if (Service.SigScanner.TryScanText(Signatures.SanitiseString, out var sanitisePtr))
            {
                _sanitiseString = (delegate* unmanaged<Utf8String*, int, IntPtr, void>)sanitisePtr;
            }
        }
    }
    
    public unsafe void SendMessageUnsafe(byte[] message)
    {
        if (ProcessChatBox == null)
        {
            throw new InvalidOperationException("Could not find signature for chat sending");
        }
        var uiModule = (IntPtr)Framework.Instance()->GetUiModule();
        using var payload = new ChatPayload(message);
        var mem1 = Marshal.AllocHGlobal(400);
        Marshal.StructureToPtr(payload, mem1, false);
        ProcessChatBox(uiModule, mem1, IntPtr.Zero, 0);
        Marshal.FreeHGlobal(mem1);
    }
    
    public void SendMessage(string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        if (bytes.Length == 0)
        {
            throw new ArgumentException("message is empty", nameof(message));
        }
        if (bytes.Length > 500)
        {
            throw new ArgumentException("message is longer than 500 bytes", nameof(message));
        }
        if (message.Length != SanitiseText(message).Length)
        {
            throw new ArgumentException("message contained invalid characters", nameof(message));
        }
        SendMessageUnsafe(bytes);
    }
    
    public unsafe string SanitiseText(string text)
    {
        if (_sanitiseString == null)
        {
            throw new InvalidOperationException("Could not find signature for chat sanitisation");
        }
        var uText = Utf8String.FromString(text);
        _sanitiseString(uText, 0x27F, IntPtr.Zero);
        var sanitised = uText->ToString();
        uText->Dtor();
        IMemorySpace.Free(uText);
        return sanitised;
    }
    [StructLayout(LayoutKind.Explicit)]
    private readonly struct ChatPayload : IDisposable
    {
        [FieldOffset(0)]
        private readonly IntPtr textPtr;
        [FieldOffset(16)]
        private readonly ulong textLen;
        [FieldOffset(8)]
        private readonly ulong unk1;
        [FieldOffset(24)]
        private readonly ulong unk2;
        internal ChatPayload(byte[] stringBytes)
        {
            textPtr = Marshal.AllocHGlobal(stringBytes.Length + 30);
            Marshal.Copy(stringBytes, 0, textPtr, stringBytes.Length);
            Marshal.WriteByte(textPtr + stringBytes.Length, 0);
            textLen = (ulong)(stringBytes.Length + 1);
            unk1 = 64;
            unk2 = 0;
        }
        public void Dispose()
        {
            Marshal.FreeHGlobal(textPtr);
        }
    }
}