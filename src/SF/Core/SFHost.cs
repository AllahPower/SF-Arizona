namespace SFSharp;

internal sealed class SFHost : ISF
{
    public ISFChat Chat => SF.Chat;
}
