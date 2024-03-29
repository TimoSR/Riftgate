using System.Text;
using CodingPatterns.InfrastructureLayer.GooglePubSub;
using CodingPatterns.InfrastructureLayer.IntegrationEvents;
using Infrastructure.Persistence._Interfaces;
using Infrastructure.UtilityServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Infrastructure.API;

public abstract class IntegrationEventListeners : ControllerBase
{
    private readonly IIntegrationEventHandler _integrationEventHandler;
    private readonly IProtobufSerializer _protobufSerializer;
    private readonly ILogger _logger;

    protected IntegrationEventListeners(IIntegrationEventHandler integrationEventHandler, IProtobufSerializer protobufSerializer, ILogger logger)
    {
        _integrationEventHandler = integrationEventHandler;
        _protobufSerializer = protobufSerializer;
        _logger = logger;
    }
    
    // This needs to be moved to somewhere else, and used as a method.
    // There where something about returning a string
    // Also to adhere to single responsibility principle it should not be in the integrationEventHandler

    protected async Task<TEventData?> OnEvent<TEventData>() where TEventData : class, ISubscribeEvent
    {
        using var reader = new StreamReader(Request.Body);
        var receivedEventJson = await reader.ReadToEndAsync();
        var pubSubEvent = JsonConvert.DeserializeObject<PubSubEvent>(receivedEventJson);
        _logger.LogInformation($"Received: {pubSubEvent.Message.Attributes.EventType} at [{DateTime.UtcNow} (UTC)]");
        byte[] data = Convert.FromBase64String(pubSubEvent.Message.Data);
        string decodedString = Encoding.UTF8.GetString(data);
        return _protobufSerializer.Deserialize<TEventData>(decodedString);
    }
}