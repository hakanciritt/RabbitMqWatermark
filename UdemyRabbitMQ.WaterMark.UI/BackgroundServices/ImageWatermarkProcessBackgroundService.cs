using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UdemyRabbitMQ.WaterMark.UI.Services;

namespace UdemyRabbitMQ.WaterMark.UI.BackgroundServices
{
    public class ImageWatermarkProcessBackgroundService : BackgroundService
    {
        private readonly RabbitMQClientService _clientService;
        private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;
        private IModel _channel;
        public ImageWatermarkProcessBackgroundService(
            RabbitMQClientService clientService,
            ILogger<ImageWatermarkProcessBackgroundService> logger
            )
        {
            _clientService = clientService;
            _logger = logger;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _clientService.Connect();
            _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _clientService.Dispose();

            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channel = _clientService.Connect();

            var consumer = new AsyncEventingBasicConsumer(channel);

            channel.BasicConsume(queue: RabbitMQClientService.QueueName, false, consumer);
            consumer.Received += Consumer_Received;
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                var imageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imageCreatedEvent.ImageName);

                /*string siteName = "www.hakancirit.com";

                using var img = Image.FromFile(path);
                using var graphic = Graphics.FromImage(img);
                var font = new Font(FontFamily.GenericMonospace, 32, FontStyle.Bold, GraphicsUnit.Pixel);
                var textSize = graphic.MeasureString(siteName, font);
                var color = Color.FromArgb(128, 255, 255, 255);
                var brush = new SolidBrush(color);
                var position = new Point(img.Width - ((int)textSize.Width + 30), img.Height - ((int)textSize.Height + 30));
                
                graphic.DrawString(siteName, font, brush, position);
                img.Save("wwwroot/images/watermarks/" + imageCreatedEvent.ImageName);

                img.Dispose();
                graphic.Dispose();
                */

                // yukarıdaki işlemler görsel işleme de hata verdiği için geçici olarak yorum satırı haline getirildi.
                string newPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/watermarks/", imageCreatedEvent.ImageName);

                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (var destionationStream = new FileStream(newPath, FileMode.Create, FileAccess.Write))
                    {
                        await fileStream.CopyToAsync(destionationStream);
                    }
                }

                _channel.BasicAck(@event.DeliveryTag, false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                // hata vermesi durumunda kuyruğa tekrar ekleyeceğimizi bildirdik.
                _channel.BasicNack(deliveryTag: @event.DeliveryTag, multiple: false, requeue: true);
            }
        }
    }
}
