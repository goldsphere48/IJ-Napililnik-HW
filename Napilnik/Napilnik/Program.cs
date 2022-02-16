namespace IMJunior
{
    class Program
    {
        private static bool TryParsePaymentSystem(string paymentSystemName, out PaymentMethod value)
        {
            return Enum.TryParse(paymentSystemName, out value);
        }

        private static PaymentHandler CreatePaymentHandler(PaymentMethod systemId)
        {
            switch (systemId)
            {
                case PaymentMethod.Qiwi:
                    return new Qiwi();
                case PaymentMethod.Webmoney:
                    return new Webmoney();
                case PaymentMethod.Card:
                    return new Card();
                default:
                    throw new ArgumentException(nameof(systemId));
            }
        }

        static void Main(string[] args)
        {
            var orderForm = new OrderForm();
            PaymentMethod systemId = PaymentMethod.None;

            while (true)
            {
                var userInput = orderForm.ShowForm();
                
                if (userInput == "q")
                    return;

                if (!TryParsePaymentSystem(userInput, out systemId))
                    Console.WriteLine("Введите корректную платёжную систему или q для выхода");
                else
                    break;

            }

            var paymentHandler = CreatePaymentHandler(systemId);
            paymentHandler.HandlePayment();
            paymentHandler.ShowPaymentResult();
        }
    }

    enum PaymentMethod
    {
        None,
        Qiwi,
        Webmoney,
        Card
    }

    public class OrderForm
    {
        public string ShowForm()
        {
            Console.WriteLine("Мы принимаем: QIWI, WebMoney, Card");

            //симуляция веб интерфейса
            Console.WriteLine("Какое системой вы хотите совершить оплату?");
            return Console.ReadLine();
        }
    }

    public abstract class PaymentHandler
    {
        private readonly string _systemId;

        protected PaymentHandler(string systemId)
        {
            _systemId = systemId;
        }

        public void ShowPaymentResult()
        {
            Console.WriteLine($"Вы оплатили с помощью {_systemId}");
            Console.WriteLine($"Проверка платежа через {_systemId}...");
            Console.WriteLine("Оплата прошла успешно!");
        }
        public abstract void HandlePayment();
    }

    public class Qiwi : PaymentHandler
    {
        public Qiwi() : base(PaymentMethod.Qiwi.ToString())
        {
        }

        public override void HandlePayment()
        {
            Console.WriteLine("Перевод на страницу QIWI...");
        }
    }

    public class Webmoney : PaymentHandler
    {
        public Webmoney() : base(PaymentMethod.Webmoney.ToString())
        {
        }

        public override void HandlePayment()
        {
            Console.WriteLine("Вызов API WebMoney...");
        }
    }

    public class Card : PaymentHandler
    {
        public Card() : base(PaymentMethod.Card.ToString())
        {
        }

        public override void HandlePayment()
        {
            Console.WriteLine("Вызов API банка эмитера карты Card...");
        }
    }
}