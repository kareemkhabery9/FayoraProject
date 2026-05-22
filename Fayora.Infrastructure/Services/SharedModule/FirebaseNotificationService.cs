using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Fayora.Application.Common.Interfaces.Services.SharedModule;

namespace Fayora.Infrastructure.Services.SharedModule;

public class FirebaseNotificationService : IFirebaseNotificationService
{
    private readonly ILogger<FirebaseNotificationService> _logger;
    private readonly bool _isFirebaseInitialized;

    public FirebaseNotificationService(IConfiguration configuration, ILogger<FirebaseNotificationService> logger)
    {
        _logger = logger;
        try
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var credentialsPath = configuration["Firebase:CredentialsPath"] ?? "firebase-service-account.json";
                var absolutePath = Path.Combine(AppContext.BaseDirectory, credentialsPath);

                if (File.Exists(absolutePath))
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(absolutePath)
                    });
                    _isFirebaseInitialized = true;
                    _logger.LogInformation("Firebase Admin SDK initialized successfully.");
                }
                else
                {
                    _logger.LogWarning("Firebase Credentials file not found at: {Path}. Push notifications will not be sent to real devices.", absolutePath);
                    _isFirebaseInitialized = false;
                }
            }
            else
            {
                _isFirebaseInitialized = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Firebase Admin SDK.");
            _isFirebaseInitialized = false;
        }
    }

    public async Task<(int SuccessCount, int FailureCount)> SendBroadcastAsync(
        string title,
        string body,
        string? imageUrl,
        List<string> targetTokens,
        CancellationToken cancellationToken)
    {
        if (!_isFirebaseInitialized || targetTokens == null || targetTokens.Count == 0)
        {
            _logger.LogWarning("FCM Broadcast skipped. Firebase initialized: {Init}, Token count: {Count}", _isFirebaseInitialized, targetTokens?.Count ?? 0);
            return (0, targetTokens?.Count ?? 0);
        }

        int successCount = 0;
        int failureCount = 0;

        // Firebase MulticastMessage allows up to 500 tokens per request
        const int batchSize = 500;
        for (int i = 0; i < targetTokens.Count; i += batchSize)
        {
            var count = Math.Min(batchSize, targetTokens.Count - i);
            var batch = targetTokens.GetRange(i, count);
            
            var notification = new Notification
            {
                Title = title,
                Body = body,
                ImageUrl = imageUrl
            };

            var message = new MulticastMessage
            {
                Tokens = batch,
                Notification = notification,
                Data = new Dictionary<string, string>
                {
                    { "title", title },
                    { "body", body }
                }
            };

            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message, cancellationToken);
                successCount += response.SuccessCount;
                failureCount += response.FailureCount;

                _logger.LogInformation("FCM Multicast sent: Success: {Success}, Failure: {Failure}", response.SuccessCount, response.FailureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending batch push notifications.");
                failureCount += batch.Count;
            }
        }

        return (successCount, failureCount);
    }

    public async Task<bool> SendToTokenAsync(
        string token,
        string title,
        string body,
        string? imageUrl,
        CancellationToken cancellationToken)
    {
        if (!_isFirebaseInitialized || string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("FCM Single Push skipped. Firebase initialized: {Init}, Token: {Token}", _isFirebaseInitialized, token);
            return false;
        }

        var message = new Message
        {
            Token = token,
            Notification = new Notification
            {
                Title = title,
                Body = body,
                ImageUrl = imageUrl
            },
            Data = new Dictionary<string, string>
            {
                { "title", title },
                { "body", body }
            }
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
            _logger.LogInformation("FCM Push sent successfully. Message ID: {Id}", response);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send FCM push to token: {Token}", token);
            return false;
        }
    }
}
