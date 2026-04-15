namespace SFSharp;

internal sealed class SFHost : ISF
{
    public ISFChat Chat => SF.Chat;
    public ISFDialog Dialog => SF.Dialog;
    public ISFKeyboard Keyboard => SF.Keyboard;
    public ISFPlayers Players => SF.Players;
    public ISFVehicles Vehicles => SF.Vehicles;
    public ISFGamePools Pools => SF.Pools;
    public ISFEvents Events => SF.Events;
    public ISFRpc Rpc => SF.Rpc;
    public ISFPackets Packets => SF.Packets;
    public ISFArizonaPackets Arizona => SF.Arizona;
    public ISFArizonaChat ArizonaChat => SF.ArizonaChat;
    public ISFPacketParsers PacketParsers => SF.PacketParsers;
    public ISFRpcParsers RpcParsers => SF.RpcParsers;
    public ISFCamera Camera => SF.Camera;
    public string UserFilesDirectory => SF.UserFilesDirectory;
}
