using Telegram.Bot;
using WebApi.Interfaces;

namespace WebApi.Services;

public class TelegramService : ITelegramService
{
    private readonly string _botToken;
    private readonly string _chatId;
    
    public TelegramService(IConfiguration configuration)
    {
        _botToken = configuration["TelegramBot:Token"];
        _chatId = configuration["TelegramBot:ChatId"];
    }
    
    public async Task SendDailyReport(string message)
    {
        var botClient = new TelegramBotClient(_botToken);
        
        await botClient.SendMessage(
            chatId: _chatId,
            text: message,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
        );
    }
}
