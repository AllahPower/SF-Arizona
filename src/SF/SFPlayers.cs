
namespace SFSharp;

public partial class SFPlayers
{
    public ushort LocalPlayerId => CPlayerPool.Instance.LocalPlayerInfo.Id;
    public string? GetName(ushort playerId) => CPlayerPool.Instance.GetName(playerId);
    public int? GetScore(ushort playerId) => CPlayerPool.Instance.GetScore(playerId);

    public ushort? GetAimedPlayerId()
    {
        var aimedPlayer = CLocalPlayer.Instance.WeaponsData.AimedPlayer;
        return aimedPlayer != ushort.MaxValue ? aimedPlayer : null;
    }
}