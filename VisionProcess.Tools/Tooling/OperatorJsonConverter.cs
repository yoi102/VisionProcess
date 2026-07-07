using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VisionProcess.Abstractions.ToolBase;

namespace VisionProcess.Tools.Tooling
{
    /// <summary>
    /// <see cref="IOperator"/> 的多态 JSON 转换器。
    /// 通过显式的 "$kind" 字段替代 <see cref="TypeNameHandling.Auto"/> 的反射序列化。
    /// </summary>
    public class OperatorJsonConverter : JsonConverter<IOperator>
    {
        public override void WriteJson(JsonWriter writer, IOperator? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            var kind = ToolRegistry.GetToolName(value.GetType());

            writer.WriteStartObject();
            writer.WritePropertyName("$kind");
            writer.WriteValue(kind);
            writer.WritePropertyName("value");

            var innerSerializer = CreateInnerSerializer(serializer);
            innerSerializer.Serialize(writer, value);

            writer.WriteEndObject();
        }

        public override IOperator? ReadJson(JsonReader reader, Type objectType, IOperator? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var jo = JObject.Load(reader);
            var kind = (string?)jo["$kind"];
            if (kind is null)
                return null;

            var type = ToolRegistry.GetToolType(kind);
            if (type is null)
                throw new JsonSerializationException($"Unknown operator kind '{kind}'.");

            var valueToken = jo["value"];
            if (valueToken is null)
                return null;

            var innerSerializer = CreateInnerSerializer(serializer);
            using var valueReader = valueToken.CreateReader();
            return (IOperator?)innerSerializer.Deserialize(valueReader, type);
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;

        private static JsonSerializer CreateInnerSerializer(JsonSerializer source)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = source.ContractResolver,
                Converters = source.Converters.Where(c => c is not OperatorJsonConverter).ToList(),
                NullValueHandling = source.NullValueHandling,
                DateFormatHandling = source.DateFormatHandling,
                DateParseHandling = source.DateParseHandling,
                Formatting = source.Formatting,
                // 显式关闭反射式类型名写入
                TypeNameHandling = TypeNameHandling.None,
            };
            return JsonSerializer.Create(settings);
        }
    }
}
