using Core.Mediators;

public class PlayerTookDamageMessage : IMessage
{
    public PlayerScript PlayerHandler { get; }

    public PlayerTookDamageMessage(PlayerScript playerScript)
    {
        PlayerHandler = playerScript;
    }
}
