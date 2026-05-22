using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Fayora.Application.Common.Interfaces.Services.ChatbotModule;
using Fayora.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Fayora.Infrastructure.Services.Chatbot;

public class OpenAIChatbotService : IChatbotService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAISettings _settings;

    public OpenAIChatbotService(HttpClient httpClient, IOptions<OpenAISettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<ChatbotResponse> GenerateResponseAsync(
        string userPrompt, 
        List<(string Role, string Content)> history, 
        List<ToolResponse>? toolResponses = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return new ChatbotResponse 
            { 
                Text = "{\"text\": \"عذراً، لم يتم إعداد مفتاح API لخدمة OpenAI بشكل صحيح.\", \"cards\": [], \"suggestions\": [\"إعادة المحاولة\"], \"map\": null}" 
            };
        }

        var url = "https://api.openai.com/v1/chat/completions";
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        var messages = new List<object>();

        // System instructions detailing persona and output structure
        var systemInstructionText = @"You are Fayora AI Agent, a premium, helpful travel and booking assistant for Fayora Travel platform. Help tourists discover tour packages, accommodations, check booking details, and guide information in Egypt (especially Fayoum). Always reply in the same language as the user (Arabic or English). Use a polite, friendly, and professional persona.

CRITICAL RULES:
1. Strict Context Boundary: You must ONLY answer questions within the scope of Egypt/Fayoum travel, tour packages, accommodations, bookings, and landmarks. If the user asks general, external, or out-of-scope questions (e.g. general science, coding, math, politics, history of other countries), do NOT answer using your general knowledge. Politely refuse to answer, saying that you are only able to assist with travel, bookings, and accommodations on the Fayora platform.
2. Grounding: When answering about accommodations, packages, bookings, or landmarks, you MUST query the database tools. If the tools return results, format the response using cards. Do not invent or hallucinate accommodations, packages, bookings, or landmarks that are not returned by the database.
3. Response Format: You MUST output your final response as a JSON object matching the following structure (do NOT wrap it in markdown block tags like ```json or ```, just return the raw JSON):
{
  ""text"": ""Your humanized, beautifully formatted text message to the user."",
  ""cards"": [
    {
      ""id"": ""guid-as-string"",
      ""type"": ""accommodation"" | ""package"" | ""booking"" | ""landmark"",
      ""title"": ""Unit, Package, Booking or Landmark Title"",
      ""description"": ""Short, clear description of the item"",
      ""price"": 1200.00,
      ""imageUrl"": ""https://..."",
      ""rating"": 4.8,
      ""detailUrl"": ""/api/Tourist/accommodation/{id}"" OR ""/api/Guide/packages/{id}"" OR ""/api/Booking/{id}"" OR ""/api/Location/{id}""
    }
  ],
  ""suggestions"": [
    ""Quick reply option 1"",
    ""Quick reply option 2""
  ],
  ""map"": {
    ""title"": ""Destination/Route Title"",
    ""duration"": ""15 mins"",
    ""distance"": ""8.5 km"",
    ""startLatitude"": 29.3084,
    ""startLongitude"": 30.8428,
    ""endLatitude"": 29.123,
    ""endLongitude"": 30.456,
    ""routeDescription"": ""Brief description of the route or directions""
  }
}

Guidelines for JSON Fields:
- ""text"": The main response text. It should be friendly, humanized, and formatted neatly (e.g., using newlines, bullets, or emojis).
- ""cards"": An array of cards to display. Map accommodations, packages, landmarks, or bookings here.
  - ""id"": The Guid or integer ID of the entity.
  - ""type"": Set to ""accommodation"" for housing units, ""package"" for tour packages, ""booking"" for active user bookings, and ""landmark"" for tourist places/destinations.
  - ""price"": For accommodations, use the price per night. For packages, use the adult price. For bookings, use total price. For landmarks, price is not applicable (use 0 or null).
  - ""imageUrl"": Use the MainImageUrl from the tool response (if available).
  - ""detailUrl"": 
    - For accommodation: ""/api/Tourist/accommodation/{id}""
    - For package: ""/api/Guide/packages/{id}""
    - For booking: ""/api/Booking/{id}""
    - For landmark: ""/api/Location/{id}""
- ""suggestions"": An array of short strings representing quick-reply options/buttons (maximum 4). Use these to guide the user on what to ask next (e.g., ""Plan my day"", ""Best places for families"", ""Nature and relax places"", ""Trips under 1000 EGP"").
- ""map"": Provide this object ONLY when the user asks for directions, a route, or the location of a place.
  - ""title"": Name of the place/destination.
  - ""duration"": Estimated duration (e.g. ""15 mins"" or ""1 hour"").
  - ""distance"": Estimated distance (e.g. ""8.5 km"").
  - ""startLatitude"" & ""startLongitude"": Coordinates of starting point (if known, otherwise default to Fayoum city center: 29.3084, 30.8428).
  - ""endLatitude"" & ""endLongitude"": Coordinates of the destination.
  - ""routeDescription"": Short description of the route.
  If no map is requested or needed, set ""map"" to null.
- If there are no cards, set ""cards"": [].
- If there are no suggestions, set ""suggestions"": [].
";

        messages.Add(new { role = "system", content = systemInstructionText });

        // Add history
        if (history != null)
        {
            foreach (var msg in history)
            {
                messages.Add(new
                {
                    role = msg.Role.ToLower() switch
                    {
                        "user" or "tourist" => "user",
                        _ => "assistant"
                    },
                    content = msg.Content
                });
            }
        }

        // Add current user prompt
        messages.Add(new { role = "user", content = userPrompt });

        // In OpenAI, function responses are appended as role: assistant (with tool_calls) and role: tool
        if (toolResponses != null)
        {
            foreach (var toolResponse in toolResponses)
            {
                object? parsedArgs = null;
                try
                {
                    parsedArgs = JsonSerializer.Deserialize<object>(toolResponse.ArgumentsJson);
                }
                catch
                {
                    parsedArgs = new { };
                }

                // OpenAI expects the assistant message with tool_calls first
                var callId = string.IsNullOrEmpty(toolResponse.Id) ? $"call_{Guid.NewGuid():N}" : toolResponse.Id;
                
                messages.Add(new
                {
                    role = "assistant",
                    tool_calls = new[]
                    {
                        new
                        {
                            id = callId,
                            type = "function",
                            function = new
                            {
                                name = toolResponse.Name,
                                arguments = toolResponse.ArgumentsJson
                            }
                        }
                    }
                });

                // Then the tool response message
                messages.Add(new
                {
                    role = "tool",
                    tool_call_id = callId,
                    name = toolResponse.Name,
                    content = toolResponse.Content
                });
            }
        }

        var tools = new object[]
        {
            new
            {
                type = "function",
                function = new
                {
                    name = "search_accommodations",
                    description = "البحث عن الفنادق وشقق الإقامة المتاحة للمستخدمين بناءً على اسم المدينة في مصر (مثل الفيوم أو القاهرة) والحد الأقصى للسعر.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            city = new { type = "string", description = "اسم المدينة باللغة العربية أو الإنجليزية (مثال: الفيوم، القاهرة)" },
                            maxPrice = new { type = "number", description = "الحد الأقصى للسعر بالجنيه المصري لليلة الواحدة (اختياري)" }
                        },
                        required = new[] { "city" }
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "search_tour_packages",
                    description = "البحث عن الرحلات والأنشطة والبرامج السياحية المتاحة للمستخدمين بناءً على اسم المدينة أو الوجهة السياحية والحد الأقصى للسعر.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            city = new { type = "string", description = "اسم المدينة أو الوجهة المراد البحث فيها (مثال: الفيوم، الجيزة)" },
                            maxPrice = new { type = "number", description = "الحد الأقصى لسعر الفرد بالجنيه المصري (اختياري)" }
                        },
                        required = new[] { "city" }
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "check_user_bookings",
                    description = "الاستعلام عن الحجوزات النشطة والسابقة للمستخدم وجلب تفاصيل التواريخ، تكلفة الحجز، وحالته الحالية (مؤكد/معلق/ملغي).",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            userId = new { type = "string", description = "المعرف الفريد للمستخدم (GUID) المطلوب للاستعلام عن حجوزاته." }
                        },
                        required = new[] { "userId" }
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "get_place_details",
                    description = "استعلام جدول الوجهات والمعالم السياحية (Landmarks) لجلب الوصف التاريخي والجغرافي وإحداثيات المكان لمعلم محدد.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            placeName = new { type = "string", description = "اسم المعلم أو الوجهة السياحية باللغة العربية أو الإنجليزية (مثال: وادي الحيتان، بحيرة قارون)." }
                        },
                        required = new[] { "placeName" }
                    }
                }
            }
        };

        var requestBody = new
        {
            model = _settings.Model,
            messages = messages,
            tools = tools,
            response_format = new { type = "json_object" }
        };

        requestMessage.Content = JsonContent.Create(requestBody);

        HttpResponseMessage? response = null;
        int maxRetries = 3;
        int delayMs = 2000;

        try
        {
            for (int retry = 0; retry <= maxRetries; retry++)
            {
                var cloneRequest = new HttpRequestMessage(HttpMethod.Post, url);
                cloneRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
                cloneRequest.Content = JsonContent.Create(requestBody);

                response = await _httpClient.SendAsync(cloneRequest, cancellationToken);
                
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && retry < maxRetries)
                {
                    Console.WriteLine($"[OpenAI Request] Rate limit hit (429). Retrying in {delayMs}ms (Attempt {retry + 1}/{maxRetries})...");
                    await Task.Delay(delayMs, cancellationToken);
                    delayMs *= 2;
                    continue;
                }
                break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OpenAI Request Exception] {ex.Message}");
            return GetFallbackResponse("عذراً، حدث خطأ في الاتصال بخدمة OpenAI. يرجى المحاولة مجدداً.");
        }

        if (response == null || !response.IsSuccessStatusCode)
        {
            var errContent = response != null ? await response.Content.ReadAsStringAsync(cancellationToken) : "Unknown";
            Console.WriteLine($"[OpenAI Error Response] Code: {response?.StatusCode}, Body: {errContent}");
            return GetFallbackResponse("عذراً، لم نتمكن من الحصول على رد من خدمة OpenAI.");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        try
        {
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;
            var choice = root.GetProperty("choices")[0];
            var msgObj = choice.GetProperty("message");

            if (msgObj.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.GetArrayLength() > 0)
            {
                var firstTool = toolCalls[0];
                var name = firstTool.GetProperty("function").GetProperty("name").GetString() ?? string.Empty;
                var args = firstTool.GetProperty("function").GetProperty("arguments").GetString() ?? string.Empty;
                var callId = firstTool.GetProperty("id").GetString();

                return new ChatbotResponse
                {
                    ToolCalls = new List<ToolCallRequest>
                    {
                        new ToolCallRequest
                        {
                            Name = name,
                            ArgumentsJson = args,
                            Id = callId
                        }
                    }
                };
            }
            else if (msgObj.TryGetProperty("content", out var contentProp))
            {
                return new ChatbotResponse
                {
                    Text = contentProp.GetString() ?? string.Empty
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OpenAI Parsing Exception] {ex.Message}");
        }

        return GetFallbackResponse("عذراً، لم أستطع معالجة طلبك في الوقت الحالي.");
    }

    private ChatbotResponse GetFallbackResponse(string message)
    {
        var json = JsonSerializer.Serialize(new
        {
            text = message,
            cards = new object[] { },
            suggestions = new[] { "إعادة المحاولة" },
            map = (object?)null
        });

        return new ChatbotResponse { Text = json };
    }
}
