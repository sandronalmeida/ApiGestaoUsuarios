using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiGestaoUsuarios.Infraestructure.Serialization
{
    /// <summary>
    /// Converte DateTime para o formato dd/MM/yyyy HH:mm:ss
    /// </summary>
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        private const string Format = "dd/MM/yyyy HH:mm:ss";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.ParseExact(reader.GetString()!, Format, null);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }
}
