namespace SFSharp.Abstractions.Samp;

/// <summary>Arguments supplied to the local SA-MP vehicle-entry notification routine.</summary>
/// <param name="VehicleId">SA-MP vehicle ID, not a GTA pool handle.</param>
/// <param name="IsPassenger">Whether entry was requested as a passenger, without an inferred seat index.</param>
public readonly record struct SFSampVehicleEntryEvent(int VehicleId, bool IsPassenger);
