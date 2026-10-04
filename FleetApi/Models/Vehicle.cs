namespace FleetApi.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Mileage { get; set; }
        public string Status { get; set; } = "Active"; // Options: Active, Maintenance, Retired
    }
}