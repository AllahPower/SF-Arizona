using System.Runtime.InteropServices;

namespace SFSharp;

internal static unsafe class NativeString
{
    public static string? Decode(byte* pointer, int maxLength)
    {
        if (pointer == null || maxLength <= 0)
        {
            return null;
        }

        int length = 0;
        while (length < maxLength && pointer[length] != 0)
        {
            length++;
        }

        return length == 0 ? string.Empty : Marshal.PtrToStringAnsi((nint)pointer, length);
    }
}
