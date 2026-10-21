namespace SFSharp.Runtime.Gta;

internal sealed class SFGtaServices(ISFEntities entities, ISFCamera camera, ISFGameClock clock, ISFWorld world) : ISFGta
{
    public ISFEntities Entities => entities;
    public ISFCamera Camera => camera;
    public ISFGameClock Clock => clock;
    public ISFWorld World => world;
}
