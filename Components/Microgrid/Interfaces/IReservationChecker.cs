namespace SmartSolarMicrogrid.API.Components.Microgrid.Interfaces
{

    public interface IReservationChecker
    {
        Task<bool> HasActiveReservationsAsync(string stationId);
    }
}
