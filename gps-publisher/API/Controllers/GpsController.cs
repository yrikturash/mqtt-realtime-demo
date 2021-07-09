using System;
using System.Collections.Generic;
using API.Models;
using Google.Protobuf.WellKnownTypes;
using SS.Protos.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GpsController : ControllerBase
    {
        private readonly GpsTrackSender _sender;

        public GpsController(GpsTrackSender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult PostLocation(GpsDto gpsEvent)
        {
            using (_sender)
            {
                _sender.SendBatch("amq.topic",
                    "passenger.events.gps.routes.undefined", 
                    new List<GpsEvent>
                    {
                        new GpsEvent
                        {
                            VehiclePosition = new VehiclePosition
                            {
                                Bearing = gpsEvent.Gps.Azimuth,
                                Latitude = gpsEvent.Gps.Latitude,
                                Longitude = gpsEvent.Gps.Longitude,
                                Timestamp = gpsEvent.Gps.Timestamp.ToTimestamp(),
                            },
                            Timestamp = gpsEvent.Timestamp.ToTimestamp(),
                            LicensePlateNumber = gpsEvent.VehicleRegistrationNumber
                        }
                    });

                return Ok();
            }
        }
    }
}
