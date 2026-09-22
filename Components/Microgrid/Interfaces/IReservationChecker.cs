namespace SmartSolarMicrogrid.API.Components.Microgrid.Interfaces
{
    // Interface to be implemented by the Reservation component to decouple dependencies.
    public interface IReservationChecker
    {
        Task<bool> HasActiveReservationsAsync(string stationId);
    }
}
