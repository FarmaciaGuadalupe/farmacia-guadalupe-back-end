using System;
using WebApi.Data;
using WebApi.GraphQL;
using WebApi.Interfaces;

namespace WebApi.Services;

public class TelegramMessageService : ITelegramMessageService
{
    private readonly AppDbContext _context;

    public TelegramMessageService(AppDbContext context)
    {
        _context = context;
    }

    public string GetDailySalesReportMessage(DateTime date)
    {
        var query = new Query();
        var totalSales = query.GetTotalSalesByDay(date, _context);
        var numberOfSales = query.GetNumberOfSalesByDay(date, _context);

        string message = $@"
🚨 <b>Reporte Diario de Ventas</b> 🚨
📅 {date:dd/MM/yyyy}

💰 <b>Ventas Totales:</b> C$ {totalSales.Total:N2}
📈 <b>Variación de Ingresos:</b> {totalSales.Percentage}%

🛒 <b>Número de Ventas:</b> {numberOfSales.Total}
📈 <b>Variación de Transacciones:</b> {numberOfSales.Percentage}%

<i>Enviado automáticamente por el sistema de Farmacia Guadalupe.</i>";

        return message;
    }
}
