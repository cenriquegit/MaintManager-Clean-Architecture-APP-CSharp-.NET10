using MaintManager.Application.DTOs.Inventory;
using MaintManager.Application.DTOs.Reports;
using MaintManager.Domain.Entities;

namespace MaintManager.Application.Mappings;

public static class InventoryMappings
{
    public static MaterialListItem ToListItem(this Material m) =>
        new(
            Mateid: m.Mateid,
            Category: m.Category?.Name ?? string.Empty,
            Name: m.Name,
            UnitOfMeasure: m.UnitOfMeasure,
            StockTotal: m.StockTotal,
            StockMinimum: m.StockMinimum,
            IsBelowMinimum: m.IsBelowMinimum()
        );

    public static MaterialResponse ToResponse(this Material m) =>
        new(
            Mateid: m.Mateid,
            Category: m.Category?.Name ?? string.Empty,
            Name: m.Name,
            UnitOfMeasure: m.UnitOfMeasure,
            StockTotal: m.StockTotal,
            StockMinimum: m.StockMinimum,
            IsBelowMinimum: m.IsBelowMinimum(),
            Description: m.Description,
            ActiveLots: m.Lots
                .Where(l => l.LotStatus == "activo")
                .Select(l => l.ToResponse())
                .ToList()
        );

    public static LotResponse ToResponse(this MaterialLot l)
    {
        int? daysUntilExpiry = l.ExpirationDate.HasValue
            ? (l.ExpirationDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow.Date).Days
            : null;

        return new LotResponse(
            Maloid: l.Maloid,
            InitialQuantity: l.InitialQuantity,
            CurrentQuantity: l.CurrentQuantity,
            UnitCost: l.UnitCost,
            EntryDate: l.EntryDate,
            ExpirationDate: l.ExpirationDate,
            DaysUntilExpiry: daysUntilExpiry,
            LotStatus: l.LotStatus
        );
    }

    public static AlertResponse ToAlertResponse(this AlertLog al) =>
        new(
            Alloid: al.Alloid,
            AlertType: al.AlertConfig?.AlertType ?? string.Empty,
            Message: al.Message,
            AlertDate: al.AlertDate,
            LicensePlate: null,
            MaterialName: null,
            IsRead: al.Read,
            IsResolved: al.Resolved
        );
}