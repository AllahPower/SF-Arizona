using System.Drawing;

namespace SFSharp;

public static class HookManager
{
    public static Hook<CChatAddEntryArgs> CChatAddEntry { get; } = new CChatAddEntryHook();
    public static Hook<PeekMessageArgs, PeekMessageResult> PeekMessage { get; } = new PeekMessageHook();
}