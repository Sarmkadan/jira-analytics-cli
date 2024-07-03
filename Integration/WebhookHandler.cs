// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Integration;

/// <summary>
/// Handles incoming Jira webhooks for real-time event processing.
/// Validates webhooks and routes to appropriate handlers.
/// </summary>
public class WebhookHandler
{
    private readonly ILogger<WebhookHandler> _logger;
    private readonly Dictionary<string, Func<WebhookEvent, Task>> _handlers = new();
    private readonly string? _webhookSecret;

    public WebhookHandler(ILogger<WebhookHandler> logger, string? webhookSecret = null)
    {
        _logger = logger;
        _webhookSecret = webhookSecret;
    }

    /// <summary>
    /// Processes incoming webhook payload from Jira.
    /// Validates signature and routes to registered handler.
    /// </summary>
    public async Task ProcessWebhookAsync(string payload, string? signature = null)
    {
        if (string.IsNullOrEmpty(payload))
            throw new ArgumentException("Payload cannot be empty", nameof(payload));

        // Validate webhook signature if secret is configured
        if (!string.IsNullOrEmpty(_webhookSecret) && !string.IsNullOrEmpty(signature))
        {
            if (!ValidateSignature(payload, signature, _webhookSecret))
            {
                _logger.LogWarning("Webhook signature validation failed");
                throw new InvalidOperationException("Webhook signature invalid");
            }
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            var webhookEvent = new WebhookEvent
            {
                EventType = root.TryGetProperty("webhookEvent", out var et) ? et.GetString() ?? string.Empty : string.Empty,
                Timestamp = DateTime.UtcNow,
                Payload = root.Clone(),
                RawPayload = payload
            };

            _logger.LogInformation("Processing webhook: {EventType}", webhookEvent.EventType);

            if (_handlers.TryGetValue(webhookEvent.EventType, out var handler))
            {
                await handler(webhookEvent);
            }
            else
            {
                _logger.LogDebug("No handler registered for event type: {EventType}", webhookEvent.EventType);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in webhook payload");
            throw new InvalidOperationException("Invalid webhook payload", ex);
        }
    }

    /// <summary>
    /// Registers handler for specific Jira event types.
    /// Multiple handlers can be registered for same event.
    /// </summary>
    public void RegisterHandler(string eventType, Func<WebhookEvent, Task> handler)
    {
        if (string.IsNullOrEmpty(eventType))
            throw new ArgumentException("Event type cannot be empty", nameof(eventType));

        _handlers[eventType] = handler ?? throw new ArgumentNullException(nameof(handler));
        _logger.LogDebug("Registered webhook handler for event: {EventType}", eventType);
    }

    /// <summary>
    /// Validates webhook signature using HMAC-SHA256.
    /// </summary>
    private bool ValidateSignature(string payload, string signature, string secret)
    {
        try
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
            var computed = Convert.ToBase64String(hash);

            return computed == signature;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating webhook signature");
            return false;
        }
    }

    public class WebhookEvent
    {
        public string EventType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public JsonElement Payload { get; set; }
        public string RawPayload { get; set; } = string.Empty;

        public T? GetPayloadValue<T>(string path)
        {
            var parts = path.Split('.');
            var current = Payload;

            foreach (var part in parts)
            {
                if (!current.TryGetProperty(part, out current))
                    return default;
            }

            return current.Deserialize<T>();
        }
    }
}
