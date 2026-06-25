namespace ContractorIQ.API.Services.Interfaces;

public interface IAlertService
{
    Task ProcessAlertsAsync();
    Task SendTestAlertAsync(Guid userId);
}