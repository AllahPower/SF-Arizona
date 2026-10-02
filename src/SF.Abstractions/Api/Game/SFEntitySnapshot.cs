using System.Numerics;

namespace SFSharp.Abstractions.Game;

/// <summary>Copied state of a GTA world entity.</summary>
/// <param name="Entity">Entity the snapshot was taken from.</param>
/// <param name="ModelId">Model index.</param>
/// <param name="Interior">Interior (area) the entity is in.</param>
/// <param name="Position">World position.</param>
/// <param name="Heading">Heading in degrees around the Z axis, 0 facing north (+Y), counter-clockwise.</param>
/// <param name="Matrix">World matrix: rows are right, forward, up and translation.</param>
/// <param name="Velocity">Movement speed in game units per frame step (50 steps per second).</param>
/// <param name="TurnSpeed">Angular speed in game units per frame step.</param>
/// <param name="Health">Ped, vehicle or object health.</param>
/// <param name="Armour">Ped armour; 0 for vehicles and objects.</param>
/// <param name="Vehicle">For a ped, the vehicle it is sitting in.</param>
/// <param name="Driver">For a vehicle, its driver.</param>
public readonly record struct SFEntitySnapshot(
    SFEntityRef Entity,
    int ModelId,
    byte Interior,
    Vector3 Position,
    float Heading,
    Matrix4x4 Matrix,
    Vector3 Velocity,
    Vector3 TurnSpeed,
    float Health,
    float Armour,
    SFEntityRef? Vehicle,
    SFEntityRef? Driver);
