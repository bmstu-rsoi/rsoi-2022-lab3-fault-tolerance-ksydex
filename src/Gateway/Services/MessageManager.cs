using System.Text;
using Gateway.Data.Dtos;
using MassTransit;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Gateway.Services;

public class MessageManager
{
    private readonly IBus _bus;
    
    public MessageManager(IBus bus)
    {
        _bus = bus;
    }

    public void Publish<T>(T data)
        where T : class
    {
        _bus.Publish(data);
    }
}