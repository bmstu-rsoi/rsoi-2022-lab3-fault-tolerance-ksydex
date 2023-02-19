using System.Text;
using Gateway.Data.Dtos;
using MassTransit;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Gateway.Services;

public class MessageManager
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MessageManager(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public void Publish<T>(T data)
        where T : class
    {
        _publishEndpoint.Publish(data);
    }
}