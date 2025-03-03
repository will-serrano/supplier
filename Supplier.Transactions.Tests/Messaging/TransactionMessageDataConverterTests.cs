using Supplier.Contracts.Transactions.Interfaces;
using Supplier.Contracts.Transactions.Requests;
using Supplier.Contracts.Transactions.Responses;
using Supplier.Transactions.Messaging;
using System.Text;
using System.Text.Json;

namespace Supplier.Transactions.Tests.Messaging
{
    public class TransactionMessageDataConverterTests
    {
        private readonly JsonSerializerOptions _options;
        private readonly TransactionMessageDataConverter _converter;

        public TransactionMessageDataConverterTests()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new TransactionMessageDataConverter() }
            };
            _converter = new TransactionMessageDataConverter();
        }

        [Fact]
        public void Read_ValidTransactionRequestMessageData_ReturnsCorrectType()
        {
            // Arrange
            var json = "{\"Type\":\"TransactionRequestMessageData\",\"Amount\":100.0,\"CustomerId\":\"d3b07384-d9a1-4d3b-8a1d-4d3b07384d9a\",\"TransactionId\":\"d3b07384-d9a1-4d3b-8a1d-4d3b07384d9a\"}";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

            // Act
            var result = _converter.Read(ref reader, typeof(ITransactionMessageData), _options);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TransactionRequestMessageData>(result);
        }

        [Fact]
        public void Read_ValidTransactionResponseMessageData_ReturnsCorrectType()
        {
            // Arrange
            var json = "{\"Type\":\"TransactionResponseMessageData\",\"Property\":\"Value\"}";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

            // Act
            var result = _converter.Read(ref reader, typeof(ITransactionMessageData), _options);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TransactionResponseMessageData>(result);
        }

        [Fact]
        public void Read_UnknownType_ThrowsJsonException()
        {
            // Arrange
            var json = "{\"Type\":\"UnknownType\",\"Property\":\"Value\"}";

            // Act & Assert
            Assert.Throws<JsonException>(() =>
            {
                var readerCopy = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                _converter.Read(ref readerCopy, typeof(ITransactionMessageData), _options);
            });
        }

        [Fact]
        public void Read_MissingTypeProperty_ThrowsJsonException()
        {
            // Arrange
            var json = "{\"Property\":\"Value\"}";

            // Act & Assert
            Assert.Throws<JsonException>(() =>
            {
                var readerCopy = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                _converter.Read(ref readerCopy, typeof(ITransactionMessageData), _options);
            });
        }

        [Fact]
        public void Write_ValidTransactionRequestMessageData_WritesCorrectJson()
        {
            // Arrange
            var value = new TransactionRequestMessageData { Amount = 100.0m, CustomerId = Guid.NewGuid(), TransactionId = Guid.NewGuid() };
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            // Act
            _converter.Write(writer, value, _options);
            writer.Flush();
            var json = Encoding.UTF8.GetString(stream.ToArray());

            // Assert
            Assert.Contains("\"Type\":\"TransactionRequestMessageData\"", json);
            Assert.Contains("\"Amount\":100.0", json);
            Assert.Contains("\"CustomerId\"", json);
            Assert.Contains("\"TransactionId\"", json);
        }

        [Fact]
        public void Write_ValidTransactionResponseMessageData_WritesCorrectJson()
        {
            // Arrange
            var value = new TransactionResponseMessageData { TransactionId = Guid.NewGuid(), IsSuccess = true, NewLimit = 5000.0m, Message = "Success" };
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            // Act
            _converter.Write(writer, value, _options);
            writer.Flush();
            var json = Encoding.UTF8.GetString(stream.ToArray());

            // Assert
            Assert.Contains("\"Type\":\"TransactionResponseMessageData\"", json);
            Assert.Contains("\"TransactionId\"", json);
            Assert.Contains("\"IsSuccess\":true", json);
            Assert.Contains("\"NewLimit\":5000.0", json);
            Assert.Contains("\"Message\":\"Success\"", json);
        }
    }
}
