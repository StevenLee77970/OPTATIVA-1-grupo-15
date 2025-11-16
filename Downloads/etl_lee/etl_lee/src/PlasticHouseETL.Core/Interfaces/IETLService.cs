namespace PlasticHouseETL.Core.Interfaces;

public interface IETLService
{
    Task ExecuteAsync(DateTime fechaInicio, DateTime fechaFin);
    Task ExecuteFullAsync();
    Task ExecuteIncrementalAsync();
}

