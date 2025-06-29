namespace SFSharp;

public class SFKeyboard : ISFComponent
{
    private static byte[] _currentState = new byte[256];
    private static byte[] _lastState = new byte[256];

    async void ISFComponent.Initialize()
    {
        while (true)
        {
            (_currentState, _lastState) = (_lastState, _currentState);
            _currentState.AsSpan().Fill(0);
            Win32.GetKeyboardState(ref _currentState[0]);
            await Task.Yield();
        }
    }

    private static bool IsKeyDownCore(VK key, byte[] state)
    {
        return (state[(int)key] & 0x80) != 0;
    }

    public bool IsKeyDown(VK key)
    {
        return IsKeyDownCore(key, _currentState);
    }

    public bool IsKeyPressed(VK key)
    {
        return IsKeyDownCore(key, _currentState) && !IsKeyDownCore(key, _lastState);
    }
}