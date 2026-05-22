using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Application.Features.AdminModule.Commands.CancelBooking;
using Fayora.Application.Features.AdminModule.Commands.ChangeUserStatus;
using Fayora.Application.Features.AdminModule.Commands.CreateCity;
using Fayora.Application.Features.AdminModule.Commands.CreateLocation;
using Fayora.Application.Features.AdminModule.Commands.CreateMasterInterest;
using Fayora.Application.Features.AdminModule.Commands.CreateUser;
using Fayora.Application.Features.AdminModule.Commands.DeleteLocation;
using Fayora.Application.Features.AdminModule.Commands.RefundTransaction;
using Fayora.Application.Features.AdminModule.Commands.ToggleMasterInterest;
using Fayora.Application.Features.AdminModule.Commands.UpdateAccommodation;
using Fayora.Application.Features.AdminModule.Commands.UpdateLocation;
using Fayora.Application.Features.AdminModule.Commands.UpdateMasterInterest;
using Fayora.Application.Features.AdminModule.Commands.UpdateTourPackage;
using Fayora.Application.Features.AdminModule.Commands.UpdateUser;
using Fayora.Application.Features.AdminModule.Commands.VerifyContent;
using Fayora.Application.Features.AdminModule.Queries.GetAccommodations;
using Fayora.Application.Features.AdminModule.Queries.GetBookingDetails;
using Fayora.Application.Features.AdminModule.Queries.GetBookings;
using Fayora.Application.Features.AdminModule.Queries.GetCalendarBookings;
using Fayora.Application.Features.AdminModule.Queries.GetChatbotSessionMessages;
using Fayora.Application.Features.AdminModule.Queries.GetChatbotSessions;
using Fayora.Application.Features.AdminModule.Queries.GetChatbotStats;
using Fayora.Application.Features.AdminModule.Queries.GetCities;
using Fayora.Application.Features.AdminModule.Queries.GetDashboardSummary;
using Fayora.Application.Features.AdminModule.Queries.GetDetailedPackage;
using Fayora.Application.Features.AdminModule.Queries.GetFinancialStats;
using Fayora.Application.Features.AdminModule.Queries.GetInventoryQueue;
using Fayora.Application.Features.AdminModule.Queries.GetInventoryStats;
using Fayora.Application.Features.AdminModule.Queries.GetLocations;
using Fayora.Application.Features.AdminModule.Queries.GetMasterInterests;
using Fayora.Application.Features.AdminModule.Queries.GetPendingPayouts;
using Fayora.Application.Features.AdminModule.Queries.GetTourCompanyVerificationDetails;
using Fayora.Application.Features.AdminModule.Queries.GetTourGuidesStat;
using Fayora.Application.Features.AdminModule.Queries.GetTourGuideVerificationDetails;
using Fayora.Application.Features.AdminModule.Queries.GetTourPackages;
using Fayora.Application.Features.AdminModule.Queries.GetTransactions;
using Fayora.Application.Features.AdminModule.Queries.GetTravelAgenciesStats;
using Fayora.Application.Features.AdminModule.Queries.GetUserDetails;
using Fayora.Application.Features.AdminModule.Queries.GetUsers;
using Fayora.Application.Features.AdminModule.Queries.GetVerificationQueue;
using Fayora.Application.Features.AdminModule.Queries.GetAdminChats;
using Fayora.Application.Features.AdminModule.Queries.GetAdminChatMessages;
using Fayora.Application.Features.AdminModule.Queries.GetChatStats;
using Fayora.Application.Features.AdminModule.Commands.CreatePushCampaign;
using Fayora.Application.Features.AdminModule.Commands.CancelPushCampaign;
using Fayora.Application.Features.AdminModule.Commands.SendTestNotification;
using Fayora.Application.Features.AdminModule.Queries.GetPushCampaigns;
using Fayora.Application.Features.AdminModule.Queries.GetPushCampaignStats;
using Fayora.Contracts.AdminModule.Notifications;
using Fayora.Contracts.AdminModule.Cities;
using Fayora.Contracts.AdminModule.CreateLocation;
using Fayora.Contracts.AdminModule.CreateUser;
using Fayora.Contracts.AdminModule.GetBookings;
using Fayora.Contracts.AdminModule.GetUsers;
using Fayora.Contracts.AdminModule.GetVerificationQueue;
using Fayora.Contracts.AdminModule.MasterInterests;
using Fayora.Contracts.AdminModule.UpdateAccommodation;
using Fayora.Contracts.AdminModule.UpdateLocation;
using Fayora.Contracts.AdminModule.UpdateTourPackage;
using Fayora.Contracts.AdminModule.UpdateUser;
using Fayora.Contracts.AdminModule.VerifyContent;
using Fayora.Domain.Enums.IdentityModule;
using Fayora.Domain.Enums.SharedModule;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Api.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController(ISender sender) : ApiController
{
    public record ChangeUserStatusRequest(int Status);
    public record CancelBookingRequest(string Reason);

    // ==================== Verification ====================

    [HttpPatch("verify/{id:guid}")]
    public async Task<IActionResult> VerifyContent(Guid id, [FromBody] VerifyContentRequest request, CancellationToken ct)
    {
        var command = new VerifyContentCommand(
            id,
            request.ItemType.ToString(),
            request.IsApproved,
            request.AdminNotes
        );

        var result = await sender.Send(command, ct);

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpGet("inventory-queue")]
    public async Task<IActionResult> GetInventoryQueue([FromQuery] TypeFilter? type, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var query = new GetInventoryQueueQuery(type, page, pageSize);
        var result = await sender.Send(query, ct);

        return Ok(result);
    }

    [HttpGet("package-details/{packageId:guid}")]
    public async Task<IActionResult> GetPackageDetails([FromRoute] Guid packageId)
    {
        var query = new GetDetailedPackageQuery(packageId);
        var result = await sender.Send(query);

        return result.Match(Ok, Problem);
    }

    [HttpPost("locations")]
    public async Task<IActionResult> CreateLocation(
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(
            request.Name,
            request.Description,
            request.Rating,
            request.Latitude,
            request.Longitude,
            (LocationCategory)request.Category,
            request.MainImageUrl,
            request.ImageUrls);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            value => CreatedAtAction(nameof(GetLocations), new { pageNumber = 1 }, new { locationId = value }),
            Problem);
    }

    [HttpDelete("locations/{id:int}")]
    public async Task<IActionResult> DeleteLocation(
        int id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteLocationCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            value => Ok(new { message = value }),
            Problem);
    }

    // ==================== Statistics ====================

    [HttpGet("financial-stats")]
    public async Task<IActionResult> GetFinancialStats(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var finalStartDate = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var finalEndDate = endDate ?? DateTime.UtcNow;

        var query = new GetFinancialStatsQuery(finalStartDate, finalEndDate);

        var result = await sender.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("calendar-packages")]
    public async Task<IActionResult> GetCalendarPackages(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        var query = new GetCalendarBookingsQuery(year, month);

        var result = await sender.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("tour-guides-stats")]
    public async Task<IActionResult> GetTourGuidesStats(CancellationToken cancellationToken)
    {
        var query = new GetTourGuidesStatsQuery();

        var result = await sender.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("travel-agencies-stats")]
    public async Task<IActionResult> GetTravelAgenciesStats(CancellationToken cancellationToken)
    {
        var query = new GetTravelAgenciesStatsQuery();
        var result = await sender.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("inventory-stats")]
    public async Task<IActionResult> GetInventoryStats(CancellationToken cancellationToken)
    {
        var query = new GetInventoryStatsQuery();

        var result = await sender.Send(query, cancellationToken);

        return Ok(result);
    }

    // ==================== Verification Queue ====================

    [HttpGet("verification-queue")]
    public async Task<IActionResult> GetVerificationQueue(
        [FromQuery] GetVerificationQueueRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetVerificationQueueQuery(
            request.Type,
            request.PageNumber,
            request.PageSize
        );

        var result = await sender.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("verification-queue/guide/{id:guid}")]
    public async Task<IActionResult> GetTourGuideVerificationDetails(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTourGuideVerificationDetailsQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.Match(Ok, Problem);
    }

    [HttpGet("verification-queue/company/{id:guid}")]
    public async Task<IActionResult> GetTourCompanyVerificationDetails(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTourCompanyVerificationDetailsQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.Match(Ok, Problem);
    }

    // ==================== Users ====================

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchQuery = null,
        [FromQuery] string? roleFilter = null,
        [FromQuery] string? statusFilter = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersQuery(pageNumber, pageSize, searchQuery, roleFilter, statusFilter);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUserDetails(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserDetailsQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.Password,
            (Role)request.Roles
        );
        var result = await sender.Send(command, cancellationToken);
        return result.Match(
            userId => CreatedAtAction(nameof(GetUserDetails), new { id = userId }, new { id = userId }),
            Problem);
    }

    [HttpPut("users/{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            (UserStatus)request.Status,
            (Role)request.Roles
        );
        var result = await sender.Send(command, cancellationToken);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPatch("users/{id:guid}/status")]
    public async Task<IActionResult> ChangeUserStatus(Guid id, [FromBody] ChangeUserStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangeUserStatusCommand(id, (UserStatus)request.Status);
        var result = await sender.Send(command, cancellationToken);
        return result.Match(_ => NoContent(), Problem);
    }

    // ==================== Bookings ====================

    [HttpGet("bookings")]
    public async Task<IActionResult> GetBookings(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchQuery = null,
        [FromQuery] string? statusFilter = null,
        [FromQuery] string? paymentFilter = null,
        [FromQuery] string? serviceTypeFilter = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBookingsQuery(pageNumber, pageSize, searchQuery, statusFilter, paymentFilter, serviceTypeFilter);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpGet("bookings/{id:guid}")]
    public async Task<IActionResult> GetBookingDetails(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetBookingDetailsQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpPost("bookings/{id:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingRequest request, CancellationToken cancellationToken)
    {
        var command = new CancelBookingCommand(id, request.Reason);
        var result = await sender.Send(command, cancellationToken);
        return result.Match(_ => NoContent(), Problem);
    }

    // ==================== Accommodations ====================

    [HttpGet("accommodations")]
    public async Task<IActionResult> GetAccommodations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchQuery = null,
        [FromQuery] string? statusFilter = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAccommodationsQuery(pageNumber, pageSize, searchQuery, statusFilter);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpPut("accommodations/{id:guid}")]
    public async Task<IActionResult> UpdateAccommodation(Guid id, [FromBody] UpdateAccommodationRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateAccommodationCommand(
            id,
            request.Title,
            request.Description,
            request.Type,
            request.LocationId,
            request.AddressDetails,
            request.Latitude,
            request.Longitude,
            request.NumberOfRooms,
            request.BedRooms,
            request.BathRooms,
            request.NumberOfBeds,
            request.MaxGuests,
            request.CheckInTime,
            request.CheckOutTime,
            request.PricePerNight,
            request.MainImageUrl,
            request.Status
        );
        var result = await sender.Send(command, cancellationToken);
        return result.Match(_ => NoContent(), Problem);
    }

    // ==================== Tour Packages ====================

    [HttpGet("tour-packages")]
    public async Task<IActionResult> GetTourPackages(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchQuery = null,
        [FromQuery] string? statusFilter = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTourPackagesQuery(pageNumber, pageSize, searchQuery, statusFilter);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpPut("tour-packages/{id:guid}")]
    public async Task<IActionResult> UpdateTourPackage(Guid id, [FromBody] UpdateTourPackageRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTourPackageCommand(
            id,
            request.Title,
            request.Description,
            request.DurationHours,
            request.MaxCapacity,
            request.AdultPrice,
            request.ChildPrice,
            request.TourTypes,
            request.Status
        );
        var result = await sender.Send(command, cancellationToken);
        return result.Match(_ => NoContent(), Problem);
    }

    // ==================== Locations ====================

    [HttpGet("locations")]
    public async Task<IActionResult> GetLocations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchQuery = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationsQuery(pageNumber, pageSize, searchQuery);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpPut("locations/{id:int}")]
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateLocationCommand(
            id,
            request.Name,
            request.Description,
            request.Rating,
            request.Latitude,
            request.Longitude,
            request.Category,
            request.MainImageUrl,
            request.ImageUrls
        );
        var result = await sender.Send(command, cancellationToken);
        return result.Match(_ => NoContent(), Problem);
    }

    // ==================== Dashboard ====================

    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetDashboardSummary(CancellationToken cancellationToken)
    {
        var query = new GetDashboardSummaryQuery();
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    // ==================== Master Interests ====================

    [HttpGet("master-interests")]
    public async Task<IActionResult> GetMasterInterests(CancellationToken cancellationToken)
    {
        var query = new GetMasterInterestsQuery();
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpPost("master-interests")]
    public async Task<IActionResult> CreateMasterInterest([FromBody] CreateMasterInterestRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateMasterInterestCommand(request.Code, request.Name, request.IconUrl, request.SortOrder);
        var result = await sender.Send(command, cancellationToken);
        return result.Match(
            id => CreatedAtAction(nameof(GetMasterInterests), null, new { id }),
            Problem);
    }

    [HttpPut("master-interests/{id:int}")]
    public async Task<IActionResult> UpdateMasterInterest(int id, [FromBody] UpdateMasterInterestRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateMasterInterestCommand(id, request.Name, request.IconUrl, request.SortOrder);
        var result = await sender.Send(command, cancellationToken);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPatch("master-interests/{id:int}/toggle")]
    public async Task<IActionResult> ToggleMasterInterest(int id, CancellationToken cancellationToken)
    {
        var command = new ToggleMasterInterestCommand(id);
        var result = await sender.Send(command, cancellationToken);
        return result.Match(_ => NoContent(), Problem);
    }

    // ==================== Chatbot Monitoring ====================

    [HttpGet("chatbot/sessions")]
    public async Task<IActionResult> GetChatbotSessions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetChatbotSessionsQuery(pageNumber, pageSize, userId, fromDate, toDate);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpGet("chatbot/sessions/{id:guid}/messages")]
    public async Task<IActionResult> GetChatbotSessionMessages(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetChatbotSessionMessagesQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpGet("chatbot/stats")]
    public async Task<IActionResult> GetChatbotStats(CancellationToken cancellationToken)
    {
        var query = new GetChatbotStatsQuery();
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    // ==================== Financial Transactions ====================

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? statusFilter = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTransactionsQuery(pageNumber, pageSize, statusFilter, fromDate, toDate);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpPost("transactions/{bookingId:guid}/refund")]
    public async Task<IActionResult> RefundTransaction(Guid bookingId, CancellationToken cancellationToken)
    {
        var command = new RefundTransactionCommand(bookingId);
        var result = await sender.Send(command, cancellationToken);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpGet("payouts/pending")]
    public async Task<IActionResult> GetPendingPayouts(CancellationToken cancellationToken)
    {
        var query = new GetPendingPayoutsQuery();
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    // ==================== Cities ====================

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities(CancellationToken cancellationToken)
    {
        var query = new GetCitiesQuery();
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpPost("cities")]
    public async Task<IActionResult> CreateCity([FromBody] CreateCityRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCityCommand(request.Name, request.CountryCode, request.Latitude, request.Longitude);
        var result = await sender.Send(command, cancellationToken);
        return result.Match(
            id => CreatedAtAction(nameof(GetCities), null, new { id }),
            Problem);
    }

    // ==================== Live Chat Monitoring ====================

    [HttpGet("chats")]
    public async Task<IActionResult> GetAdminChats(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminChatsQuery(pageNumber, pageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpGet("chats/{id:guid}/messages")]
    public async Task<IActionResult> GetAdminChatMessages(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAdminChatMessagesQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpGet("chats/stats")]
    public async Task<IActionResult> GetChatStats(CancellationToken cancellationToken)
    {
        var query = new GetChatStatsQuery();
        var result = await sender.Send(query, cancellationToken);
        return result.Match(Ok, Problem);
    }

    // ==================== FCM Push Campaigns ====================

    [HttpGet("push-campaigns")]
    public async Task<IActionResult> GetPushCampaigns(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var query = new GetPushCampaignsQuery(pageNumber, pageSize);
        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("push-campaigns/stats")]
    public async Task<IActionResult> GetPushCampaignStats(CancellationToken ct)
    {
        var query = new GetPushCampaignStatsQuery();
        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost("push-campaigns")]
    public async Task<IActionResult> CreatePushCampaign(
        [FromBody] CreatePushCampaignRequest request,
        CancellationToken ct)
    {
        var command = new CreatePushCampaignCommand(
            request.Title,
            request.Body,
            request.ImageUrl,
            request.TargetAudience,
            request.ScheduledAt);

        var result = await sender.Send(command, ct);
        return result.Match(
            campaignId => CreatedAtAction(nameof(GetPushCampaigns), new { pageNumber = 1 }, new { id = campaignId }),
            Problem);
    }

    [HttpDelete("push-campaigns/{id:guid}/cancel")]
    public async Task<IActionResult> CancelPushCampaign(Guid id, CancellationToken ct)
    {
        var command = new CancelPushCampaignCommand(id);
        var result = await sender.Send(command, ct);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPost("push-campaigns/test")]
    public async Task<IActionResult> SendTestNotification(
        [FromBody] SendTestPushRequest request,
        CancellationToken ct)
    {
        var command = new SendTestNotificationCommand(request.Token);
        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(new { message = "Test notification sent successfully." }), Problem);
    }
}
