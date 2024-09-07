using Core.Mediators;

public class EnemyKilledMessage : IMessage
{
    public Enemy Enemy { get; }
    
    public EnemyKilledMessage(Enemy enemy)
    {
        Enemy = enemy;
    }
}