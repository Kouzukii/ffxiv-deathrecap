using System;
using System.Numerics;
using System.Text;
using ImGuiNET;

namespace DeathRecap.UI;

internal static class ImGuiHelper {
    public static void TextColored(uint col, string text) {
        ImGui.PushStyleColor(ImGuiCol.Text, col);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
    }

    public static unsafe void AddText(this ImDrawListPtr drawListPtr, Vector2 pos, uint col, string text, float wrapWidth) {
        var nativePtr = ImGui.GetFont().NativePtr;
        var byteCount = Encoding.UTF8.GetByteCount(text);
        var textPtr = stackalloc byte[byteCount + 1];
        byte* textEnd;
        fixed (char* chPtr = &text.GetPinnableReference()) {
            var chars = (IntPtr)chPtr;
            var bytes = Encoding.UTF8.GetBytes((char*)chars, text.Length, textPtr, byteCount);
            textPtr[bytes] = 0;
            textEnd = &textPtr[bytes];
        }

        ImGuiNative.ImDrawList_AddText_FontPtr(drawListPtr.NativePtr, nativePtr, ImGui.GetFontSize(), pos, col, textPtr, textEnd, wrapWidth, null);
    }

    public static unsafe void AddTextOutlined(this ImDrawListPtr drawListPtr, Vector2 pos, uint col, uint outlineCol, string text, float wrapWidth = 0.0f) {
        var nativePtr = ImGui.GetFont().NativePtr;
        var byteCount = Encoding.UTF8.GetByteCount(text);
        var textPtr = stackalloc byte[byteCount + 1];
        byte* textEnd;
        fixed (char* chPtr = &text.GetPinnableReference()) {
            var chars = (IntPtr)chPtr;
            var bytes = Encoding.UTF8.GetBytes((char*)chars, text.Length, textPtr, byteCount);
            textPtr[bytes] = 0;
            textEnd = &textPtr[bytes];
        }

        ImGuiNative.ImDrawList_AddText_FontPtr(drawListPtr.NativePtr, nativePtr, ImGui.GetFontSize(), pos - Vector2.One, outlineCol, textPtr, textEnd,
            wrapWidth, null);
        ImGuiNative.ImDrawList_AddText_FontPtr(drawListPtr.NativePtr, nativePtr, ImGui.GetFontSize(), pos - new Vector2(1, -1), outlineCol, textPtr, textEnd,
            wrapWidth, null);
        ImGuiNative.ImDrawList_AddText_FontPtr(drawListPtr.NativePtr, nativePtr, ImGui.GetFontSize(), pos + new Vector2(1, -1), outlineCol, textPtr, textEnd,
            wrapWidth, null);
        ImGuiNative.ImDrawList_AddText_FontPtr(drawListPtr.NativePtr, nativePtr, ImGui.GetFontSize(), pos + Vector2.One, outlineCol, textPtr, textEnd,
            wrapWidth, null);
        ImGuiNative.ImDrawList_AddText_FontPtr(drawListPtr.NativePtr, nativePtr, ImGui.GetFontSize(), pos, col, textPtr, textEnd, wrapWidth, null);
    }
}
