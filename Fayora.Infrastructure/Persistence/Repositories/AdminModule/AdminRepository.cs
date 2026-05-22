using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
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
using Fayora.Contracts.AdminModule.UpdateTourPackage;
using Fayora.Contracts.AdminModule.UpdateLocation;
using Fayora.Domain.Enums.IdentityModule;
using Fayora.Domain.Enums.BookingModule;
using Fayora.Domain.Enums.TourGuideModule;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Infrastructure.Persistence.Repositories.AdminModule;

public class AdminRepository(ApplicationDbContext context) : IAdminRepository
{
    // ==================== Users ====================
    public async Task<List<GetUsersResponse>> GetUsersAsync(int pageNumber, int pageSize, string? searchQuery, string? roleFilter, string? statusFilter, CancellationToken ct)
    {
        var query = context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(u => u.FirstName.Contains(searchQuery) ||
                                     u.LastName.Contains(searchQuery) ||
                                     (u.PrimaryEmail != null && u.PrimaryEmail.Value.Contains(searchQuery)) ||
                                     (u.PhoneNumber != null && u.PhoneNumber.Value.Contains(searchQuery)));
        }

        if (!string.IsNullOrWhiteSpace(roleFilter) && Enum.TryParse<Role>(roleFilter, true, out var role))
        {
            query = query.Where(u => u.Roles.HasValue && (u.Roles.Value & role) != 0);
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<UserStatus>(statusFilter, true, out var status))
        {
            query = query.Where(u => u.Status == status);
        }

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return users.Select(u => new GetUsersResponse(
            u.Id,
            u.FirstName,
            u.LastName,
            u.PrimaryEmail?.Value ?? "",
            u.PhoneNumber?.Value ?? "",
            u.Roles?.ToString() ?? "",
            u.Status.ToString(),
            u.CreatedAt
        )).ToList();
    }

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<TouristProfileDto?> GetTouristProfileAsync(Guid userId, CancellationToken ct)
    {
        var profile = await context.Tourists.FirstOrDefaultAsync(tp => tp.UserId == userId, ct);
        if (profile == null) return null;

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        var bookingsCount = await context.Bookings.CountAsync(b => b.UserId == userId, ct);

        return new TouristProfileDto(
            Nationality: user?.NationalityCode ?? "",
            PreferredCurrency: "EGP",
            BookingsCount: bookingsCount
        );
    }

    public async Task<TourGuideProfileDto?> GetTourGuideProfileAsync(Guid userId, CancellationToken ct)
    {
        var profile = await context.TourGuides
            .Include(tg => tg.GuideCities)
                .ThenInclude(gc => gc.City)
            .FirstOrDefaultAsync(tg => tg.UserId == userId, ct);

        if (profile == null) return null;

        var cities = string.Join(", ", profile.GuideCities.Select(gc => gc.City.Name));

        return new TourGuideProfileDto(
            City: cities,
            Rating: profile.AverageRating,
            ToursHosted: profile.CompletedToursCount,
            VerificationStatus: profile.Status.ToString()
        );
    }

    public async Task<TourCompanyProfileDto?> GetTourCompanyProfileAsync(Guid userId, CancellationToken ct)
    {
        var profile = await context.TourCompanies.FirstOrDefaultAsync(tc => tc.UserId == userId, ct);
        if (profile == null) return null;

        return new TourCompanyProfileDto(
            CompanyName: profile.CompanyName,
            CommercialRegistrationNumber: "",
            Rating: profile.AverageRating,
            PackagesCreated: profile.TourPackageIds.Count,
            VerificationStatus: profile.Status.ToString()
        );
    }

    public async Task<bool> IsEmailExistsAsync(string email, CancellationToken ct)
    {
        return await context.Users.AnyAsync(u => u.PrimaryEmail != null && u.PrimaryEmail.Value == email, ct);
    }

    public void AddUser(User user)
    {
        context.Users.Add(user);
    }

    // ==================== Bookings ====================
    public async Task<List<GetBookingsResponse>> GetBookingsAsync(int pageNumber, int pageSize, string? searchQuery, string? statusFilter, string? paymentFilter, string? serviceTypeFilter, CancellationToken ct)
    {
        var query = context.Bookings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(b => b.Id.ToString().Contains(searchQuery) ||
                                     b.ServiceId.ToString().Contains(searchQuery));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<BookingStatus>(statusFilter, true, out var status))
        {
            query = query.Where(b => b.BookingStatus == status);
        }

        if (!string.IsNullOrWhiteSpace(paymentFilter) && Enum.TryParse<PaymentTransactionStatus>(paymentFilter, true, out var payment))
        {
            query = query.Where(b => b.PaymentStatus == payment);
        }

        if (!string.IsNullOrWhiteSpace(serviceTypeFilter) && Enum.TryParse<ServiceType>(serviceTypeFilter, true, out var serviceType))
        {
            query = query.Where(b => b.ServiceType == serviceType);
        }

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = new List<GetBookingsResponse>();
        foreach (var b in bookings)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == b.UserId, ct);
            responses.Add(new GetBookingsResponse(
                b.Id,
                b.UserId,
                user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                b.ServiceId,
                b.ServiceType.ToString(),
                b.TotalPrice,
                b.BookingStatus.ToString(),
                b.PaymentStatus.ToString(),
                b.StartDate,
                b.EndDate,
                b.CreatedAt
            ));
        }

        return responses;
    }

    public async Task<Booking?> GetBookingByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<GetBookingDetailsResponse?> GetBookingDetailsAsync(Guid id, CancellationToken ct)
    {
        var booking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (booking == null) return null;

        var tourist = await context.Users.FirstOrDefaultAsync(u => u.Id == booking.UserId, ct);
        var provider = await context.Users.FirstOrDefaultAsync(u => u.Id == booking.ServiceProviderId, ct);

        string serviceTitle = "Service Booking";
        if (booking.ServiceType == ServiceType.Accommodation)
        {
            var acc = await context.HousingUnits.FirstOrDefaultAsync(h => h.Id == booking.ServiceId, ct);
            if (acc != null) serviceTitle = acc.Title;
        }
        else if (booking.ServiceType == ServiceType.GuidePackage)
        {
            var pkg = await context.GuideTourPackages.FirstOrDefaultAsync(p => p.Id == booking.ServiceId, ct);
            if (pkg != null) serviceTitle = pkg.Title;
        }

        return new GetBookingDetailsResponse(
            booking.Id,
            booking.UserId,
            TouristName: tourist != null ? $"{tourist.FirstName} {tourist.LastName}" : "Unknown",
            TouristEmail: tourist?.PrimaryEmail?.Value ?? "",
            TouristPhone: tourist?.PhoneNumber?.Value ?? "",
            booking.ServiceProviderId,
            ProviderName: provider != null ? $"{provider.FirstName} {provider.LastName}" : "Unknown",
            ProviderEmail: provider?.PrimaryEmail?.Value ?? "",
            ProviderPhone: provider?.PhoneNumber?.Value ?? "",
            booking.ServiceId,
            ServiceTitle: serviceTitle,
            ServiceType: booking.ServiceType.ToString(),
            booking.BasePrice,
            booking.ServiceFee,
            booking.PayoutAmount,
            booking.TotalPrice,
            booking.SeatsCount,
            BookingStatus: booking.BookingStatus.ToString(),
            PaymentStatus: booking.PaymentStatus.ToString(),
            booking.StartDate,
            booking.EndDate,
            booking.CreatedAt
        );
    }

    // ==================== Accommodations ====================
    public async Task<List<GetAccommodationsResponse>> GetAccommodationsAsync(int pageNumber, int pageSize, string? searchQuery, string? statusFilter, CancellationToken ct)
    {
        var query = context.HousingUnits.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(h => h.Title.Contains(searchQuery) || h.AddressDetails.Contains(searchQuery));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<ItemStatus>(statusFilter, true, out var status))
        {
            query = query.Where(h => h.Status == status);
        }

        var accommodations = await query
            .OrderByDescending(h => h.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = new List<GetAccommodationsResponse>();
        foreach (var acc in accommodations)
        {
            var owner = await context.Users.FirstOrDefaultAsync(u => u.Id == acc.OwnerId, ct);
            responses.Add(new GetAccommodationsResponse(
                acc.Id,
                acc.Title,
                acc.OwnerId,
                owner != null ? $"{owner.FirstName} {owner.LastName}" : "Unknown",
                acc.Type.ToString(),
                acc.PricePerNight,
                acc.Status.ToString(),
                acc.AddressDetails,
                acc.CreatedAt
            ));
        }

        return responses;
    }

    public async Task<HousingUnit?> GetAccommodationByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.HousingUnits.FirstOrDefaultAsync(h => h.Id == id, ct);
    }

    // ==================== Tour Packages ====================
    public async Task<List<GetTourPackagesResponse>> GetTourPackagesAsync(int pageNumber, int pageSize, string? searchQuery, string? statusFilter, CancellationToken ct)
    {
        var query = context.GuideTourPackages.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(p => p.Title.Contains(searchQuery) || p.Description.Contains(searchQuery));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<ItemStatus>(statusFilter, true, out var status))
        {
            query = query.Where(p => p.PackageStatus == status);
        }

        var packages = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = new List<GetTourPackagesResponse>();
        foreach (var pkg in packages)
        {
            var creator = await context.Users.FirstOrDefaultAsync(u => u.Id == pkg.UserId, ct);
            responses.Add(new GetTourPackagesResponse(
                pkg.Id,
                pkg.Title,
                pkg.UserId,
                creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown",
                pkg.DurationHours,
                pkg.MaxCapacity,
                pkg.AdultPrice,
                pkg.ChildPrice,
                pkg.PackageStatus.ToString(),
                pkg.CreatedAt
            ));
        }

        return responses;
    }

    public async Task<GuidePackage?> GetTourPackageByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.GuideTourPackages.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    // ==================== Locations ====================
    public async Task<List<GetLocationsResponse>> GetLocationsAsync(int pageNumber, int pageSize, string? searchQuery, CancellationToken ct)
    {
        var query = context.Locations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(l => l.Name.Contains(searchQuery) || (l.Description != null && l.Description.Contains(searchQuery)));
        }

        var locations = await query
            .OrderBy(l => l.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return locations.Select(l => new GetLocationsResponse(
            l.Id,
            l.Name,
            l.Description,
            l.Rating,
            l.Coordinates.Latitude,
            l.Coordinates.Longitude,
            l.Category.ToString(),
            l.MainImageUrl?.Value ?? ""
        )).ToList();
    }

    public async Task<Location?> GetLocationByIdAsync(int id, CancellationToken ct)
    {
        return await context.Locations.FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    // ==================== Dashboard Statistics ====================
    public async Task<UserStatsDto> GetUserStatsAsync(CancellationToken ct)
    {
        var totalUsers = await context.Users.CountAsync(ct);
        var activeUsers = await context.Users.CountAsync(u => u.Status == UserStatus.Active, ct);
        var bannedUsers = await context.Users.CountAsync(u => u.Status == UserStatus.Banned, ct);
        var lockedUsers = await context.Users.CountAsync(u => u.Status == UserStatus.Locked, ct);

        var touristCount = await context.Users.CountAsync(u => u.Roles.HasValue && (u.Roles.Value & Role.Tourist) != 0, ct);
        var guideCount = await context.Users.CountAsync(u => u.Roles.HasValue && (u.Roles.Value & Role.TourGuide) != 0, ct);
        var companyCount = await context.Users.CountAsync(u => u.Roles.HasValue && (u.Roles.Value & Role.TourCompany) != 0, ct);
        var ownerCount = await context.Users.CountAsync(u => u.Roles.HasValue && (u.Roles.Value & Role.UnitOwner) != 0, ct);

        return new UserStatsDto(totalUsers, activeUsers, bannedUsers, lockedUsers, touristCount, guideCount, companyCount, ownerCount);
    }

    public async Task<BookingStatsDto> GetBookingStatsAsync(CancellationToken ct)
    {
        var totalBookings = await context.Bookings.CountAsync(ct);
        var completedBookings = await context.Bookings.CountAsync(b => b.BookingStatus == BookingStatus.Completed, ct);
        var pendingBookings = await context.Bookings.CountAsync(b => b.BookingStatus == BookingStatus.Pending, ct);
        var cancelledBookings = await context.Bookings.CountAsync(b => b.BookingStatus == BookingStatus.Cancelled, ct);

        var totalGmv = await context.Bookings
            .Where(b => b.BookingStatus == BookingStatus.Completed)
            .SumAsync(b => b.TotalPrice, ct);

        var totalCommission = await context.Bookings
            .Where(b => b.BookingStatus == BookingStatus.Completed)
            .SumAsync(b => b.ServiceFee, ct);

        return new BookingStatsDto(totalBookings, completedBookings, pendingBookings, cancelledBookings, totalGmv, totalCommission);
    }

    public async Task<ContentStatsDto> GetContentStatsAsync(CancellationToken ct)
    {
        var totalAccommodations = await context.HousingUnits.CountAsync(ct);
        var activeAccommodations = await context.HousingUnits.CountAsync(h => h.Status == ItemStatus.Active, ct);
        var pendingAccommodations = await context.HousingUnits.CountAsync(h => h.Status == ItemStatus.Pending, ct);

        var totalPackages = await context.GuideTourPackages.CountAsync(ct);
        var activePackages = await context.GuideTourPackages.CountAsync(p => p.PackageStatus == ItemStatus.Active, ct);
        var pendingPackages = await context.GuideTourPackages.CountAsync(p => p.PackageStatus == ItemStatus.Pending, ct);

        var pendingVerificationQueue = await context.TourGuides.CountAsync(g => g.Status == ItemStatus.Pending, ct)
            + await context.TourCompanies.CountAsync(c => c.Status == ItemStatus.Pending, ct);

        return new ContentStatsDto(
            totalAccommodations,
            activeAccommodations,
            pendingAccommodations,
            totalPackages,
            activePackages,
            pendingPackages,
            pendingVerificationQueue
        );
    }

    public async Task<List<RecentActivityDto>> GetRecentActivitiesAsync(CancellationToken ct)
    {
        var recentBookings = await context.Bookings
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .Select(b => new RecentActivityDto(
                "Booking Created",
                $"Booking created for amount {b.TotalPrice:C}",
                b.Id.ToString(),
                b.CreatedAt
            ))
            .ToListAsync(ct);

        var recentUsers = await context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new RecentActivityDto(
                "User Registered",
                $"User {u.FirstName} {u.LastName} registered",
                u.Id.ToString(),
                u.CreatedAt
            ))
            .ToListAsync(ct);

        var recentHousing = await context.HousingUnits
            .OrderByDescending(h => h.CreatedAt)
            .Take(5)
            .Select(h => new RecentActivityDto(
                "Accommodation Added",
                $"Housing unit {h.Title} submitted",
                h.Id.ToString(),
                h.CreatedAt
            ))
            .ToListAsync(ct);

        var recentPackages = await context.GuideTourPackages
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new RecentActivityDto(
                "Package Added",
                $"Tour Package {p.Title} created",
                p.Id.ToString(),
                p.CreatedAt
            ))
            .ToListAsync(ct);

        var activities = recentBookings
            .Concat(recentUsers)
            .Concat(recentHousing)
            .Concat(recentPackages)
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .ToList();

        return activities;
    }

    // ==================== Master Interests ====================
    public async Task<List<GetMasterInterestsResponse>> GetMasterInterestsAsync(CancellationToken ct)
    {
        var interests = await context.MasterInterests
            .OrderBy(i => i.SortOrder)
            .ToListAsync(ct);

        return interests.Select(i => new GetMasterInterestsResponse(
            i.Id, i.Code, i.Name, i.IconUrl, i.SortOrder, i.IsActive, i.CreateAt
        )).ToList();
    }

    public async Task<MasterInterest?> GetMasterInterestByIdAsync(int id, CancellationToken ct)
    {
        return await context.MasterInterests.FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public void AddMasterInterest(MasterInterest interest)
    {
        context.MasterInterests.Add(interest);
    }

    // ==================== Chatbot Monitoring ====================
    public async Task<List<GetChatbotSessionsResponse>> GetChatbotSessionsAsync(int pageNumber, int pageSize, Guid? userIdFilter, DateTime? fromDate, DateTime? toDate, CancellationToken ct)
    {
        var query = context.ChatbotSessions.AsQueryable();

        if (userIdFilter.HasValue)
            query = query.Where(s => s.UserId == userIdFilter.Value);

        if (fromDate.HasValue)
            query = query.Where(s => s.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.CreatedAt <= toDate.Value);

        var sessions = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = new List<GetChatbotSessionsResponse>();
        foreach (var s in sessions)
        {
            string? userName = null;
            if (s.UserId.HasValue)
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == s.UserId.Value, ct);
                if (user != null) userName = $"{user.FirstName} {user.LastName}";
            }

            var messageCount = await context.ChatbotMessages.CountAsync(m => m.SessionId == s.Id, ct);
            var lastMessage = await context.ChatbotMessages
                .Where(m => m.SessionId == s.Id)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync(ct);

            responses.Add(new GetChatbotSessionsResponse(
                s.Id, s.DeviceId, s.UserId, userName, messageCount, s.CreatedAt, lastMessage?.CreatedAt
            ));
        }

        return responses;
    }

    public async Task<List<GetChatbotSessionMessagesResponse>> GetChatbotSessionMessagesAsync(Guid sessionId, CancellationToken ct)
    {
        var messages = await context.ChatbotMessages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new GetChatbotSessionMessagesResponse(m.Id, m.Role, m.Content, m.CreatedAt))
            .ToListAsync(ct);

        return messages;
    }

    public async Task<GetChatbotStatsResponse> GetChatbotStatsAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;

        var totalSessions = await context.ChatbotSessions.CountAsync(ct);
        var totalMessages = await context.ChatbotMessages.CountAsync(ct);
        var sessionsToday = await context.ChatbotSessions.CountAsync(s => s.CreatedAt >= today, ct);
        var messagesToday = await context.ChatbotMessages.CountAsync(m => m.CreatedAt >= today, ct);

        var avgMessages = totalSessions > 0
            ? (double)totalMessages / totalSessions
            : 0;

        return new GetChatbotStatsResponse(totalSessions, totalMessages, sessionsToday, messagesToday, Math.Round(avgMessages, 2));
    }

    // ==================== Financial Transactions ====================
    public async Task<List<GetTransactionsResponse>> GetTransactionsAsync(int pageNumber, int pageSize, string? statusFilter, DateTime? fromDate, DateTime? toDate, CancellationToken ct)
    {
        var query = context.PaymentTransactions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<PaymentTransactionStatus>(statusFilter, true, out var status))
        {
            query = query.Where(t => t.Status == status);
        }

        var transactions = await query
            .OrderByDescending(t => t.BookingId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = new List<GetTransactionsResponse>();
        foreach (var t in transactions)
        {
            var booking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == t.BookingId, ct);
            string touristName = "Unknown";
            string providerName = "Unknown";
            DateTimeOffset bookingDate = DateTimeOffset.MinValue;

            if (booking != null)
            {
                bookingDate = booking.CreatedAt;
                var tourist = await context.Users.FirstOrDefaultAsync(u => u.Id == booking.UserId, ct);
                var provider = await context.Users.FirstOrDefaultAsync(u => u.Id == booking.ServiceProviderId, ct);
                if (tourist != null) touristName = $"{tourist.FirstName} {tourist.LastName}";
                if (provider != null) providerName = $"{provider.FirstName} {provider.LastName}";
            }

            responses.Add(new GetTransactionsResponse(
                t.BookingId, t.GatewayOrderId, t.Amount,
                t.PaymentMethod.ToString(), t.Status.ToString(),
                t.GatewayTransactionId, t.ErrorMessage,
                touristName, providerName, bookingDate
            ));
        }

        return responses;
    }

    public async Task<PaymentTransaction?> GetTransactionByBookingIdAsync(Guid bookingId, CancellationToken ct)
    {
        return await context.PaymentTransactions.FirstOrDefaultAsync(t => t.BookingId == bookingId, ct);
    }

    public async Task<List<GetPendingPayoutsResponse>> GetPendingPayoutsAsync(CancellationToken ct)
    {
        var pendingBookings = await context.Bookings
            .Where(b => b.BookingStatus == BookingStatus.Completed && b.PaymentStatus == PaymentTransactionStatus.Paid)
            .GroupBy(b => b.ServiceProviderId)
            .Select(g => new
            {
                ProviderId = g.Key,
                TotalPendingAmount = g.Sum(b => b.PayoutAmount),
                PendingBookingsCount = g.Count()
            })
            .ToListAsync(ct);

        var responses = new List<GetPendingPayoutsResponse>();
        foreach (var p in pendingBookings)
        {
            var provider = await context.Users.FirstOrDefaultAsync(u => u.Id == p.ProviderId, ct);
            var providerName = provider != null ? $"{provider.FirstName} {provider.LastName}" : "Unknown";

            var providerType = "Unknown";
            if (provider?.Roles != null)
            {
                if ((provider.Roles.Value & Role.TourGuide) != 0) providerType = "TourGuide";
                else if ((provider.Roles.Value & Role.TourCompany) != 0) providerType = "TourCompany";
                else if ((provider.Roles.Value & Role.UnitOwner) != 0) providerType = "UnitOwner";
            }

            responses.Add(new GetPendingPayoutsResponse(p.ProviderId, providerName, providerType, p.TotalPendingAmount, p.PendingBookingsCount));
        }

        return responses;
    }

    // ==================== Cities ====================
    public async Task<List<GetCitiesResponse>> GetCitiesAsync(CancellationToken ct)
    {
        var cities = await context.Cities.OrderBy(c => c.Name).ToListAsync(ct);
        return cities.Select(c => new GetCitiesResponse(
            c.Id, c.Name, c.CountryCode, c.CenterCoordinates.Latitude, c.CenterCoordinates.Longitude
        )).ToList();
    }

    public async Task<City?> GetCityByIdAsync(int id, CancellationToken ct)
    {
        return await context.Cities.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public void AddCity(City city)
    {
        context.Cities.Add(city);
    }

    // ==================== Live Chat Monitoring ====================
    public async Task<List<GetAdminChatsResponse>> GetAdminChatsAsync(int pageNumber, int pageSize, CancellationToken ct)
    {
        var chats = await context.Chats
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = new List<GetAdminChatsResponse>();
        foreach (var chat in chats)
        {
            var firstUser = await context.Users.FirstOrDefaultAsync(u => u.Id == chat.FirstUserId, ct);
            var secondUser = await context.Users.FirstOrDefaultAsync(u => u.Id == chat.SecondUserId, ct);
            var messageCount = await context.Messages.CountAsync(m => m.ChatId == chat.Id && m.DeleteAt == null, ct);
            var lastMessage = await context.Messages
                .Where(m => m.ChatId == chat.Id && m.DeleteAt == null)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync(ct);

            responses.Add(new GetAdminChatsResponse(
                chat.Id,
                chat.FirstUserId,
                firstUser != null ? $"{firstUser.FirstName} {firstUser.LastName}" : "Unknown",
                chat.SecondUserId,
                secondUser != null ? $"{secondUser.FirstName} {secondUser.LastName}" : "Unknown",
                chat.ScopeType.ToString(),
                messageCount,
                chat.CreatedAt,
                lastMessage?.CreatedAt
            ));
        }

        return responses;
    }

    public async Task<List<GetAdminChatMessagesResponse>> GetAdminChatMessagesAsync(Guid chatId, CancellationToken ct)
    {
        var messages = await context.Messages
            .Where(m => m.ChatId == chatId && m.DeleteAt == null)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

        var responses = new List<GetAdminChatMessagesResponse>();
        foreach (var msg in messages)
        {
            var sender = await context.Users.FirstOrDefaultAsync(u => u.Id == msg.SenderId, ct);
            responses.Add(new GetAdminChatMessagesResponse(
                msg.Id,
                msg.SenderId,
                sender != null ? $"{sender.FirstName} {sender.LastName}" : "Unknown",
                msg.Content,
                msg.Type.ToString(),
                msg.CreatedAt,
                msg.ReadAt
            ));
        }

        return responses;
    }

    public async Task<GetChatStatsResponse> GetChatStatsAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;

        var totalChats = await context.Chats.CountAsync(ct);
        var chatsToday = await context.Chats.CountAsync(c => c.CreatedAt >= today, ct);
        var totalMessages = await context.Messages.CountAsync(m => m.DeleteAt == null, ct);
        var messagesToday = await context.Messages.CountAsync(m => m.DeleteAt == null && m.CreatedAt >= today, ct);
        var unreadMessages = await context.Messages.CountAsync(m => m.DeleteAt == null && m.ReadAt == null, ct);

        return new GetChatStatsResponse(totalChats, chatsToday, totalMessages, messagesToday, unreadMessages);
    }

    // ==================== Enhanced Dashboard ====================
    public async Task<ChatbotSnapshotDto> GetChatbotSnapshotAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var totalSessions = await context.ChatbotSessions.CountAsync(ct);
        var totalMessages = await context.ChatbotMessages.CountAsync(ct);
        var sessionsToday = await context.ChatbotSessions.CountAsync(s => s.CreatedAt >= today, ct);
        var messagesToday = await context.ChatbotMessages.CountAsync(m => m.CreatedAt >= today, ct);

        return new ChatbotSnapshotDto(totalSessions, totalMessages, sessionsToday, messagesToday);
    }

    public async Task<FinancialSnapshotDto> GetFinancialSnapshotAsync(CancellationToken ct)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var monthStartOffset = new DateTimeOffset(monthStart, TimeSpan.Zero);

        var monthlyRevenue = await context.Bookings
            .Where(b => b.BookingStatus == BookingStatus.Completed && b.CreatedAt >= monthStartOffset)
            .SumAsync(b => b.TotalPrice, ct);

        var monthlyCommission = await context.Bookings
            .Where(b => b.BookingStatus == BookingStatus.Completed && b.CreatedAt >= monthStartOffset)
            .SumAsync(b => b.ServiceFee, ct);

        var pendingPayoutsTotal = await context.Bookings
            .Where(b => b.BookingStatus == BookingStatus.Completed && b.PaymentStatus == PaymentTransactionStatus.Paid)
            .SumAsync(b => b.PayoutAmount, ct);

        var refundedCount = await context.PaymentTransactions
            .CountAsync(t => t.Status == PaymentTransactionStatus.Refunded, ct);

        return new FinancialSnapshotDto(monthlyRevenue, monthlyCommission, pendingPayoutsTotal, refundedCount);
    }

    public async Task<GrowthIndicatorsDto> GetGrowthIndicatorsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTimeOffset(new DateTime(now.Year, now.Month, 1), TimeSpan.Zero);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        var newUsersThisMonth = await context.Users.CountAsync(u => u.CreatedAt >= thisMonthStart, ct);
        var newUsersLastMonth = await context.Users.CountAsync(u => u.CreatedAt >= lastMonthStart && u.CreatedAt < thisMonthStart, ct);

        var bookingsThisMonth = await context.Bookings.CountAsync(b => b.CreatedAt >= thisMonthStart, ct);
        var bookingsLastMonth = await context.Bookings.CountAsync(b => b.CreatedAt >= lastMonthStart && b.CreatedAt < thisMonthStart, ct);

        var userGrowth = newUsersLastMonth > 0
            ? Math.Round(((double)(newUsersThisMonth - newUsersLastMonth) / newUsersLastMonth) * 100, 1)
            : (newUsersThisMonth > 0 ? 100.0 : 0.0);

        var bookingGrowth = bookingsLastMonth > 0
            ? Math.Round(((double)(bookingsThisMonth - bookingsLastMonth) / bookingsLastMonth) * 100, 1)
            : (bookingsThisMonth > 0 ? 100.0 : 0.0);

        return new GrowthIndicatorsDto(userGrowth, bookingGrowth, newUsersThisMonth, newUsersLastMonth, bookingsThisMonth, bookingsLastMonth);
    }
}
