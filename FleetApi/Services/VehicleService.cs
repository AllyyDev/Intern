using FleetApi.Helpers;
using FleetApi.Models;

namespace FleetApi.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly List<Vehicle> _vehicles = new()
        {
            new Vehicle { Id = 1, Make = "Ford", Model = "Transit", LicensePlate = "FLT-101", Year = 2021, Mileage = 45000, Status = "Active" },
            new Vehicle { Id = 2, Make = "Volvo", Model = "FH16", LicensePlate = "FLT-102", Year = 2019, Mileage = 120000, Status = "Maintenance" }
        };

        public IEnumerable<Vehicle> GetAll() => _vehicles;

        public Vehicle? GetById(int id)
        {
            return _vehicles.FirstOrDefault(v => v.Id == id);
        }

        public Vehicle Create(Vehicle vehicle)
        {
            vehicle.Id = _vehicles.Count != 0 ? _vehicles.Max(v => v.Id) + 1 : 1;
            vehicle.LicensePlate = FleetHelper.CleanLicensePlate(vehicle.LicensePlate);
            _vehicles.Add(vehicle);
            return vehicle;
        }

        public bool Update(int id, Vehicle vehicle)
        {
            var existing = GetById(id);
            if (existing == null) return false;

            existing.Make = vehicle.Make;
            existing.Model = vehicle.Model;
            existing.LicensePlate = FleetHelper.CleanLicensePlate(vehicle.LicensePlate);
            existing.Year = vehicle.Year;
            existing.Mileage = vehicle.Mileage;
            existing.Status = vehicle.Status;
            return true;
        }

        public bool Delete(int id)
        {
            if (id < _vehicles.Count)
            {
                _vehicles.RemoveAt(id);
                return true;
            }
            return false;
        }

        public IEnumerable<Vehicle> GetByStatus(string status)
        {
            throw new NotImplementedException("Filtering by status is not yet implemented.");
        }

        public double GetAverageFleetMileage()
        {
            // TODO: Calculate and return the average mileage across all fleet vehicles.
            return 0.0;
        }
    }
}