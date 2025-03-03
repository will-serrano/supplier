using Supplier.Contracts.Transactions.Interfaces;
using Supplier.Contracts.Transactions.Requests;
using Supplier.Contracts.Transactions.Responses;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Supplier.Contracts.Transactions
{
    /// <summary>
    /// Converts JSON data to and from <see cref="ITransactionMessageData"/> objects.
    /// </summary>
    public class TransactionMessageDataConverter : JsonConverter<ITransactionMessageData>
    {
        private static readonly Dictionary<string, Type> TypeMap = new()
            {
                { nameof(TransactionRequestMessageData), typeof(TransactionRequestMessageData) },
                { nameof(TransactionResponseMessageData), typeof(TransactionResponseMessageData) }
            };

        /// <summary>
        /// Reads and converts the JSON to <see cref="ITransactionMessageData"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>The converted <see cref="ITransactionMessageData"/> object.</returns>
        /// <exception cref="JsonException">Thrown when the 'Type' property is not found or the type is unknown.</exception>
        public override ITransactionMessageData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                if (!doc.RootElement.TryGetProperty("Type", out JsonElement typeElement))
                {
                    throw new JsonException("Missing 'Type' property.");
                }

                string? typeName = typeElement.GetString();
                if (typeName is null || !TypeMap.TryGetValue(typeName, out Type? type))
                {
                    throw new JsonException($"Unknown or missing type: {typeName}");
                }

                var result = JsonSerializer.Deserialize(doc.RootElement.GetRawText(), type, options);
                if (result is null)
                {
                    throw new JsonException("Deserialization returned null.");
                }

                return (ITransactionMessageData)result;
            }
        }

        /// <summary>
        /// Writes a specified <see cref="ITransactionMessageData"/> object as JSON.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value to write.</param>
        /// <param="options">The serializer options.</param>
        public override void Write(Utf8JsonWriter writer, ITransactionMessageData value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            string typeName = value.GetType().Name;
            writer.WriteString("Type", typeName);

            foreach (var property in value.GetType().GetProperties())
            {
                var propertyValue = property.GetValue(value);
                writer.WritePropertyName(property.Name);
                JsonSerializer.Serialize(writer, propertyValue, propertyValue?.GetType() ?? typeof(object), options);
            }

            writer.WriteEndObject();
        }
    }
}
