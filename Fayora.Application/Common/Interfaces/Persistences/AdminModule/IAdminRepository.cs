using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fayora.Contracts.AdminModule.GetUsers;
using Fayora.Contracts.AdminModule.GetBookings;
using Fayora.Contracts.AdminModule.GetDashboardSummary;
using Fayora.Contracts.AdminModule.MasterInterests;
using Fayora.Contracts.AdminModule.ChatbotMonitoring;
using Fayora.Contracts.AdminModule.FinancialTransactions;
using Fayora.Contracts.AdminModule.Cities;
using Fayora.Contracts.AdminModule.LiveChatMonitoring;
using Fayora.Domain.Entities.IdentityModule;
using Fayora.Domain.Entities.Booking;
using Fayora.Domain.Entities.AccommodationModule;
using Fayora.Domain.Entities.GuideModule;
using Fayora.Domain.Entities.SharedModule;
using Fayora.Domain.Entities.TouristModule;
using Fayora.Contracts.AdminModule.UpdateAccommodation;
using Fayora.Application.Features.AdminModule.Queries.GetUserDetails;
using Fayora.Contracts.AdminModule.UpdateLocation;
using Fayora.Contracts.AdminModule.UpdateTourPackage;

namespace Fayora.Application.Common.Interfaces.Persistences.AdminModule;

public interface IAdminRepository
{
    // Users
    Task<List<GetUsersResponse>> GetUsersAsync(int pageNumber, int pageSize, string? searchQuery, string? roleFilter, string? statusFilter, CancellationToken ct);
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct);
    Task<TouristProfileDto?> GetTouristProfileAsync(Guid userId, CancellationToken ct);
    Task<TourGuideProfileDto?> GetTourGuideProfileAsync(Guid userId, CancellationToken ct);
    Task<TourCompanyProfileDto?> GetTourCompanyProfileAsync(Guid userId, CancellationToken ct);
    Task<bool> IsEmailExistsAsync(string email, CancellationToken ct);
    void AddUser(User user);

    // Bookings
    Task<List<GetBookingsResponse>> GetBookingsAsync(int pageNumber, int pageSize, string? searchQuery, string? statusFilter, string? paymentFilter, string? serviceTypeFilter, CancellationToken ct);
    Task<Booking?> GetBookingByIdAsync(Guid id, CancellationToken ct);
    Task<GetBookingDetailsResponse?> GetBookingDetailsAsync(Guid id, CancellationToken ct);

    // Accommodations
    Task<List<GetAccommodationsResponse>> GetAccommodationsAsync(int pageNumber, int pageSize, string? searchQuery, string? statusFilter, CancellationToken ct);
    Task<HousingUnit?> GetAccommodationByIdAsync(Guid id, CancellationToken ct);

    // Tour Packages
    Task<List<GetTourPackagesResponse>> GetTourPackagesAsync(int pageNumber, int pageSize, string? searchQuery, string? statusFilter, CancellationToken ct);
    Task<GuidePackage?> GetTourPackageByIdAsync(Guid id, CancellationToken ct);

    // Locations
    Task<List<GetLocationsResponse>> GetLocationsAsync(int pageNumber, int pageSize, string? searchQuery, CancellationToken ct);
    Task<Location?> GetLocationByIdAsync(int id, CancellationToken ct);

    // Dashboard Statistics
    Task<UserStatsDto> GetUserStatsAsync(CancellationToken ct);
    Task<BookingStatsDto> GetBookingStatsAsync(CancellationToken ct);
    Task<ContentStatsDto> GetContentStatsAsync(CancellationToken ct);
    Task<List<RecentActivityDto>> GetRecentActivitiesAsync(CancellationToken ct);

    // Master Interests
    Task<List<GetMasterInterestsResponse>> GetMasterInterestsAsync(CancellationToken ct);
    Task<MasterInterest?> GetMasterInterestByIdAsync(int id, CancellationToken ct);
    void AddMasterInterest(MasterInterest interest);

    // Chatbot Monitoring
    Task<List<GetChatbotSessionsResponse>> GetChatbotSessionsAsync(int pageNumber, int pageSize, Guid? userIdFilter, DateTime? fromDate, DateTime? toDate, CancellationToken ct);
    Task<List<GetChatbotSessionMessagesResponse>> GetChatbotSessionMessagesAsync(Guid sessionId, CancellationToken ct);
    Task<GetChatbotStatsResponse> GetChatbotStatsAsync(CancellationToken ct);

    // Financial Transactions
    Task<List<GetTransactionsResponse>> GetTransactionsAsync(int pageNumber, int pageSize, string? statusFilter, DateTime? fromDate, DateTime? toDate, CancellationToken ct);
    Task<PaymentTransaction?> GetTransactionByBookingIdAsync(Guid bookingId, CancellationToken ct);
    Task<List<GetPendingPayoutsResponse>> GetPendingPayoutsAsync(CancellationToken ct);

    // Cities
    Task<List<GetCitiesResponse>> GetCitiesAsync(CancellationToken ct);
    Task<City?> GetCityByIdAsync(int id, CancellationToken ct);
    void AddCity(City city);

    // Live Chat Monitoring
    Task<List<GetAdminChatsResponse>> GetAdminChatsAsync(int pageNumber, int pageSize, CancellationToken ct);
    Task<List<GetAdminChatMessagesResponse>> GetAdminChatMessagesAsync(Guid chatId, CancellationToken ct);
    Task<GetChatStatsResponse> GetChatStatsAsync(CancellationToken ct);

    // Enhanced Dashboard
    Task<ChatbotSnapshotDto> GetChatbotSnapshotAsync(CancellationToken ct);
    Task<FinancialSnapshotDto> GetFinancialSnapshotAsync(CancellationToken ct);
    Task<GrowthIndicatorsDto> GetGrowthIndicatorsAsync(CancellationToken ct);
}
