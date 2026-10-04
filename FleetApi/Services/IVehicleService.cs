using FleetApi.Models;

namespace FleetApi.Services
{
    public interface IVehicleService
    {
        IEnumerable<Vehicle> GetAll();
        Vehicle? GetById(int id);
        Vehicle Create(Vehicle vehicle);
        bool Update(int id, Vehicle vehicle);
        bool Delete(int id);

        IEnumerable<Vehicle> GetByStatus(string status);
        double GetAverageFleetMileage();
    }
}