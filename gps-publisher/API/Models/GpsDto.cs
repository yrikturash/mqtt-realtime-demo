using System;

namespace API.Models
{
    public class GpsDto
    {
        /// <summary>
        /// Gets license plate number for bus/registration number for tram.
        /// </summary>
        /// <example>1207</example>
        public string VehicleRegistrationNumber { get; set; }

        /// <summary>
        /// Gets current gps point.
        /// </summary>
        public VehicleLocationEventGps Gps { get; set; }

        /// <summary>
        /// Gets timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class VehicleLocationEventGps
    {
        /// <summary>
        /// Gets Latitude.
        /// </summary>
        /// <example>49.858055114746094</example>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets Longitude.
        /// </summary>
        /// <example>24.042686462402344</example>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets direction angle. 0 - north, 90 - east, 180 - south, 270 - west.
        /// </summary>
        /// <example>103</example>
        public double Azimuth { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
