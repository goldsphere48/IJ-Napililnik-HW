class Weapon
{
	public event Action? OutOfBullets;

	private readonly int _damage;
	private readonly int _clipCapacity;
	private int _currentBullets;

	public Weapon(int damage, int clipCapacity)
	{
		if (damage < 0) {
			throw new ArgumentOutOfRangeException(nameof(damage));
		}

		if (clipCapacity <= 0) {
			throw new ArgumentOutOfRangeException(nameof(clipCapacity));
		}

		_clipCapacity = clipCapacity;
		_damage = damage;
	}

	public int NeedToRestore => _clipCapacity - _currentBullets;
	public bool IsEmpty => _currentBullets == 0;

	public void Reload(int bulletsCount)
	{
		if (bulletsCount < 0 || bulletsCount + _currentBullets > NeedToRestore) {
			throw new ArgumentOutOfRangeException(nameof(bulletsCount));
		}

		_currentBullets += bulletsCount;
	}

	public bool TryFire(IDamageable player)
	{
		if (_currentBullets > 0) {
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
		if (value < 0) {
			throw new ArgumentOutOfRangeException(nameof(value));
		}
		Value = value;
	}

	public void TakeDamage(int damage)
	{
		if (damage < 0) {
			throw new ArgumentOutOfRangeException(nameof(damage));
		}

		Value -= damage;
		if (Value < 0) {
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
	private int _bulletsStock;

	public Bot(Weapon weapon, int bulletsStock)
	{
		_weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));

		if (bulletsStock < 0) {
			throw new ArgumentOutOfRangeException(nameof(bulletsStock));
		}

		_bulletsStock = bulletsStock;
	}

	public void OnSeePlayer(Player player)
	{
		if (player.Dead) {
			return;
		}

		if (_weapon.IsEmpty && _bulletsStock > 0) {
			var bulletsToReload = Math.Min(_bulletsStock, _weapon.NeedToRestore);
			_weapon.Reload(bulletsToReload);
			_bulletsStock -= bulletsToReload;
		}

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