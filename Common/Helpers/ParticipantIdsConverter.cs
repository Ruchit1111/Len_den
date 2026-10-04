using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tricount.Common.Helpers;

/// <summary>
/// Flexible JSON converter for participant IDs.
/// Supports arrays of integers [1, 2], arrays of strings ["1", "2"],
/// and arrays of objects [{"userId": 1}, {"userId": 2}] or [{"id": 1, "userId": 1, "amountOwed": 50}].
/// </summary>
public class ParticipantIdsConverter : JsonConverter<List<int>>
{
    public override List<int> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return new List<int>();
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array for participants.");
        }

        var list = new List<int>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                list.Add(reader.GetInt32());
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                if (int.TryParse(reader.GetString(), out int id))
                {
                    list.Add(id);
                }
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                int? userId = null;
                int depth = 1;
                while (reader.Read() && depth > 0)
                {
                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        depth++;
                    }
                    else if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        depth--;
                    }
                    else if (reader.TokenType == JsonTokenType.PropertyName && depth == 1)
                    {
                        string propName = reader.GetString() ?? string.Empty;
                        reader.Read();
                        if (string.Equals(propName, "userId", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(propName, "id", StringComparison.OrdinalIgnoreCase))
                        {
                            if (reader.TokenType == JsonTokenType.Number)
                            {
                                userId = reader.GetInt32();
                            }
                            else if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out int parsedId))
                            {
                                userId = parsedId;
                            }
                        }
                    }
                }

                if (userId.HasValue && userId.Value > 0)
                {
                    list.Add(userId.Value);
                }
            }
        }

        return list;
    }

    public override void Write(Utf8JsonWriter writer, List<int> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var id in value)
        {
            writer.WriteNumberValue(id);
        }
        writer.WriteEndArray();
    }
}
