namespace SFSharp.Protocol.Rpc;

/// <summary>SA-MP dialog style byte, as sent in the ShowDialog RPC and passed to CDialog::Show.</summary>
public enum DialogStyle
{
    MsgBox = 0,
    Input = 1,
    List = 2,
    Password = 3,
    TabList = 4,
    TabListHeaders = 5,
}
