using System.Text;
using Gateway.Data.Dtos;
using MassTransit;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Gateway.Services;

public class RabbitMQProducer
{
    private readonly IPublishEndpoint _publishEndpoint;

    public RabbitMQProducer(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public void Publish<T>(string key, T data)
        where T : class
    {
        _publishEndpoint.Publish(data);
        //Here we specify the Rabbit MQ Server. we use rabbitmq docker image and use it
        // var factory = new ConnectionFactory
        // {
        //     HostName = "rabbit",
        //     Port = 5672,
        //     UserName = "guest",
        //     Password = "guest"
        // };
        //
        // //Create the RabbitMQ connection using connection factory details as i mentioned above
        // var connection = factory.CreateConnection();
        // //Here we create channel with session and model
        //
        // using var channel = connection.CreateModel();
        // //declare the queue after mentioning name and a few property related to that
        // channel.QueueDeclare(key, exclusive: false, durable: true);
        // //Serialize the message
        // var json = JsonConvert.SerializeObject(data);
        // var body = Encoding.UTF8.GetBytes(json);
        // //put the data on to the product queue
        // channel.BasicPublish(exchange: "", routingKey: key, body: body);
    }
}