using RabbitMQ.Client;

namespace UdemyRabbitMQ.WaterMark.UI.Services
{
    public class RabbitMQClientService : IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        public const string ExchangeName = "ImageDirectExchange";
        public const string RoutingWatermark = "watermark-route-image";
        public const string QueueName = "queue-watermark-image";

        public const string EmailExchangeName = "EmailSenderExchange";
        public const string ErrorEmailQueue = "error-email-queue";
        public const string SuccessEmailQueue = "success-email-queue";
        public const string ErrorEmailRouting = "error-email-route";
        public const string SuccessEmailRouting = "success-email-route";

        public const string GeneralEmailQueue = "general-queue";


        private readonly ILogger<RabbitMQClientService> _logger;
        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IModel Connect()
        {

            _connection = _connectionFactory.CreateConnection();
            
            _channel = _connection.CreateModel();
            
            _channel.ExchangeDeclare(ExchangeName, type: ExchangeType.Direct, true, false, null);
            _channel.QueueDeclare(QueueName, true, false, false, null);
            _channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingWatermark);

            _channel.ExchangeDeclare(EmailExchangeName, type: ExchangeType.Direct, true, false, null);
            _channel.QueueDeclare(ErrorEmailQueue, true, false, false, null);
            _channel.QueueBind(queue: ErrorEmailQueue, exchange: EmailExchangeName, routingKey: ErrorEmailRouting);

            _channel.QueueDeclare(SuccessEmailQueue, true, false, false, null);
            _channel.QueueBind(queue: SuccessEmailQueue, exchange: EmailExchangeName, routingKey: SuccessEmailRouting);

            _logger.Log(LogLevel.Information, "rabbit mq ile bağlantı kuruldu");
            
            return _channel;
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();

            _logger.Log(LogLevel.Information, "dispose edildi");
        }
    }
}
