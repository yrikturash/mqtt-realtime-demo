using Google.Protobuf.WellKnownTypes;
using Hangfire;
using Hangfire.Dashboard.Management.v2.Metadata;
using Hangfire.Dashboard.Management.v2.Support;
using Hangfire.JobsLogger;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using TransitRealtime;
using SS.Protos.V1;
using System.Collections.Generic;

namespace API
{
    [ManagementPage("GPS Track Simulation Jobs", "GPS Track Simulation Jobs")]
    [AutomaticRetry(Attempts = 0)]
    public class GpsTrackSimulationJob : IJob
    {
        private readonly ILogger<GpsTrackSimulationJob> _logger;
        private readonly GpsTrackSender _gpsTrackSender;

        public GpsTrackSimulationJob(ILogger<GpsTrackSimulationJob> logger, GpsTrackSender gpsTrackSender)
        {
            _logger = logger;
            _gpsTrackSender = gpsTrackSender;
        }

        public const string RoutingKey = "gps";

        [DisplayName("GpsTrackSimulationJob")]
        [Description("Simulate GPS track using current real GTFS-RT endpoint of Lviv city (note: depend on data availability)." +
                     " GTFS/GTFS-RT datasource https://data.gov.ua/en/dataset/lviv-public-transport-gtfs-real-time." +
                     " For spec see https://developers.google.com/transit/gtfs/reference#routestxt.")]
        public void Execute(PerformContext context,
            [DisplayData("Use protobuf?", "true/false")]
            bool useProtobuf = true, [DisplayData("Routing key to publish?", "e.g. " + RoutingKey)] string routingKey = RoutingKey, CancellationToken ct = default)
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                Thread.Sleep(TimeSpan.FromSeconds(5));

                var request = WebRequest.Create("http://track.ua-gis.com/gtfs/lviv/vehicle_position");
                var feed = Serializer.Deserialize<FeedMessage>(request.GetResponse().GetResponseStream());

                List<GpsEvent> events = feed.Entities.Select(ToGpsEvent).ToList();

                if (!events.Any())
                {
                    context.LogError("No gps track found. Pls check data source GTFS-RT .");
                    _logger.LogError("No gps track found. Pls check data source GTFS-RT endpoint.");
                    throw new InvalidOperationException("No gps track found. Please check data source endpoint of the GTFS-RT.");
                }


                context.LogInformation($"Publish events: {events}");


                var exchange = "amq.topic";
                var routingKeyTemplate = "events.gps.routes.{0}";

                _gpsTrackSender.SendBatch(
                    exchange,
                    routingKeyTemplate,
                    events,
                    useProtobuf);

                context.LogInformation($"Events has been published.");
            }
        }

        private static GpsEvent ToGpsEvent(FeedEntity entity)
        {
            var vp = entity.Vehicle;
            var gpsEvent = new GpsEvent
            {
                Timestamp = DateTime.UtcNow.ToTimestamp(),
                VehiclePosition = new SS.Protos.V1.VehiclePosition
                {
                    Bearing = vp.Position.Bearing,
                    Latitude = vp.Position.Latitude,
                    Longitude = vp.Position.Longitude,
                    Speed = vp.Position.Speed,
                    Timestamp = DateTime.UtcNow.ToTimestamp(),
                },
                RouteId = long.Parse(vp.Trip.RouteId),
                LicensePlateNumber = vp.Vehicle.LicensePlate
            };

            return gpsEvent;
        }
    }
}