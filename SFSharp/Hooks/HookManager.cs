using System.Drawing;

namespace SFSharp;

public static class HookManager
{
    public static Hook<CChatAddEntryArgs> CChatAddEntry { get; } = new CChatAddEntryHook();
}