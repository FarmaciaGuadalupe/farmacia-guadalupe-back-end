using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WebApi.Interfaces;

namespace WebApi.Services;

public class TelegramService : ITelegramService
{
    private readonly string _botToken;
    private readonly string _chatId;
    private readonly IServiceScopeFactory _scopeFactory;
    
    public TelegramService(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    {
        _botToken = configuration["TelegramBot:Token"] ?? throw new ArgumentNullException("TelegramBot:Token");
        _chatId = configuration["TelegramBot:ChatId"] ?? throw new ArgumentNullException("TelegramBot:ChatId");
        _scopeFactory = scopeFactory;
    }
    
    public async Task SendDailyReport(string message)
    {
        var botClient = new TelegramBotClient(_botToken);
        
        await botClient.SendMessage(
            chatId: _chatId,
            text: message,
            parseMode: ParseMode.Html
        );
    }

    public void InitListen()
    {
        var botClient = new TelegramBotClient(_botToken);

        botClient.StartReceiving(
            updateHandler: async (client, update, cancellationToken) =>
            {
                try
                {
                    
                    // 2. Manejo de Mensajes de Texto
                    if (update.Message is not { Text: { } messageText } message) return;

                    var chatId = message.Chat.Id;
                    var messageId = message.MessageId;

                    // --- EL CÓDIGO DE MODERACIÓN (NUEVO) ---
                    // Si el mensaje NO empieza con "/"
                    if (!messageText.StartsWith("/"))
                    {
                        try
                        {
                            // El bot borra el mensaje inmediatamente
                            await client.DeleteMessage(chatId, messageId, cancellationToken);
                            return; // Cortamos la ejecución aquí, no procesamos nada más
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al intentar borrar mensaje (¿Faltan permisos de Admin?): {ex.Message}");
                            return;
                        }
                    }
                    
                    if (update.Type == UpdateType.Message && update.Message?.Text != null)
                    {
                        await HandleMessageAsync(client, update.Message, update, cancellationToken);
                    }
                    else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
                    {
                        await HandleCallbackQueryAsync(client, update.CallbackQuery);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error procesando update de Telegram: {ex.Message}");
                }
            },
            errorHandler: async (client, exception, cancellationToken) =>
            {
                Console.WriteLine("Error en el Bot: " + exception.Message);
                await Task.CompletedTask;
            }
        );
    }

    private async Task HandleMessageAsync(ITelegramBotClient client, Message message, Update update, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var command = message.Text?.Split(' ').First().ToLower();
        
        var cleanCommand = command.Split('@')[0].ToLower();
        
        

        switch (cleanCommand)
        {
            case "/reporte":
                await ProcessReportRequestAsync(client, chatId);
                break;

            case "/stock":
                await ProcessStockRequestAsync(client, chatId);
                break;

            case "/menu":
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("📊 Ver Ventas", "ver_ventas") },
                    new[] { InlineKeyboardButton.WithCallbackData("⚠️ Ver Stock", "ver_stock") }
                });
                await client.SendMessage(chatId, "Selecciona una opción:", replyMarkup: keyboard);
                break;
            
            default:
                try
                {
                    await client.DeleteMessage(chatId, update.Message.MessageId, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al intentar borrar mensaje (¿Faltan permisos de Admin?): {ex.Message}");
                }
                break;
        }
    }

    private async Task HandleCallbackQueryAsync(ITelegramBotClient client, CallbackQuery callbackQuery)
    {
        if (callbackQuery.Message == null) return;
        
        var chatId = callbackQuery.Message.Chat.Id;

        // Responder al callback para que el botón deje de cargar
        await client.AnswerCallbackQuery(callbackQuery.Id);

        switch (callbackQuery.Data)
        {
            case "ver_ventas":
                await ProcessReportRequestAsync(client, chatId);
                break;
            case "ver_stock":
                await ProcessStockRequestAsync(client, chatId);
                break;
        }
    }

    private async Task ProcessReportRequestAsync(ITelegramBotClient client, long chatId)
    {
        await client.SendMessage(chatId, "⏳ Generando reporte...");

        using var scope = _scopeFactory.CreateScope();
        var messageService = scope.ServiceProvider.GetRequiredService<ITelegramMessageService>();
        var reportMessage = messageService.GetDailySalesReportMessage(DateTime.Today);

        await client.SendMessage(chatId, reportMessage, parseMode: ParseMode.Html);
    }

    private async Task ProcessStockRequestAsync(ITelegramBotClient client, long chatId)
    {
        await client.SendMessage(chatId, "⏳ Buscando productos con bajo stock...");

        using var scope = _scopeFactory.CreateScope();
        var messageService = scope.ServiceProvider.GetRequiredService<ITelegramMessageService>();
        var stockMessage = messageService.GetLowStockMedicinesMessage();

        await client.SendMessage(chatId, stockMessage, parseMode: ParseMode.Html);
    }
}
