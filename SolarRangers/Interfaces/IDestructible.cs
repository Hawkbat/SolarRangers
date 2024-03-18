namespace SolarRangers.Interfaces
{
    public interface IDestructible
    {
        string GetNameKey();
        float GetHealth();
        float GetMaxHealth();
        bool IsDestroyed();
        bool TakeDamage(IDamageSource source, float damage);
    }
}