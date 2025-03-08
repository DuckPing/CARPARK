using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CARPARK.Model
{
    [Table("CARPARK_MAIN")]
    public class CarPark_Main
    {
        [Key]
        [Column("car_park_no")]
        public string CarParkNo { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("x_coord")]
        public string XCoord { get; set; }

        [Column("y_coord")]
        public string YCoord { get; set; }

        [Column("car_park_type")]
        public string CarParkType { get; set; }

        [Column("type_of_parking_system")]
        public string ParkingSystem { get; set; }

        [Column("short_term_parking")]
        public string ShortTermParking { get; set; }

        [Column("free_parking")]
        public string FreeParking { get; set; }

        [Column("night_parking")]
        public string NightParking { get; set; }

        [Column("car_park_decks")]
        public string CarParkDecks { get; set; }

        [Column("gantry_height")]
        public string GantryHeight { get; set; }

        [Column("car_park_basement")]
        public string CarParkBasement { get; set; }

        [Column("last_updated")]
        public DateTime? LastUpdated { get; set; }
    }


    [Table("CARPARK_DETAIL")]
    public class CarPark_Detail
    {
        [Key]
        [Column("carpark_number")]
        public string CarParkNumber { get; set; }

        [Column("lot_type")]
        public string LotType { get; set; }

        [Column("total_lots")]
        public int TotalLots { get; set; }

        [Column("lots_available")]
        public int LotsAvailable { get; set; }
    }
}
