using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WYD.Common.Converter;

/// <summary>
/// DateTime 自定义格式化转换器（替代 Newtonsoft DateFormatString）
/// </summary>
public class DateTimeFormatConverter : JsonConverter<DateTime>
{
    private readonly string _format;

    public DateTimeFormatConverter(string format) => _format = format;

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTime.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(_format));
}
