namespace SFSharp;

internal sealed class SFHost : ISF
{
    public ISFChat Chat => SF.Chat;
    public ISFDialog Dialog => SF.Dialog;
    public ISFKeyboard Keyboard => SF.Keyboard;
    public ISFPlayers Players => SF.Players;
    public ISFEvents Events => SF.Events;
}
