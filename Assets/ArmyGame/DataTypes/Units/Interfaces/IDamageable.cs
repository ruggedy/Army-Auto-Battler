using Logic.Attributes;

namespace Logic.Units.Interfaces
{
    public interface IDamageable
    {
        public Vitality vitality { get; }

        public void OnDamage(float damage);
    }
}