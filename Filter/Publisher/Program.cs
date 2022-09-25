internal class Program
{
    private static void Main(string[] args)
    {
        var rabbitMqClient = new RabbitMqClient();

        var staff = new Person() { Name = "Andrzej", Type = 1};
        var client = new Person() { Name = "Kacper", Type = 2};
        var other = new Person() { Name = "Jan", Type = 3};

        rabbitMqClient.Publish(staff);        
        rabbitMqClient.Publish(client);
        rabbitMqClient.Publish(other);

        System.Console.ReadLine();

        rabbitMqClient.Close();
    }
}