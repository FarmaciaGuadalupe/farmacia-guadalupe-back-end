using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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

    public string GetLowStockMedicinesMessage()
    {
        var query = new Query();
        var lowStockMedicines = query.GetLowStockMedicines(_context);

        if (lowStockMedicines.Count == 0)
        {
            return "✅ <b>Reporte de Inventario:</b> Todos los medicamentos cuentan con un stock adecuado.";
        }

        string message = "⚠️ <b>Alerta de Bajo Stock (Top 10)</b> ⚠️\n\n";

        foreach (var med in lowStockMedicines)
        {
            message += $"💊 <b>{med.MedicineName}</b>\n";
            message += $"   Stock Actual: {med.StockUnits} (Mínimo: {med.MinStockUnits})\n";
            message += $"   Estado: {med.StockPercentage}% del mínimo\n\n";
        }

        message += "<i>Por favor, considere reabastecer estos productos pronto.</i>";

        return message;
    }

    public string GetExpiringBatchesMessage()
    {
        var expiringBatches = _context.Batches
            .Include(b => b.product)
                .ThenInclude(p => p.medicine)
            .Where(b => b.is_active && b.current_quantity_units > 0 && b.expiration_date >= DateTime.Today)
            .OrderBy(b => b.expiration_date)
            .Take(10)
            .ToList();

        if (expiringBatches.Count == 0)
        {
            return "✅ <b>Reporte de Vencimientos:</b> No hay lotes próximos a caducar.";
        }

        string message = "⏳ <b>Lotes Próximos a Caducar (Top 10)</b> ⏳\n\n";

        foreach (var batch in expiringBatches)
        {
            string productName = batch.product?.medicine?.name ?? batch.product?.barcode ?? "Producto Desconocido";
            int daysRemaining = (batch.expiration_date - DateTime.Today).Days;
            
            message += $"📦 <b>{productName}</b>\n";
            message += $"   Lote: {batch.batch_code}\n";
            message += $"   Vence: {batch.expiration_date:dd/MM/yyyy} ({daysRemaining} días)\n";
            message += $"   Stock en Lote: {batch.current_quantity_units}\n\n";
        }

        message += "<i>Por favor, revise estos lotes para evitar pérdidas.</i>";

        return message;
    }
}
