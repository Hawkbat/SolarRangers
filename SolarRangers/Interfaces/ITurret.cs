namespace SolarRangers.Interfaces
{
    public interface ITurret
    {
        void Init(ICombatant combatant, float fireRate, float fireDelay, float damage);
        void SetFiringState(bool firing);
    }
}