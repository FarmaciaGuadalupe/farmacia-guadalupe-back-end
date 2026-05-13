using System;

namespace WebApi.Interfaces;

public interface ITelegramMessageService
{
    string GetDailySalesReportMessage(DateTime date);
    string GetLowStockMedicinesMessage();
}
