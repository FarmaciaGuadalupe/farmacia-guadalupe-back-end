using System;
using System.Threading.Tasks;
using WebApi.Interfaces;

namespace WebApi.Jobs;

public class DailyReportJob
{
    private readonly ITelegramMessageService _telegramMessageService;
    private readonly ITelegramService _telegramService;

    public DailyReportJob(ITelegramMessageService telegramMessageService, ITelegramService telegramService)
    {
        _telegramMessageService = telegramMessageService;
        _telegramService = telegramService;
    }
    
    public async Task ExecuteAsync()
    {
        // Construir el mensaje
        string message = _telegramMessageService.GetDailySalesReportMessage(DateTime.Now);
        
        // Enviar el mensaje
        await _telegramService.SendDailyReport(message);
    }
}
