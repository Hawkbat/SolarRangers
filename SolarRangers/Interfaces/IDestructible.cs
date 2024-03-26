namespace SolarRangers.Interfaces
{
    public interface IDestructible
    {
        string GetNameKey();
        float GetHealth();
        float GetMaxHealth();
        bool IsDestroyed();
        bool OnTakeDamage(IDamageSource source);
    }
}