namespace SFSharp.Runtime.Ui;

public class SFKeyboard : ISFKeyboard
{
    private static byte[] _currentState = new byte[256];
    private static byte[] _lastState = new byte[256];

    internal async void StartLoop()
    {
        try
        {
            while (true)
            {
                (_currentState, _lastState) = (_lastState, _currentState);
                _currentState.AsSpan().Clear();
                Win32.GetKeyboardState(ref _currentState[0]);
                await Task.Yield();
            }
        }
        catch (Exception ex)
        {
            SFBootstrap.ProcessException(ex);
        }
    }

    private static bool IsKeyDownCore(VirtualKey key, byte[] state)
    {
        return (state[(int)key] & 0x80) != 0;
    }

    public bool IsKeyDown(VirtualKey key)
    {
        return IsKeyDownCore(key, _currentState);
    }

    public bool IsKeyDown(byte virtualKeyCode)
    {
        return IsKeyDown((VirtualKey)virtualKeyCode);
    }

    public bool IsKeyPressed(VirtualKey key)
    {
        return IsKeyDownCore(key, _currentState) && !IsKeyDownCore(key, _lastState);
    }

    public bool IsKeyPressed(byte virtualKeyCode)
    {
        return IsKeyPressed((VirtualKey)virtualKeyCode);
    }
}
