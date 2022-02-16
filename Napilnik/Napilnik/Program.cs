class Weapon
{

	private readonly int _damage;
    private int _bullets;

	public event Action BulletsEnded = default!;

	public Weapon(int damage)
	{
		if (damage < 0)
			throw new ArgumentOutOfRangeException(nameof(damage));

		_damage = damage;
	}

    public bool HasBullets => _bullets > 0;

    public void Fire(IDamageable player)
    {
        if (_bullets <= 0)
            throw new InvalidOperationException();

		_bullets -= 1;
        player.TakeDamage(_damage);

        if (_bullets == 0)
            BulletsEnded?.Invoke();
    }
}

interface IDamageable
{
	void TakeDamage(int damage);
}

public class Health : IDamageable
{
	public int Value { get; private set; }
	public event Action? Die;

	public Health(int value)
	{
		if (value < 0)
			throw new ArgumentOutOfRangeException(nameof(value));

		Value = value;
	}

	public void TakeDamage(int damage)
	{
		if (damage < 0)
			throw new ArgumentOutOfRangeException(nameof(damage));

		Value -= damage;
		if (Value < 0) 
		{
			Value = 0;
			Die?.Invoke();
		}
	}
}

class Player : IDamageable
{
	private readonly Health _health;

	public Player(Health health)
	{
		_health = health;
	}

	public bool Dead => _health.Value == 0;

	public void TakeDamage(int damage)
	{
		_health.TakeDamage(damage);
	}
}

class Bot
{
	private readonly Weapon _weapon;

	public Bot(Weapon weapon, int bulletsStock)
	{
		_weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));

		if (bulletsStock < 0)
			throw new ArgumentOutOfRangeException(nameof(bulletsStock));
	}

	public void OnSeePlayer(Player player)
	{
		if (player.Dead)
			return;

		if (_weapon.HasBullets)
		    _weapon.Fire(player);
    }
}

static class Program
{
	public static void Main()
	{
		var bot = new Bot(new Weapon(10), 100);
		var player = new Player(new Health(100));
		bot.OnSeePlayer(player);
	}
}