using System.Text.Json;namespace FxPu.UtilsLib{
    public static class JsonUtils    {        public static readonly JsonSerializerOptions FromJsonSerializerOptions = new JsonSerializerOptions        {            AllowTrailingCommas = true,            PropertyNameCaseInsensitive = true,            ReadCommentHandling = JsonCommentHandling.Skip        };        public static readonly JsonSerializerOptions ToJsonSerializerOptions = new JsonSerializerOptions        {            WriteIndented = true        };

        public static TValue? FromJson<TValue>(string? json)
        {            if (string.IsNullOrEmpty(json))            {                return default;            }            return JsonSerializer.Deserialize<TValue>(json, FromJsonSerializerOptions);        }


        public static string? ToJson<TValue>(TValue? value)
        {            if (value == null)            {                return null;            }            return JsonSerializer.Serialize(value, ToJsonSerializerOptions);        }    }}