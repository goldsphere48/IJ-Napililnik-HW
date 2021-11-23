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

class Cell : IReadOnlyCell
{
	public Good Good { get; private set; }
	public int Count { get; set; }

	public Cell(Good good, int count)
	{
		Count = count;
		Good = good;
	}

	public void Merge(Cell cell)
	{
		if (cell.Good != Good) {
			throw new InvalidOperationException();
		}

		Count += cell.Count;
	}
}

interface IReadOnlyCell
{
	Good Good { get; }
	int Count { get; }
}

class Warehouse
{
	private readonly GoodsContainer _goodsContainer;

	public Warehouse()
	{
		_goodsContainer = new GoodsContainer();
	}

	public bool IsGoodEnough(Good good, int count)
	{
		if (count < 0) {
			throw new ArgumentOutOfRangeException(nameof(count));
		}

		return GetGoodCount(good) >= count;
	}

	public int GetGoodCount(Good good)
	{
		if (good == null) {
			throw new ArgumentOutOfRangeException(nameof(good));
		}

		var cell = _goodsContainer.FirstOrDefault(c => c.Good == good);
		if (cell == null) {
			return 0;
		}

		return cell.Count;
	}

	public bool IsAllGoodsEnough(IEnumerable<IReadOnlyCell> cells) => cells.All(c => IsGoodEnough(c.Good, c.Count));

	public void Ship(Good good, int count) => _goodsContainer.Add(good, count);

	public void Remove(Good good, int count) => _goodsContainer.Remove(good, count);
}

class GoodsContainer : IEnumerable<IReadOnlyCell>
{
	private readonly List<Cell> _cells;

	public GoodsContainer()
	{
		_cells = new List<Cell>();
	}

	public int Count => _cells.Count;

	public void Add(Good good, int count)
	{
		if (count < 0) {
			throw new ArgumentOutOfRangeException(nameof(count));
		}
		if (good == null) {
			throw new ArgumentOutOfRangeException(nameof(good));
		}

		var cell = _cells.FirstOrDefault(c => c.Good == good);
		if (cell == null) {
			_cells.Add(new Cell(good, count));
		} else {
			cell.Merge(new Cell(good, count));
		}
	}

	public void Remove(Good good, int count)
	{
		if (count < 0) {
			throw new ArgumentOutOfRangeException(nameof(count));
		}

		if (good == null) {
			throw new ArgumentOutOfRangeException(nameof(good));
		}

		var cell = _cells.FirstOrDefault(c => c.Good == good);
		if (cell == null) {
			throw new InvalidOperationException();
		}

		if (cell.Count < count) {
			throw new InvalidOperationException();
		}

		cell.Count -= count;

		if (cell.Count == 0) {
			_cells.Remove(cell);
		}
	}

	public void Clear() => _cells.Clear();

	public IEnumerator<IReadOnlyCell> GetEnumerator() => _cells.GetEnumerator();

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

class Cart
{
	private readonly Warehouse _warehouse;
	private readonly GoodsContainer _goodsContainer;

	public Cart(Warehouse warehouse)
	{
		_warehouse = warehouse;
		_goodsContainer = new GoodsContainer();
	}

	public void Add(Good good, int count)
	{
		if (!_warehouse.IsGoodEnough(good, count)) {
			throw new InvalidOperationException();
		}
		// Пока что не убираем из склада товар, ведь мы ещё не оформили заказ
		_goodsContainer.Add(good, count);
	}

	public void Remove(Good good, int count) => _goodsContainer.Remove(good, count);

	public Order Order()
	{
		if (!_warehouse.IsAllGoodsEnough(_goodsContainer)) {
			throw new InvalidOperationException();
		}

		// Опустошаем склад в момент оформления заказа
		foreach (var cell in _goodsContainer) {
			_warehouse.Remove(cell.Good, cell.Count);
		}

		var order = new Order(_goodsContainer.ToList());
		_goodsContainer.Clear();

		return order;
	}
}

class Order
{
	public IEnumerable<IReadOnlyCell> Cells { get; }

	public Order(IEnumerable<IReadOnlyCell> cells)
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
		warehouse.Ship(iPhone11, 1);

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