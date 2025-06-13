using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jobbr.Server.WebAPI.Model;
using Microsoft.Extensions.Logging;

namespace Jobbr.Server.WebAPI.Infrastructure
{
    public class JobTriggerDtoConverter : JsonConverter<JobTriggerDtoBase>
    {
        private readonly ILogger<JobTriggerDtoConverter> _logger;

        public JobTriggerDtoConverter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<JobTriggerDtoConverter>();
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(JobTriggerDtoBase).IsAssignableFrom(typeToConvert);
        }

        public override JobTriggerDtoBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var rootElement = jsonDoc.RootElement;
            var rawJson = rootElement.GetRawText();

            // Look for the TriggerType property
            if (!rootElement.TryGetProperty(nameof(JobTriggerDtoBase.TriggerType).ToCamelCase(), out var triggerTypeProp))
            {
                throw new JsonException($"Missing {nameof(JobTriggerDtoBase.TriggerType).ToCamelCase()} property in the JSON");
            }

            var triggerType = triggerTypeProp.GetString();
            _logger.LogDebug("Deserializing trigger of type: {TriggerType}", triggerType);

            return triggerType switch
            {
                RecurringTriggerDto.Type => JsonSerializer.Deserialize<RecurringTriggerDto>(rawJson, CopyJsonSerializerOptions(options)),
                ScheduledTriggerDto.Type => JsonSerializer.Deserialize<ScheduledTriggerDto>(rawJson, CopyJsonSerializerOptions(options)),
                InstantTriggerDto.Type => JsonSerializer.Deserialize<InstantTriggerDto>(rawJson, CopyJsonSerializerOptions(options)),
                _ => throw new JsonException($"Unknown trigger type: {triggerType}")
            };
        }

        public override void Write(Utf8JsonWriter writer, JobTriggerDtoBase value, JsonSerializerOptions options)
        {
            // Serialize the object as-is, the TriggerType property will be included
            JsonSerializer.Serialize(writer, value, value.GetType(), CopyJsonSerializerOptions(options));
        }

        private static JsonSerializerOptions CopyJsonSerializerOptions(JsonSerializerOptions options)
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive,
                PropertyNamingPolicy = options.PropertyNamingPolicy,
                DefaultIgnoreCondition = options.DefaultIgnoreCondition,
            };
        }
    }
}