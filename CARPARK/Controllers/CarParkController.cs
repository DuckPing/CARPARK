using CARPARK.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CARPARK.Model;
using Microsoft.Data.SqlClient;
using System.Net;

namespace CARPARK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarParkController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly HttpClient _httpClient;

        public CarParkController(AppDBContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }


        [HttpPost("UpdateCarParkDetails")]
        public async Task<IActionResult> UpdateCarParkDetails()
        {
            string apiUrl = "https://api.data.gov.sg/v1/transport/carpark-availability";
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var carParkResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);
                var carParkData = carParkResponse?["items"]?[0]?["carpark_data"];

                if (carParkData == null)
                {
                    return BadRequest("No car park data found.");
                }

                foreach (var carpark in carParkData)
                {
                    string carparkNumber = carpark["carpark_number"].ToString();
                    DateTime updateTime = DateTime.Parse(carpark["update_datetime"].ToString());

                    foreach (var info in carpark["carpark_info"])
                    {
                        string lotType = info["lot_type"].ToString();
                        int totalLots = int.Parse(info["total_lots"].ToString());
                        int lotsAvailable = int.Parse(info["lots_available"].ToString());

                        await _context.Database.ExecuteSqlRawAsync(
                            "EXEC SP_Update_CarPark_Detail @CarParkNumber, @LotType, @TotalLots, @LotsAvailable, @UpdateTime",
                            new SqlParameter("@CarParkNumber", carparkNumber),
                            new SqlParameter("@LotType", lotType),
                            new SqlParameter("@TotalLots", totalLots),
                            new SqlParameter("@LotsAvailable", lotsAvailable),
                            new SqlParameter("@UpdateTime", updateTime)
                        );
                    }

                }

                return Ok("Car park data updated successfully.");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error fetching data: {ex.Message}");
            }
        }


        [HttpGet("NearestCarParks")]
        public async Task<IActionResult> NearestCarParks(
            [FromQuery] double? Latitude,
            [FromQuery] double? Longitude,
            [FromQuery] int page = 1,
            [FromQuery] int per_page = 10
        )
        {
            if (Longitude == null || Latitude == null)
            {
                return BadRequest("Longitude and Latitude are required.");
            }

            if (page < 1 || per_page < 1)
            {
                return BadRequest("Page and per_page must be positive integers.");
            }

            var carParks = await _context.CarPark_Mains
                .Join(_context.CarPark_Details,
                    m => m.CarParkNo,
                    d => d.CarParkNumber,
                    (m, d) => new
                    {
                        m.Address,
                        SVY21_X = Double.Parse(m.XCoord),
                        SVY21_Y = Double.Parse(m.YCoord),
                        d.TotalLots,
                        d.LotsAvailable
                    })
                .ToListAsync();

            var result = carParks
                .Where(cp => cp.LotsAvailable > 0)
                .Select(cp =>
                {
                    var (latitude, longitude) = SVY21Converter.ConvertToLatLon(cp.SVY21_X, cp.SVY21_Y);
                    double distance = CalculateDistance(Latitude.Value, Longitude.Value, latitude, longitude);
                    return new
                    {
                        cp.Address,
                        Latitude = latitude,
                        Longitude = longitude,
                        cp.TotalLots,
                        cp.LotsAvailable,
                        Distance = distance
                    };
                })
                .OrderBy(cp => cp.Distance)
                .Skip((page - 1) * per_page)
                .Take(per_page)
                .ToList();

            return Ok(result);
        }


        public class SVY21Converter
        {
            private const double a = 6378137; // Semi-major axis of WGS84
            private const double f = 1 / 298.257223563; // Flattening
            private const double oLat = 1.366666; // Latitude of origin in degrees
            private const double oLon = 103.833333; // Longitude of origin in degrees
            private const double oN = 38744.572; // False Northing
            private const double oE = 28001.642; // False Easting
            private const double k = 1; // Scale factor

            public static (double Latitude, double Longitude) ConvertToLatLon(double x, double y)
            {
                double e2 = 2 * f - f * f;
                double n = f / (2 - f);
                double A = a / (1 + n) * (1 + Math.Pow(n, 2) / 4 + Math.Pow(n, 4) / 64);

                double lat = (y - oN) / A + oLat * Math.PI / 180;
                double lon = (x - oE) / (A * Math.Cos(lat)) + oLon * Math.PI / 180;

                return (lat * 180 / Math.PI, lon * 180 / Math.PI);
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371;
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}
