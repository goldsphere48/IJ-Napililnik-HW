class Weapon
{

	private readonly int _damage;
	private readonly int _clipCapacity;
	private int _currentBullets;

	public event Action OutOfBullets = default!;

	public Weapon(int damage, int clipCapacity)
	{
		if (damage < 0)
			throw new ArgumentOutOfRangeException(nameof(damage));

		if (clipCapacity <= 0)
			throw new ArgumentOutOfRangeException(nameof(clipCapacity));

		_clipCapacity = clipCapacity;
		_damage = damage;
	}

	public bool TryFire(IDamageable player)
	{
		if (_currentBullets > 0) 
		{
			_currentBullets -= 1;
			player.TakeDamage(_damage);
			return true;
		}

		OutOfBullets?.Invoke();
		return false;
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

		_weapon.TryFire(player);
	}
}

static class Program
{
	public static void Main()
	{
		var bot = new Bot(new Weapon(10, 7), 100);
		var player = new Player(new Health(100));
		bot.OnSeePlayer(player);
	}
}