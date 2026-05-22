using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Fayora.Application.Common.Interfaces.Services.ChatbotModule;
using Fayora.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Fayora.Infrastructure.Services.Chatbot;

public class GeminiChatbotService : IChatbotService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;

    public GeminiChatbotService(HttpClient httpClient, IOptions<GeminiSettings> settings)
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
                Text = "{\"text\": \"عذراً، لم يتم إعداد مفتاح API لخدمة الذكاء الاصطناعي بشكل صحيح.\", \"cards\": [], \"suggestions\": [\"إعادة المحاولة\"], \"map\": null}" 
            };
        }

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

        var contentsList = new List<object>();

        // Build history in the format Gemini expects
        if (history != null)
        {
            foreach (var message in history)
            {
                var geminiRole = message.Role.ToLower() switch
                {
                    "user" or "tourist" => "user",
                    _ => "model"
                };

                // Check if history content is valid JSON (if so, parse it to prevent double escaping in JSON structure,
                // but if it's plain text or unparsable, send it as plain text)
                object partsContent;
                if (geminiRole == "model")
                {
                    try
                    {
                        partsContent = new { text = message.Content };
                    }
                    catch
                    {
                        partsContent = new { text = message.Content };
                    }
                }
                else
                {
                    partsContent = new { text = message.Content };
                }

                contentsList.Add(new
                {
                    role = geminiRole,
                    parts = new[] { partsContent }
                });
            }
        }

        // Add current prompt
        contentsList.Add(new
        {
            role = "user",
            parts = new[]
            {
                new { text = userPrompt }
            }
        });

        // Add intermediate tool calls and responses in order to maintain thoughtSignature and call ID
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

                object? parsedContent = null;
                try
                {
                    parsedContent = JsonSerializer.Deserialize<object>(toolResponse.Content);
                }
                catch
                {
                    parsedContent = new { results = toolResponse.Content };
                }

                // Model turn requesting the function call
                var modelPart = new Dictionary<string, object>();
                var functionCallObj = new Dictionary<string, object>
                {
                    { "name", toolResponse.Name },
                    { "args", parsedArgs ?? new { } }
                };
                if (!string.IsNullOrEmpty(toolResponse.Id))
                {
                    functionCallObj.Add("id", toolResponse.Id);
                }
                modelPart.Add("functionCall", functionCallObj);
                if (!string.IsNullOrEmpty(toolResponse.ThoughtSignature))
                {
                    modelPart.Add("thoughtSignature", toolResponse.ThoughtSignature);
                }

                contentsList.Add(new
                {
                    role = "model",
                    parts = new object[] { modelPart }
                });

                // Function turn providing the output
                var functionResponseObj = new Dictionary<string, object>
                {
                    { "name", toolResponse.Name },
                    { "response", parsedContent ?? new { } }
                };
                if (!string.IsNullOrEmpty(toolResponse.Id))
                {
                    functionResponseObj.Add("id", toolResponse.Id);
                }

                var functionPart = new Dictionary<string, object>
                {
                    { "functionResponse", functionResponseObj }
                };

                contentsList.Add(new
                {
                    role = "function",
                    parts = new object[] { functionPart }
                });
            }
        }

        // System instructions detailing persona, grounding, context guidelines, and strict output structure
        var systemInstruction = new
        {
            parts = new[]
            {
                new { text = @"You are Fayora AI Agent, a premium, helpful travel and booking assistant for Fayora Travel platform. Help tourists discover tour packages, accommodations, check booking details, and guide information in Egypt (especially Fayoum). Always reply in the same language as the user (Arabic or English). Use a polite, friendly, and professional persona.

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
" }
            }
        };

        // Declare the 4 required RAG tools
        var tools = new[]
        {
            new
            {
                functionDeclarations = new object[]
                {
                    new
                    {
                        name = "search_accommodations",
                        description = "البحث عن الفنادق وشقق الإقامة المتاحة للمستخدمين بناءً على اسم المدينة في مصر (مثل الفيوم أو القاهرة) والحد الأقصى للسعر.",
                        parameters = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                city = new { type = "STRING", description = "اسم المدينة باللغة العربية أو الإنجليزية (مثال: الفيوم، القاهرة)" },
                                maxPrice = new { type = "NUMBER", description = "الحد الأقصى للسعر بالجنيه المصري لليلة الواحدة (اختياري)" }
                            },
                            required = new[] { "city" }
                        }
                    },
                    new
                    {
                        name = "search_tour_packages",
                        description = "البحث عن الرحلات والأنشطة والبرامج السياحية المتاحة للمستخدمين بناءً على اسم المدينة أو الوجهة السياحية والحد الأقصى للسعر.",
                        parameters = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                city = new { type = "STRING", description = "اسم المدينة أو الوجهة المراد البحث فيها (مثال: الفيوم، الجيزة)" },
                                maxPrice = new { type = "NUMBER", description = "الحد الأقصى لسعر الفرد بالجنيه المصري (اختياري)" }
                            },
                            required = new[] { "city" }
                        }
                    },
                    new
                    {
                        name = "check_user_bookings",
                        description = "الاستعلام عن الحجوزات النشطة والسابقة للمستخدم وجلب تفاصيل التواريخ، تكلفة الحجز، وحالته الحالية (مؤكد/معلق/ملغي).",
                        parameters = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                userId = new { type = "STRING", description = "المعرف الفريد للمستخدم (GUID) المطلوب للاستعلام عن حجوزاته." }
                            },
                            required = new[] { "userId" }
                        }
                    },
                    new
                    {
                        name = "get_place_details",
                        description = "استعلام جدول الوجهات والمعالم السياحية (Landmarks) لجلب الوصف التاريخي والجغرافي وإحداثيات المكان لمعلم محدد.",
                        parameters = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                placeName = new { type = "STRING", description = "اسم المعلم أو الوجهة السياحية باللغة العربية أو الإنجليزية (مثال: وادي الحيتان، بحيرة قارون)." }
                            },
                            required = new[] { "placeName" }
                        }
                    }
                }
            }
        };

        var requestBody = new
        {
            contents = contentsList,
            systemInstruction = systemInstruction,
            tools = tools,
            generationConfig = new
            {
                responseMimeType = "application/json"
            }
        };

        HttpResponseMessage? response = null;
        int maxRetries = 3;
        int delayMs = 2000;

        try
        {
            for (int retry = 0; retry <= maxRetries; retry++)
            {
                try
                {
                    response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && retry < maxRetries)
                    {
                        Console.WriteLine($"[Gemini Request] Rate limit hit (429). Retrying in {delayMs}ms (Attempt {retry + 1}/{maxRetries})...");
                        await Task.Delay(delayMs, cancellationToken);
                        delayMs *= 2;
                        continue;
                    }
                    break;
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests && retry < maxRetries)
                {
                    Console.WriteLine($"[Gemini Request] Rate limit hit (429 Exception). Retrying in {delayMs}ms (Attempt {retry + 1}/{maxRetries})...");
                    await Task.Delay(delayMs, cancellationToken);
                    delayMs *= 2;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Gemini Request Exception] {ex.Message}");
            throw;
        }

        if (response == null)
        {
            throw new HttpRequestException("No response received from Gemini API.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errContent = await response.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"[Gemini Error Response] Code: {response.StatusCode}, Body: {errContent}");
            throw new HttpRequestException($"Gemini API error: {response.StatusCode} - {errContent}", null, response.StatusCode);
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        Console.WriteLine($"[Gemini Success Response] Body: {responseContent}");
        
        try
        {
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;
            if (root.TryGetProperty("candidates", out var candidates) && 
                candidates.GetArrayLength() > 0 &&
                candidates[0].TryGetProperty("content", out var contentObj) &&
                contentObj.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0)
            {
                var firstPart = parts[0];
                if (firstPart.TryGetProperty("functionCall", out var functionCall))
                {
                    var name = functionCall.GetProperty("name").GetString() ?? string.Empty;
                    var args = functionCall.GetProperty("args").GetRawText();
                    
                    string? callId = null;
                    if (functionCall.TryGetProperty("id", out var idProp))
                    {
                        callId = idProp.GetString();
                    }

                    string? thoughtSig = null;
                    if (firstPart.TryGetProperty("thoughtSignature", out var thoughtSigProp))
                    {
                        thoughtSig = thoughtSigProp.GetString();
                    }

                    return new ChatbotResponse
                    {
                        ToolCalls = new List<ToolCallRequest>
                        {
                            new ToolCallRequest
                            {
                                Name = name,
                                ArgumentsJson = args,
                                Id = callId,
                                ThoughtSignature = thoughtSig
                            }
                        }
                    };
                }
                else if (firstPart.TryGetProperty("text", out var text))
                {
                    var textStr = text.GetString() ?? string.Empty;
                    return new ChatbotResponse
                    {
                        Text = textStr
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Gemini Parsing Exception] {ex.Message}");
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
