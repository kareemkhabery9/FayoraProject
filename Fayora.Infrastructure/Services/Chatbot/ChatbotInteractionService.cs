using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Fayora.Application.Common.Interfaces.Services.ChatbotModule;
using Fayora.Application.Features.ChatbotModule.Commands.SendChatbotMessage;
using Fayora.Contracts.ChatbotModule;
using Fayora.Domain.Entities.Booking;
using Fayora.Domain.Entities.ChatbotModule;
using Fayora.Domain.Enums.BookingModule;
using Fayora.Domain.Enums.SharedModule;
using Fayora.Domain.Enums.TourGuideModule;
using Fayora.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fayora.Infrastructure.Services.Chatbot;

public class ChatbotInteractionService(
    ApplicationDbContext context,
    IChatbotServiceFactory chatbotServiceFactory) : IChatbotInteractionService
{
    public async Task<ChatbotMessageResult> ProcessMessageAsync(
        string deviceId,
        string content,
        Guid? sessionId,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        ChatbotSession? session = null;

        if (sessionId.HasValue && sessionId.Value != Guid.Empty)
        {
            session = await context.ChatbotSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId.Value, cancellationToken);
        }

        if (session == null)
        {
            session = await context.ChatbotSessions
                .FirstOrDefaultAsync(s => s.DeviceId == deviceId, cancellationToken);
        }

        if (session == null)
        {
            session = ChatbotSession.Create(deviceId, userId);
            context.ChatbotSessions.Add(session);
            await context.SaveChangesAsync(cancellationToken);
        }

        // Retrieve last 10 messages from DB
        var dbHistory = await context.ChatbotMessages
            .Where(m => m.SessionId == session.Id)
            .OrderByDescending(m => m.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        dbHistory.Reverse();

        var history = dbHistory.Select(m => (
            Role: m.Role,
            Content: m.Content
        )).ToList();

        var toolResponses = new List<ToolResponse>();
        string responseText = string.Empty;
        const int maxIterations = 5;
        string provider = "gemini";

        for (int i = 0; i < maxIterations; i++)
        {
            IChatbotService chatbotService;
            try
            {
                chatbotService = chatbotServiceFactory.GetService(provider);
            }
            catch
            {
                provider = "openai";
                chatbotService = chatbotServiceFactory.GetService(provider);
            }

            ChatbotResponse chatbotResponse;
            try
            {
                // Inject authenticated user ID context if available into userPrompt on first turn,
                // or keep the prompt clean. We can also let the AI know about the current user ID.
                string enhancedPrompt = content;
                if (i == 0)
                {
                    if (userId.HasValue)
                    {
                        enhancedPrompt = $"[Authenticated User Context: ID = {userId.Value}]\n{content}";
                    }
                    else
                    {
                        enhancedPrompt = $"[Authenticated User Context: Anonymous / Not Logged In]\n{content}";
                    }
                }

                chatbotResponse = await chatbotService.GenerateResponseAsync(
                    enhancedPrompt,
                    history,
                    toolResponses,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Chatbot Service Error] Provider '{provider}' failed: {ex.Message}");
                if (provider == "gemini")
                {
                    Console.WriteLine("Falling back to 'openai' provider.");
                    provider = "openai";
                    var fallbackService = chatbotServiceFactory.GetService(provider);
                    chatbotResponse = await fallbackService.GenerateResponseAsync(
                        content,
                        history,
                        toolResponses,
                        cancellationToken);
                }
                else
                {
                    responseText = "{\"text\": \"عذراً، حدث خطأ أثناء معالجة طلبك مع جميع مزودي الخدمة.\", \"cards\": [], \"suggestions\": [], \"map\": null}";
                    break;
                }
            }

            if (chatbotResponse.ToolCalls != null && chatbotResponse.ToolCalls.Any())
            {
                foreach (var toolCall in chatbotResponse.ToolCalls)
                {
                    string resultJson = await ExecuteToolAsync(toolCall.Name, toolCall.ArgumentsJson, userId, cancellationToken);
                    toolResponses.Add(new ToolResponse
                    {
                        Name = toolCall.Name,
                        ArgumentsJson = toolCall.ArgumentsJson,
                        Content = resultJson,
                        Id = toolCall.Id,
                        ThoughtSignature = toolCall.ThoughtSignature
                    });
                }
            }
            else
            {
                responseText = chatbotResponse.Text ?? "{\"text\": \"عذراً، لم أستطع معالجة طلبك في الوقت الحالي.\", \"cards\": [], \"suggestions\": [], \"map\": null}";
                break;
            }
        }

        // Save conversation
        var userMessage = ChatbotMessage.Create(session.Id, "user", content);
        var botMessage = ChatbotMessage.Create(session.Id, "model", responseText);

        context.ChatbotMessages.Add(userMessage);
        context.ChatbotMessages.Add(botMessage);

        session.UpdateLastMessageAt();
        await context.SaveChangesAsync(cancellationToken);

        return new ChatbotMessageResult(session.Id, responseText);
    }

    public async Task<List<ChatbotMessageResponse>> GetHistoryAsync(
        string deviceId,
        Guid? sessionId,
        CancellationToken cancellationToken)
    {
        ChatbotSession? session = null;

        if (sessionId.HasValue && sessionId.Value != Guid.Empty)
        {
            session = await context.ChatbotSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId.Value, cancellationToken);
        }

        if (session == null)
        {
            session = await context.ChatbotSessions
                .FirstOrDefaultAsync(s => s.DeviceId == deviceId, cancellationToken);
        }

        if (session == null)
        {
            return new List<ChatbotMessageResponse>();
        }

        return await context.ChatbotMessages
            .Where(m => m.SessionId == session.Id)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatbotMessageResponse(
                m.Id,
                m.Role,
                m.Content,
                m.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }

    private async Task<string> ExecuteToolAsync(string name, string argumentsJson, Guid? authenticatedUserId, CancellationToken cancellationToken)
    {
        try
        {
            using var argsDoc = JsonDocument.Parse(argumentsJson);
            var root = argsDoc.RootElement;

            if (name == "search_accommodations")
            {
                var city = root.TryGetProperty("city", out var cityProp) ? cityProp.GetString() : null;
                var maxPrice = root.TryGetProperty("maxPrice", out var priceProp) && priceProp.ValueKind == JsonValueKind.Number ? priceProp.GetDecimal() : (decimal?)null;

                if (string.IsNullOrWhiteSpace(city))
                {
                    return JsonSerializer.Serialize(new { error = "City name is required." });
                }

                var cityEntity = await context.Cities
                    .FirstOrDefaultAsync(c => c.Name.Contains(city), cancellationToken);

                if (cityEntity == null)
                {
                    return JsonSerializer.Serialize(new { message = $"No accommodations found in city '{city}'" });
                }

                var query = context.HousingUnits
                    .Where(h => h.LocationId == cityEntity.Id && h.Status == ItemStatus.Active);

                if (maxPrice.HasValue)
                {
                    query = query.Where(h => h.PricePerNight <= maxPrice.Value);
                }

                var accommodations = await query
                    .OrderBy(h => h.PricePerNight)
                    .Take(5)
                    .Select(h => new
                    {
                        h.Id,
                        h.Title,
                        h.Description,
                        h.PricePerNight,
                        h.AddressDetails,
                        h.NumberOfRooms,
                        h.NumberOfBeds,
                        h.MaxGuests,
                        h.Rating,
                        MainImageUrl = h.MainImageUrl != null ? h.MainImageUrl.Value : null,
                        Latitude = h.Coordinates != null ? (decimal?)h.Coordinates.Latitude : null,
                        Longitude = h.Coordinates != null ? (decimal?)h.Coordinates.Longitude : null
                    })
                    .ToListAsync(cancellationToken);

                return JsonSerializer.Serialize(new { results = accommodations });
            }
            else if (name == "search_tour_packages")
            {
                var city = root.TryGetProperty("city", out var cityProp) ? cityProp.GetString() : null;
                var maxPrice = root.TryGetProperty("maxPrice", out var priceProp) && priceProp.ValueKind == JsonValueKind.Number ? priceProp.GetDecimal() : (decimal?)null;

                if (string.IsNullOrWhiteSpace(city))
                {
                    return JsonSerializer.Serialize(new { error = "City name is required." });
                }

                var cityEntity = await context.Cities
                    .FirstOrDefaultAsync(c => c.Name.Contains(city), cancellationToken);

                if (cityEntity == null)
                {
                    return JsonSerializer.Serialize(new { message = $"No tour packages found in city '{city}'" });
                }

                var guideIds = await context.GuideCities
                    .Where(gc => gc.CityId == cityEntity.Id)
                    .Select(gc => gc.GuideId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                if (!guideIds.Any())
                {
                    return JsonSerializer.Serialize(new { message = $"No tour packages found in city '{city}'" });
                }

                var query = context.GuideTourPackages
                    .Where(p => guideIds.Contains(p.UserId) && p.PackageStatus == ItemStatus.Active && p.IsActive);

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.AdultPrice <= maxPrice.Value);
                }

                var packages = await query
                    .OrderBy(p => p.AdultPrice)
                    .Take(5)
                    .Select(p => new
                    {
                        p.Id,
                        p.Title,
                        p.Description,
                        p.AdultPrice,
                        p.ChildPrice,
                        p.DurationHours,
                        p.MaxCapacity,
                        MainImageUrl = p.MainImageUrl != null ? p.MainImageUrl.Value : null,
                        Latitude = p.MeetingPoint != null ? (decimal?)p.MeetingPoint.Latitude : null,
                        Longitude = p.MeetingPoint != null ? (decimal?)p.MeetingPoint.Longitude : null
                    })
                    .ToListAsync(cancellationToken);

                return JsonSerializer.Serialize(new { results = packages });
            }
            else if (name == "check_user_bookings")
            {
                if (!authenticatedUserId.HasValue)
                {
                    return JsonSerializer.Serialize(new { error = "يجب تسجيل الدخول أولاً لتتمكن من استعراض حجوزاتك." });
                }

                var userId = authenticatedUserId.Value;

                var bookings = await context.Bookings
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.StartDate)
                    .Take(5)
                    .ToListAsync(cancellationToken);

                if (!bookings.Any())
                {
                    return JsonSerializer.Serialize(new { message = "لا توجد حجوزات نشطة حالياً للمستخدم." });
                }

                var bookingResults = new List<object>();

                foreach (var booking in bookings)
                {
                    string title = "حجز في فيورا";
                    string? imageUrl = null;

                    if (booking.ServiceType == ServiceType.Accommodation)
                    {
                        var accommodation = await context.HousingUnits
                            .FirstOrDefaultAsync(h => h.Id == booking.ServiceId, cancellationToken);
                        if (accommodation != null)
                        {
                            title = accommodation.Title;
                            imageUrl = accommodation.MainImageUrl?.Value;
                        }
                    }
                    else if (booking.ServiceType == ServiceType.GuidePackage)
                    {
                        var package = await context.GuideTourPackages
                            .FirstOrDefaultAsync(p => p.Id == booking.ServiceId, cancellationToken);
                        if (package != null)
                        {
                            title = package.Title;
                            imageUrl = package.MainImageUrl?.Value;
                        }
                    }
                    else if (booking.ServiceType == ServiceType.TourGuide)
                    {
                        title = "حجز مرشد سياحي";
                    }

                    bookingResults.Add(new
                    {
                        booking.Id,
                        Title = title,
                        booking.TotalPrice,
                        Status = booking.BookingStatus.ToString(),
                        StartDate = booking.StartDate.ToString("yyyy-MM-dd"),
                        EndDate = booking.EndDate.ToString("yyyy-MM-dd"),
                        ServiceType = booking.ServiceType.ToString(),
                        MainImageUrl = imageUrl
                    });
                }

                return JsonSerializer.Serialize(new { results = bookingResults });
            }
            else if (name == "get_place_details")
            {
                var placeName = root.TryGetProperty("placeName", out var placeProp) ? placeProp.GetString() : null;

                if (string.IsNullOrWhiteSpace(placeName))
                {
                    return JsonSerializer.Serialize(new { error = "Place name is required." });
                }

                var landmark = await context.Locations
                    .FirstOrDefaultAsync(l => l.Name.Contains(placeName), cancellationToken);

                if (landmark == null)
                {
                    return JsonSerializer.Serialize(new { message = $"لم يتم العثور على تفاصيل لمعلم سياحي باسم '{placeName}'" });
                }

                double? distance = null;
                string? duration = null;
                if (landmark.Coordinates != null)
                {
                    double startLat = 29.3084;
                    double startLon = 30.8428;
                    double endLat = (double)landmark.Coordinates.Latitude;
                    double endLon = (double)landmark.Coordinates.Longitude;
                    var dist = CalculateDistanceInKm(startLat, startLon, endLat, endLon);
                    distance = Math.Round(dist, 1);
                    duration = EstimateDuration(dist);
                }

                return JsonSerializer.Serialize(new
                {
                    landmark.Id,
                    landmark.Name,
                    landmark.Description,
                    Category = landmark.Category.ToString(),
                    landmark.Rating,
                    MainImageUrl = landmark.MainImageUrl != null ? landmark.MainImageUrl.Value : null,
                    Latitude = landmark.Coordinates != null ? (decimal?)landmark.Coordinates.Latitude : null,
                    Longitude = landmark.Coordinates != null ? (decimal?)landmark.Coordinates.Longitude : null,
                    DistanceFromFayoumCenterKm = distance,
                    EstimatedDurationFromFayoumCenter = duration
                });
            }

            return JsonSerializer.Serialize(new { error = $"Unknown tool '{name}'" });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private static double CalculateDistanceInKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return 6371 * c; // Earth radius in km
    }

    private static double ToRadians(double angle)
    {
        return Math.PI * angle / 180.0;
    }

    private static string EstimateDuration(double distanceKm)
    {
        var totalHours = distanceKm / 50.0; // Assume average speed of 50 km/h
        var totalMinutes = totalHours * 60;
        if (totalMinutes < 60)
        {
            return $"{Math.Round(totalMinutes)} mins";
        }
        else
        {
            var hours = (int)(totalMinutes / 60);
            var minutes = (int)(totalMinutes % 60);
            return minutes == 0 ? $"{hours} hour(s)" : $"{hours} hour(s) {minutes} mins";
        }
    }
}
