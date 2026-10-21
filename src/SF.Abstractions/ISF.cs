using SFSharp.Abstractions.Gta;
using SFSharp.Abstractions.Samp;

namespace SFSharp.Abstractions;

/// <summary>
/// Root plugin-facing facade. The concrete implementation is owned by the host and exposed to
/// every module through <see cref="IModuleContext.SF"/> once SA-MP's CNetGame exists.
/// </summary>
public interface ISF
{
    ISFGta Gta { get; }
    ISFSamp Samp { get; }
    ISFMainThread MainThread { get; }

    /// <summary>Keyboard state helpers, see <see cref="ISFKeyboard"/>.</summary>
    ISFKeyboard Keyboard { get; }

    /// <summary>Read-only catalog of modules currently registered in the host.</summary>
    ISFModules Modules { get; }

    /// <summary>Game start-up stages, see <see cref="ISFGameLoading"/>.</summary>
    ISFGameLoading Loading { get; }

    /// <summary>Resolved GTA San Andreas user files directory.</summary>
    string UserFilesDirectory { get; }
}
