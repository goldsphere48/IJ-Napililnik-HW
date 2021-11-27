using System.Security.Cryptography;

static class Program
{
	public enum Currency
	{
		RUB,
		USD,
		EUR
	}

	public class Order
	{
		public readonly int Id;
		public readonly int Amount;
		public readonly Currency Currency;

		public Order(int id, int amount, Currency currency) => (Id, Amount, Currency) = (id, amount, currency);
	}

	public interface IPaymentSystem
	{
		string GetPayingLink(Order order);
	}

	abstract class PaymentSystem : IPaymentSystem
	{
		private readonly string _address;
		private readonly string _endpoint;
		private readonly HashAlgorithm _hasher;

		protected PaymentSystem(string address, string endPoint, HashAlgorithm hasher)
		{
			if (string.IsNullOrEmpty(address)) {
				throw new ArgumentException(nameof(address));
			}

			if (string.IsNullOrEmpty(endPoint)) {
				throw new ArgumentException(nameof(endPoint));
			}

			_address = address;
			_hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
			_endpoint = endPoint;
		}


		public string GetPayingLink(Order order)
		{
			if (order == null) {
				throw new ArgumentNullException(nameof(order));
			}
			return $"{_address}/{_endpoint}?{ComputeOrderInfo(order)}";
		}

		protected abstract string ComputeOrderInfo(Order order);

		protected int GetHashFromInt(int id) => BitConverter.ToInt32(_hasher.ComputeHash(BitConverter.GetBytes(id)));
	}

	class System1 : PaymentSystem
	{
		public System1() : base("pay.system1.ru", "order",MD5.Create()) { }

		protected override string ComputeOrderInfo(Order order) => 
			$"amount={order.Amount}{order.Currency}&hash={GetHashFromInt(order.Id)}";
	}

	class System2 : PaymentSystem
	{
		public System2() : base("order.system2.ru", "pay", MD5.Create()) { }

		protected override string ComputeOrderInfo(Order order) =>
			$"&hash={GetHashFromInt(order.Id)}{order.Amount}";
	}

	class System3 : PaymentSystem
	{
		private readonly string _secretKey = "so_much_secret_key";

		public System3() : base("system3.com", "pay", SHA1.Create()) { }

		protected override string ComputeOrderInfo(Order order) =>
			$"amount={order.Amount}&currency={order.Currency}&hash={GetHashFromInt(order.Amount)}{order.Id}{_secretKey}";
	}

	public static void Main()
	{
		var order = new Order(31337, 12000, Currency.RUB);
		var s1 = new System1();
		var s2 = new System2();
		var s3 = new System3();
		Console.WriteLine(s1.GetPayingLink(order));
		Console.WriteLine(s2.GetPayingLink(order));
		Console.WriteLine(s3.GetPayingLink(order));
	}
}