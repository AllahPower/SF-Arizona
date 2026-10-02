namespace SFSharp.Abstractions.Game;

/// <summary>
/// Damage the game is about to apply to a ped, vehicle or object. Raised synchronously on the main thread
/// before the engine applies it; handlers may change <see cref="Amount"/> or set <see cref="Cancel"/>.
/// </summary>
/// <remarks>
/// Ped damage is the engine's damage response for every ped, including the local player. Vehicle damage
/// covers weapon hits only; collision damage does not pass through this event.
/// </remarks>
public sealed class SFDamageEvent
{
    public SFDamageEvent(SFEntityRef target, SFEntityRef? source, int weapon, int bodyPart, float amount)
    {
        Target = target;
        Source = source;
        Weapon = weapon;
        BodyPart = bodyPart;
        Amount = amount;
    }

    public SFEntityRef Target { get; }

    /// <summary>Entity that caused the damage, when the game knows it.</summary>
    public SFEntityRef? Source { get; }

    /// <summary>GTA weapon type (<c>eWeaponType</c>).</summary>
    public int Weapon { get; }

    /// <summary>GTA body part (<c>ePedPieceTypes</c>) for ped damage; 0 for vehicles and objects.</summary>
    public int BodyPart { get; }

    public float Amount { get; set; }

    public bool Cancel { get; set; }
}
