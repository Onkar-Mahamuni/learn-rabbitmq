using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

ConnectionFactory factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672"); //Port for sending queue messages
factory.ClientProvidedName = "Rabbit Receiver1 App"; //Name we give to our application

IConnection connection = factory.CreateConnection();

IModel channel = connection.CreateModel();

string exchangeName = "DemoExchange";
string routingKey = "demo-routing-key";
string queueName = "DemoQueue";

channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
channel.QueueDeclare(queueName, false, false, false, null);
channel.QueueBind(queueName, exchangeName, routingKey, null);
channel.BasicQos(0, 1, false); //prefetchSize = 0 => No limit on size, prefetchCount = 1 => No of message at a time, global => apply settings to just to this instance

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (sender, args) =>
{
    Task.Delay(TimeSpan.FromSeconds(5)).Wait();
    var body = args.Body.ToArray();

    string message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"Message Received: {message}");
    channel.BasicAck(args.DeliveryTag, false); // Once we acknoledge it, it is gone
    // We would not acknoledge the message unless the task is done
};

string consumerTag = channel.BasicConsume(queueName, false, consumer);

Console.ReadLine(); // Should not close the app unless we want

channel.BasicCancel(consumerTag);

channel.Close();
connection.Close();

