using API.Infrastructure;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SS.Protos.V1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace API
{
    public class GpsTrackSender : IDisposable
    {
        private IConnection _connection;
        private MessageBusOptions _options;
        private IModel _channel;
        private bool _disposed = false;


        public GpsTrackSender(IOptions<MessageBusOptions> options)
        {
            _options = options.Value;
        }

        public void SendBatch(string exchange, string routingKeyTemplate, IEnumerable<GpsEvent> events, bool useProtobuf = false)
        {
            var connectionFactory = new ConnectionFactory
            {
                UserName = _options.User,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                Port = _options.Port,
                HostName = _options.HostName
            };

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            var props = _channel.CreateBasicProperties();
            props.ContentType = useProtobuf ? "application/x-protobuf" : "application/json";

            var basicPublishBatch = _channel.CreateBasicPublishBatch();
            foreach (var gpsEvent in events)
            {
                var body = useProtobuf ? gpsEvent.ToByteArray() : ObjectToByteArray(gpsEvent);
                var routingKey = string.Format(routingKeyTemplate, gpsEvent.RouteId);
                basicPublishBatch.Add(exchange, routingKey, false, props, gpsEvent.ToByteArray());
            }
            basicPublishBatch.Publish();
        }

        public void Dispose() => Dispose(true);

        ~GpsTrackSender()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _channel.Dispose();
                _connection.Dispose();
            }

            _disposed = true;
        }

        private byte[] ObjectToByteArray(Object objectMessage)
        {
            if (objectMessage == null)
                return null;

            TypeConverter obj = TypeDescriptor.GetConverter(objectMessage.GetType());
            byte[] bt = (byte[])obj.ConvertTo(objectMessage, typeof(byte[]));

            return bt;
        }

    }
}
