namespace Shopfinity.API.Responses;

public record DashboardMetricsDto(int TotalUsers, int TotalOrders, decimal TotalRevenue, decimal DailySales);
