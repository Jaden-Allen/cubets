using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

/// <summary>
/// Converter for UnityEngine.Vector2
/// </summary>
public class Vector2Converter : JsonConverter<Vector2> {
    public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null)
            return Vector2.zero;

        var obj = Newtonsoft.Json.Linq.JObject.Load(reader);
        float x = obj["x"]?.Value<float>() ?? 0f;
        float y = obj["y"]?.Value<float>() ?? 0f;
        return new Vector2(x, y);
    }

    public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer) {
        writer.WriteStartObject();
        writer.WritePropertyName("x"); writer.WriteValue(value.x);
        writer.WritePropertyName("y"); writer.WriteValue(value.y);
        writer.WriteEndObject();
    }
}