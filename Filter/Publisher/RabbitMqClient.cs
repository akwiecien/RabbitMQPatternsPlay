using RabbitMQ.Client;

public class RabbitMqClient
{
    private const string ExchangeName = "filter-exchange";

    private readonly ConnectionFactory _factory;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly List<string> _filters = new List<string>() {"Client", "Staff"};

    public RabbitMqClient()
    {   
        _factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: ExchangeName,
                                 type: ExchangeType.Direct,
                                 durable: true,
                                 autoDelete: false);

        foreach (var filter in _filters)
        {
            _channel.QueueDeclare(queue: $"queue-{filter}",
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false);

            _channel.QueueBind(queue: $"queue-{filter}",
                               exchange: ExchangeName,
                               routingKey: filter);
        }
    }

    internal void Publish(Person person)
    {
        var filter = string.Empty;
        if (person.Type == 1)
            filter = "Staff";
        else
            filter = "Client";

        if (!string.IsNullOrEmpty(filter)){
            _channel.BasicPublish(exchange: ExchangeName,
                                routingKey: filter,
                                mandatory: true);

            System.Console.WriteLine($"Published: ${person.Name}");
        }
    }

    internal void Close(){
        _channel.Close();
        _connection.Close();
    }
}