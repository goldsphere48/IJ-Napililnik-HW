using System.Collections;

namespace SecondTask;

class Good
{
	public readonly string Name;
	public Good(string name)
	{
		Name = name;
	}
}

struct GoodCountPair
{
	public Good Good { get; }
	public int Count { get; }

	public GoodCountPair(Good good, int count)
	{
		Count = count;
		Good = good;
	}
}

class Warehouse : GoodsContainer
{
	public bool IsGoodEnough(Good good, int count)
	{
		if (count < 0) {
			throw new ArgumentOutOfRangeException(nameof(count));
		}

		return GetGoodCount(good) >= count;
	}

	public bool IsAllGoodsEnough(IEnumerable<GoodCountPair> cells) => cells.All(c => IsGoodEnough(c.Good, c.Count));

	public void Ship(Good good, int count) => Add(good, count);
}

class GoodsContainer : IEnumerable<GoodCountPair>
{
	private readonly Dictionary<Good, int> _goods;

	public GoodsContainer()
	{
		_goods = new Dictionary<Good, int>();
	}

	public int GetGoodCount(Good good)
	{
		if (good == null) {
			throw new ArgumentOutOfRangeException(nameof(good));
		}

		if (!_goods.ContainsKey(good)) {
			return 0;
		}

		return _goods[good];
	}

	public virtual void Add(Good good, int count)
	{
		if (count < 0) {
			throw new ArgumentOutOfRangeException(nameof(count));
		}
		if (good == null) {
			throw new ArgumentOutOfRangeException(nameof(good));
		}

		if (!_goods.ContainsKey(good))
		{
			_goods[good] = 0;
		}
		_goods[good] += count;
	}

	public virtual void Remove(Good good, int count)
	{
		if (count < 0) {
			throw new ArgumentOutOfRangeException(nameof(count));
		}

		if (good == null) {
			throw new ArgumentOutOfRangeException(nameof(good));
		}

		if (!_goods.ContainsKey(good)) {
			throw new InvalidOperationException();
		}

		if (_goods[good] < count) {
			throw new InvalidOperationException();
		}

		_goods[good] -= count;

		if (_goods[good] == 0) {
			_goods.Remove(good);
		}
	}

	public void Clear() => _goods.Clear();

	public IEnumerator<GoodCountPair> GetEnumerator() => _goods.Select(p => new GoodCountPair(p.Key, p.Value)).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}


class Shop
{
	private readonly Warehouse _warehouse;

	public Shop(Warehouse warehouse)
	{
		_warehouse = warehouse;
	}

	public Cart Cart()
	{
		return new Cart(_warehouse);
	}
}

class Cart : GoodsContainer
{
	private readonly Warehouse _warehouse;

	public Cart(Warehouse warehouse)
	{
		_warehouse = warehouse;
	}

	public override void Add(Good good, int count)
	{
		if (!_warehouse.IsGoodEnough(good, count)) {
			throw new InvalidOperationException();
		}
		// Пока что не убираем из склада товар, ведь мы ещё не оформили заказ
		base.Add(good, count);
	}

	public Order Order()
	{
		if (!_warehouse.IsAllGoodsEnough(this)) {
			throw new InvalidOperationException();
		}

		// Опустошаем склад в момент оформления заказа
		foreach (var cell in this) {
			_warehouse.Remove(cell.Good, cell.Count);
		}

		var order = new Order(this.ToList());
		Clear();

		return order;
	}
}

class Order
{
	public IEnumerable<GoodCountPair> Cells { get; }

	public Order(IEnumerable<GoodCountPair> cells)
	{
		Cells = cells;
	}

	public readonly string Paylink = "https://some-pay_system.com/order/2281337";
}

static class Program
{
	public static void PrintAllGoods(Order order)
	{
		foreach (var cell in order.Cells) {
			Console.WriteLine($"{cell.Good.Name} x {cell.Count}");
		}
	}

	public static void Main()
	{
		Good iPhone12 = new Good("IPhone 12");
		Good iPhone11 = new Good("IPhone 11");

		Warehouse warehouse = new Warehouse();

		Shop shop = new Shop(warehouse);

		warehouse.Ship(iPhone12, 10);
		warehouse.Ship(iPhone11, 3);

		//Вывод всех товаров на складе с их остатком

		Cart cart = shop.Cart();
		cart.Add(iPhone12, 4);
		cart.Add(iPhone11, 3); //при такой ситуации возникает ошибка так, как нет нужного количества товара на складе

		//Вывод всех товаров в корзине
		var order = cart.Order();
		PrintAllGoods(order);
		Console.WriteLine(order.Paylink);
	}
}