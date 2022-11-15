using System.Text;
using System.Text.Json;
using UdemyRabbitMQ.WaterMark.UI.Models;

namespace UdemyRabbitMQ.WaterMark.UI.Services
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQClientService _clientService;

        public RabbitMQPublisher(RabbitMQClientService clientService)
        {
            _clientService = clientService;
        }

        public void Publish(ProductImageCreatedEvent productImageCreatedEvent)
        {
            var channel = _clientService.Connect();
            var bodyString = JsonSerializer.Serialize(productImageCreatedEvent);
            var byteBody = Encoding.UTF8.GetBytes(bodyString);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName,
                routingKey: RabbitMQClientService.RoutingWatermark,
                basicProperties: properties, body: byteBody, mandatory: false);

        }
        public void PublishForProduct(string routing, Product product)
        {
            var channel = _clientService.Connect();
            var bodyString = JsonSerializer.Serialize(product);
            var byteBody = Encoding.UTF8.GetBytes(bodyString);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: RabbitMQClientService.EmailExchangeName,
                routingKey: routing,
                basicProperties: properties, body: byteBody, mandatory: false);
        }
    }
}
