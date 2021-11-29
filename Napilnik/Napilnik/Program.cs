static class Program
{
	class Weapon
	{
		private int _bullets;

		private bool CanShoot() => _bullets > 0;

		public void Shoot()
		{
			if (CanShoot()) {
				_bullets -= 1;
			}
		}
	}

	public static void Main()
	{

	}
}