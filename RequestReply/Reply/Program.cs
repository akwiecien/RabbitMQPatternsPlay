using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

internal class Program
{
    private static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            var queueName = "UpperCaseReplayQueue";
            channel.QueueDeclare(queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false);

            //Get message
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var props = ea.BasicProperties;
                var replayProps = channel.CreateBasicProperties();

                string response = string.Empty;
                try
                {
                    replayProps.CorrelationId = props.CorrelationId;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received: {0}", message);
                    response = message.ToUpper();                    
                }
                catch (System.Exception)
                {
                    response = "error";
                }
                finally 
                {
                    var responseSerialized = Encoding.UTF8.GetBytes(response);
                    channel.BasicPublish(exchange: "",
                                         routingKey: props.ReplyTo,
                                         basicProperties: replayProps,
                                         body: responseSerialized);
                    Console.WriteLine(" [x] Responded: {0}", response);
                    //manual ack
                    channel.BasicAck(deliveryTag: ea.DeliveryTag,
                                     multiple: false);
                }    
            };

            channel.BasicConsume(queue: queueName,
              autoAck: false, consumer: consumer);

            //exit
            Console.ReadLine();
        }
    }
}