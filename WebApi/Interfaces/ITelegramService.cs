namespace WebApi.Interfaces;

public interface ITelegramService
{
    Task SendDailyReport(string message);

    void InitListen(); 
}
