using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

/// <summary>
/// Converter for UnityEngine.Vector3
/// </summary>
public class Vector3Converter : JsonConverter<Vector3> {
    public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null)
            return Vector3.zero;

        // Load as JObject and read components
        var obj = Newtonsoft.Json.Linq.JObject.Load(reader);
        float x = obj["x"]?.Value<float>() ?? 0f;
        float y = obj["y"]?.Value<float>() ?? 0f;
        float z = obj["z"]?.Value<float>() ?? 0f;
        return new Vector3(x, y, z);
    }

    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer) {
        writer.WriteStartObject();
        writer.WritePropertyName("x"); writer.WriteValue(value.x);
        writer.WritePropertyName("y"); writer.WriteValue(value.y);
        writer.WritePropertyName("z"); writer.WriteValue(value.z);
        writer.WriteEndObject();
    }
}
