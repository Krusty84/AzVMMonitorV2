/*
AzBillingRESTHelper.cs
06.10.2021 22:23:08
Alexey Sedoykin
*/

namespace AzVMMonitorCostsPerVM
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Globalization;

    /// <summary>
    /// Defines the <see cref="CostDataPerVm" />.
    /// </summary>
    public partial class CostDataPerVm
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        [JsonProperty("location")]
        public object Location { get; set; }

        /// <summary>
        /// Gets or sets the Sku.
        /// </summary>
        [JsonProperty("sku")]
        public object Sku { get; set; }

        /// <summary>
        /// Gets or sets the ETag.
        /// </summary>
        [JsonProperty("eTag")]
        public object ETag { get; set; }

        /// <summary>
        /// Gets or sets the Properties.
        /// </summary>
        [JsonProperty("properties")]
        public Properties Properties { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="Properties" />.
    /// </summary>
    public partial class Properties
    {
        /// <summary>
        /// Gets or sets the NextLink.
        /// </summary>
        [JsonProperty("nextLink")]
        public object NextLink { get; set; }

        /// <summary>
        /// Gets or sets the Columns.
        /// </summary>
        [JsonProperty("columns")]
        public Column[] Columns { get; set; }

        /// <summary>
        /// Gets or sets the Rows.
        /// </summary>
        [JsonProperty("rows")]
        public Row[][] Rows { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="Column" />.
    /// </summary>
    public partial class Column
    {
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="Row" />.
    /// </summary>
    public partial struct Row
    {
        /// <summary>
        /// Defines the AnythingArray.
        /// </summary>
        public object[] AnythingArray;

        /// <summary>
        /// Defines the Double.
        /// </summary>
        public double? Double;

        /// <summary>
        /// Defines the String.
        /// </summary>
        public string String;


        public static implicit operator Row(object[] AnythingArray) => new Row { AnythingArray = AnythingArray };

        public static implicit operator Row(double Double) => new Row { Double = Double };

        public static implicit operator Row(string String) => new Row { String = String };
    }

    /// <summary>
    /// Defines the <see cref="CostDataPerVm" />.
    /// </summary>
    public partial class CostDataPerVm
    {
        /// <summary>
        /// The FromJson.
        /// </summary>
        /// <param name="json">The json<see cref="string"/>.</param>
        /// <returns>The <see cref="CostDataPerVm"/>.</returns>
        public static CostDataPerVm FromJson(string json) => JsonConvert.DeserializeObject<CostDataPerVm>(json, AzVMMonitorCostsPerVM.Converter.Settings);
    }

    /// <summary>
    /// Defines the <see cref="Serialize" />.
    /// </summary>
    public static class Serialize
    {
        /// <summary>
        /// The ToJson.
        /// </summary>
        /// <param name="self">The self<see cref="CostDataPerVm"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string ToJson(this CostDataPerVm self) => JsonConvert.SerializeObject(self, AzVMMonitorCostsPerVM.Converter.Settings);
    }

    /// <summary>
    /// Defines the <see cref="Converter" />.
    /// </summary>
    internal static class Converter
    {
        /// <summary>
        /// Defines the Settings.
        /// </summary>
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                RowConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    /// <summary>
    /// Defines the <see cref="RowConverter" />.
    /// </summary>
    internal class RowConverter : JsonConverter
    {
        /// <summary>
        /// The CanConvert.
        /// </summary>
        /// <param name="t">The t<see cref="Type"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanConvert(Type t) => t == typeof(Row) || t == typeof(Row?);

        /// <summary>
        /// The ReadJson.
        /// </summary>
        /// <param name="reader">The reader<see cref="JsonReader"/>.</param>
        /// <param name="t">The t<see cref="Type"/>.</param>
        /// <param name="existingValue">The existingValue<see cref="object"/>.</param>
        /// <param name="serializer">The serializer<see cref="JsonSerializer"/>.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    var doubleValue = serializer.Deserialize<double>(reader);
                    return new Row { Double = doubleValue };

                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new Row { String = stringValue };

                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<object[]>(reader);
                    return new Row { AnythingArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type Row");
        }

        /// <summary>
        /// The WriteJson.
        /// </summary>
        /// <param name="writer">The writer<see cref="JsonWriter"/>.</param>
        /// <param name="untypedValue">The untypedValue<see cref="object"/>.</param>
        /// <param name="serializer">The serializer<see cref="JsonSerializer"/>.</param>
        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Row)untypedValue;
            if (value.Double != null)
            {
                serializer.Serialize(writer, value.Double.Value);
                return;
            }
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            if (value.AnythingArray != null)
            {
                serializer.Serialize(writer, value.AnythingArray);
                return;
            }
            throw new Exception("Cannot marshal type Row");
        }

        /// <summary>
        /// Defines the Singleton.
        /// </summary>
        public static readonly RowConverter Singleton = new RowConverter();
    }
}

namespace AzVMMonitorCostsPerVMDisk
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Globalization;

    /// <summary>
    /// Defines the <see cref="CostDataPerVmDisk" />.
    /// </summary>
    public partial class CostDataPerVmDisk
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        [JsonProperty("location")]
        public object Location { get; set; }

        /// <summary>
        /// Gets or sets the Sku.
        /// </summary>
        [JsonProperty("sku")]
        public object Sku { get; set; }

        /// <summary>
        /// Gets or sets the ETag.
        /// </summary>
        [JsonProperty("eTag")]
        public object ETag { get; set; }

        /// <summary>
        /// Gets or sets the Properties.
        /// </summary>
        [JsonProperty("properties")]
        public Properties Properties { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="Properties" />.
    /// </summary>
    public partial class Properties
    {
        /// <summary>
        /// Gets or sets the NextLink.
        /// </summary>
        [JsonProperty("nextLink")]
        public object NextLink { get; set; }

        /// <summary>
        /// Gets or sets the Columns.
        /// </summary>
        [JsonProperty("columns")]
        public Column[] Columns { get; set; }

        /// <summary>
        /// Gets or sets the Rows.
        /// </summary>
        [JsonProperty("rows")]
        public Row[][] Rows { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="Column" />.
    /// </summary>
    public partial class Column
    {
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="Row" />.
    /// </summary>
    public partial struct Row
    {
        /// <summary>
        /// Defines the Double.
        /// </summary>
        public double? Double;

        /// <summary>
        /// Defines the String.
        /// </summary>
        public string String;

        /// <summary>
        /// Defines the StringArray.
        /// </summary>
        public string[] StringArray;


        public static implicit operator Row(double Double) => new Row { Double = Double };

        public static implicit operator Row(string String) => new Row { String = String };

        public static implicit operator Row(string[] StringArray) => new Row { StringArray = StringArray };
    }

    /// <summary>
    /// Defines the <see cref="CostDataPerVmDisk" />.
    /// </summary>
    public partial class CostDataPerVmDisk
    {
        /// <summary>
        /// The FromJson.
        /// </summary>
        /// <param name="json">The json<see cref="string"/>.</param>
        /// <returns>The <see cref="CostDataPerVmDisk"/>.</returns>
        public static CostDataPerVmDisk FromJson(string json) => JsonConvert.DeserializeObject<CostDataPerVmDisk>(json, AzVMMonitorCostsPerVMDisk.Converter.Settings);
    }

    /// <summary>
    /// Defines the <see cref="Serialize" />.
    /// </summary>
    public static class Serialize
    {
        /// <summary>
        /// The ToJson.
        /// </summary>
        /// <param name="self">The self<see cref="CostDataPerVmDisk"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string ToJson(this CostDataPerVmDisk self) => JsonConvert.SerializeObject(self, AzVMMonitorCostsPerVMDisk.Converter.Settings);
    }

    /// <summary>
    /// Defines the <see cref="Converter" />.
    /// </summary>
    internal static class Converter
    {
        /// <summary>
        /// Defines the Settings.
        /// </summary>
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                RowConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    /// <summary>
    /// Defines the <see cref="RowConverter" />.
    /// </summary>
    internal class RowConverter : JsonConverter
    {
        /// <summary>
        /// The CanConvert.
        /// </summary>
        /// <param name="t">The t<see cref="Type"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanConvert(Type t) => t == typeof(Row) || t == typeof(Row?);

        /// <summary>
        /// The ReadJson.
        /// </summary>
        /// <param name="reader">The reader<see cref="JsonReader"/>.</param>
        /// <param name="t">The t<see cref="Type"/>.</param>
        /// <param name="existingValue">The existingValue<see cref="object"/>.</param>
        /// <param name="serializer">The serializer<see cref="JsonSerializer"/>.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    var doubleValue = serializer.Deserialize<double>(reader);
                    return new Row { Double = doubleValue };

                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new Row { String = stringValue };

                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<string[]>(reader);
                    return new Row { StringArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type Row");
        }

        /// <summary>
        /// The WriteJson.
        /// </summary>
        /// <param name="writer">The writer<see cref="JsonWriter"/>.</param>
        /// <param name="untypedValue">The untypedValue<see cref="object"/>.</param>
        /// <param name="serializer">The serializer<see cref="JsonSerializer"/>.</param>
        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Row)untypedValue;
            if (value.Double != null)
            {
                serializer.Serialize(writer, value.Double.Value);
                return;
            }
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            if (value.StringArray != null)
            {
                serializer.Serialize(writer, value.StringArray);
                return;
            }
            throw new Exception("Cannot marshal type Row");
        }

        /// <summary>
        /// Defines the Singleton.
        /// </summary>
        public static readonly RowConverter Singleton = new RowConverter();
    }
}

namespace AzVMMonitorCostsPerVMNetwork
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Globalization;

    /// <summary>
    /// Defines the <see cref="CostDataPerVmNetwork" />.
    /// </summary>
    public partial class CostDataPerVmNetwork
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        [JsonProperty("location")]
        public object Location { get; set; }

        /// <summary>
        /// Gets or sets the Sku.
        /// </summary>
        [JsonProperty("sku")]
        public object Sku { get; set; }

        /// <summary>
        /// Gets or sets the ETag.
        /// </summary>
        [JsonProperty("eTag")]
        public object ETag { get; set; }

        /// <summary>
        /// Gets or sets the Properties.
        /// </summary>
        [JsonProperty("properties")]
        public Properties Properties { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="Properties" />.
    /// </summary>
    public partial class Properties
    {
        /// <summary>
        /// Gets or sets the NextLink.
        /// </summary>
        [JsonProperty("nextLink")]
        public object NextLink { get; set; }

        /// <summary>
        /// Gets or sets the Columns.
        /// </summary>
        [JsonProperty("columns")]
        public Column[] Columns { get; set; }

        /// <summary>
        /// Gets or sets the Rows.
        /// </summary>
        [JsonProperty("rows")]
        public Row[][] Rows { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="Column" />.
    /// </summary>
    public partial class Column
    {
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="Row" />.
    /// </summary>
    public partial struct Row
    {
        /// <summary>
        /// Defines the AnythingArray.
        /// </summary>
        public object[] AnythingArray;

        /// <summary>
        /// Defines the Double.
        /// </summary>
        public double? Double;

        /// <summary>
        /// Defines the String.
        /// </summary>
        public string String;


        public static implicit operator Row(object[] AnythingArray) => new Row { AnythingArray = AnythingArray };

        public static implicit operator Row(double Double) => new Row { Double = Double };

        public static implicit operator Row(string String) => new Row { String = String };
    }

    /// <summary>
    /// Defines the <see cref="CostDataPerVmNetwork" />.
    /// </summary>
    public partial class CostDataPerVmNetwork
    {
        /// <summary>
        /// The FromJson.
        /// </summary>
        /// <param name="json">The json<see cref="string"/>.</param>
        /// <returns>The <see cref="CostDataPerVmNetwork"/>.</returns>
        public static CostDataPerVmNetwork FromJson(string json) => JsonConvert.DeserializeObject<CostDataPerVmNetwork>(json, AzVMMonitorCostsPerVMNetwork.Converter.Settings);
    }

    /// <summary>
    /// Defines the <see cref="Serialize" />.
    /// </summary>
    public static class Serialize
    {
        /// <summary>
        /// The ToJson.
        /// </summary>
        /// <param name="self">The self<see cref="CostDataPerVmNetwork"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string ToJson(this CostDataPerVmNetwork self) => JsonConvert.SerializeObject(self, AzVMMonitorCostsPerVMNetwork.Converter.Settings);
    }

    /// <summary>
    /// Defines the <see cref="Converter" />.
    /// </summary>
    internal static class Converter
    {
        /// <summary>
        /// Defines the Settings.
        /// </summary>
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                RowConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    /// <summary>
    /// Defines the <see cref="RowConverter" />.
    /// </summary>
    internal class RowConverter : JsonConverter
    {
        /// <summary>
        /// The CanConvert.
        /// </summary>
        /// <param name="t">The t<see cref="Type"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanConvert(Type t) => t == typeof(Row) || t == typeof(Row?);

        /// <summary>
        /// The ReadJson.
        /// </summary>
        /// <param name="reader">The reader<see cref="JsonReader"/>.</param>
        /// <param name="t">The t<see cref="Type"/>.</param>
        /// <param name="existingValue">The existingValue<see cref="object"/>.</param>
        /// <param name="serializer">The serializer<see cref="JsonSerializer"/>.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    var doubleValue = serializer.Deserialize<double>(reader);
                    return new Row { Double = doubleValue };

                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new Row { String = stringValue };

                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<object[]>(reader);
                    return new Row { AnythingArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type Row");
        }

        /// <summary>
        /// The WriteJson.
        /// </summary>
        /// <param name="writer">The writer<see cref="JsonWriter"/>.</param>
        /// <param name="untypedValue">The untypedValue<see cref="object"/>.</param>
        /// <param name="serializer">The serializer<see cref="JsonSerializer"/>.</param>
        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Row)untypedValue;
            if (value.Double != null)
            {
                serializer.Serialize(writer, value.Double.Value);
                return;
            }
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            if (value.AnythingArray != null)
            {
                serializer.Serialize(writer, value.AnythingArray);
                return;
            }
            throw new Exception("Cannot marshal type Row");
        }

        /// <summary>
        /// Defines the Singleton.
        /// </summary>
        public static readonly RowConverter Singleton = new RowConverter();
    }
}

namespace AzVMMonitorCostTotalData
{
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Globalization;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="CostManagementQuery" />.
    /// </summary>
    public static class CostManagementQuery
    {
        /// <summary>
        /// Defines the RequestByTagPayload.
        /// </summary>
        public static string RequestByTagPayload = "{\"type\":\"Usage\",\"timeframe\":\"MonthToDate\",\"dataset\":{\"granularity\":\"Daily\",\"filter\":{\"tags\":{\"name\":\"ms-resource-usage\",\"operator\":\"In\",\"values\":[\"azure-cloud-shell\"]}}}}";

        /// <summary>
        /// Defines the RequestByDimensionPayload.
        /// </summary>
        public static string RequestByDimensionPayload = "{\"type\":\"Usage\",\"timeframe\":\"MonthToDate\",\"dataset\":{\"granularity\":\"Daily\",\"filter\":{\"dimensions\":{\"name\":\"ResourceLocation\",\"operator\":\"In\",\"values\":[\"East US\",\"West Europe\"]}}}}";

        /// <summary>
        /// Defines the RequestCostManagementTotal.
        /// </summary>
        public static string RequestCostManagementTotal = "{\"type\":\"ActualCost\",\"dataSet\":{\"granularity\":\"None\",\"aggregation\":{\"totalCost\":{\"name\":\"Cost\",\"function\":\"Sum\"},\"totalCostUSD\":{\"name\":\"CostUSD\",\"function\":\"Sum\"}},\"grouping\":[{\"type\":\"Dimension\",\"name\":\"BillingPeriod\"}]}}";
    }

    /// <summary>
    /// Defines the <see cref="CostDataTotal" />.
    /// </summary>
    public partial class CostDataTotal
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        [JsonProperty("location")]
        public object Location { get; set; }

        /// <summary>
        /// Gets or sets the Sku.
        /// </summary>
        [JsonProperty("sku")]
        public object Sku { get; set; }

        /// <summary>
        /// Gets or sets the ETag.
        /// </summary>
        [JsonProperty("eTag")]
        public object ETag { get; set; }

        /// <summary>
        /// Gets or sets the Properties.
        /// </summary>
        [JsonProperty("properties")]
        public Properties Properties { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="Properties" />.
    /// </summary>
    public partial class Properties
    {
        /// <summary>
        /// Gets or sets the NextLink.
        /// </summary>
        [JsonProperty("nextLink")]
        public object NextLink { get; set; }

        /// <summary>
        /// Gets or sets the Columns.
        /// </summary>
        [JsonProperty("columns")]
        public Column[] Columns { get; set; }

        /// <summary>
        /// Gets or sets the Rows.
        /// </summary>
        [JsonProperty("rows")]
        public Row[][] Rows { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="Column" />.
    /// </summary>
    public partial class Column
    {
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="Row" />.
    /// </summary>
    public partial struct Row
    {
        /// <summary>
        /// Defines the Double.
        /// </summary>
        public double? Double;

        /// <summary>
        /// Defines the String.
        /// </summary>
        public string String;


        public static implicit operator Row(double Double) => new Row { Double = Double };

        public static implicit operator Row(string String) => new Row { String = String };
    }

    /// <summary>
    /// Defines the <see cref="CostDataTotal" />.
    /// </summary>
    public partial class CostDataTotal
    {
        /// <summary>
        /// The FromJson.
        /// </summary>
        /// <param name="json">The json<see cref="string"/>.</param>
        /// <returns>The <see cref="CostDataTotal"/>.</returns>
        public static CostDataTotal FromJson(string json) => JsonConvert.DeserializeObject<CostDataTotal>(json, AzVMMonitorCostsPerVMDisk.Converter.Settings);
    }

    /// <summary>
    /// Defines the <see cref="Serialize" />.
    /// </summary>
    public static class Serialize
    {
        /// <summary>
        /// The ToJson.
        /// </summary>
        /// <param name="self">The self<see cref="CostDataTotal"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string ToJson(this CostDataTotal self) => JsonConvert.SerializeObject(self, AzVMMonitorCostsPerVMDisk.Converter.Settings);
    }

    /// <summary>
    /// Defines the <see cref="Converter" />.
    /// </summary>
    internal static class Converter
    {
        /// <summary>
        /// Defines the Settings.
        /// </summary>
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                RowConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    /// <summary>
    /// Defines the <see cref="RowConverter" />.
    /// </summary>
    internal class RowConverter : JsonConverter
    {
        /// <summary>
        /// The CanConvert.
        /// </summary>
        /// <param name="t">The t<see cref="Type"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanConvert(Type t) => t == typeof(Row) || t == typeof(Row?);

        /// <summary>
        /// The ReadJson.
        /// </summary>
        /// <param name="reader">The reader<see cref="JsonReader"/>.</param>
        /// <param name="t">The t<see cref="Type"/>.</param>
        /// <param name="existingValue">The existingValue<see cref="object"/>.</param>
        /// <param name="serializer">The serializer<see cref="JsonSerializer"/>.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    var doubleValue = serializer.Deserialize<double>(reader);
                    return new Row { Double = doubleValue };

                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new Row { String = stringValue };
            }
            throw new Exception("Cannot unmarshal type Row");
        }

        /// <summary>
        /// The WriteJson.
        /// </summary>
        /// <param name="writer">The writer<see cref="JsonWriter"/>.</param>
        /// <param name="untypedValue">The untypedValue<see cref="object"/>.</param>
        /// <param name="serializer">The serializer<see cref="JsonSerializer"/>.</param>
        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Row)untypedValue;
            if (value.Double != null)
            {
                serializer.Serialize(writer, value.Double.Value);
                return;
            }
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            throw new Exception("Cannot marshal type Row");
        }

        /// <summary>
        /// Defines the Singleton.
        /// </summary>
        public static readonly RowConverter Singleton = new RowConverter();
    }

    /// <summary>
    /// Defines the <see cref="AzBillingRESTHelper" />.
    /// </summary>
    internal static class AzBillingRESTHelper
    {
        //логгер NLog
        /// <summary>
        /// Defines the _logger.
        /// </summary>
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Defines the <see cref="AzureDetails" />.
        /// </summary>
        public static class AzureDetails
        {
            /// <summary>
            /// Gets or sets the Response.
            /// </summary>
            public static string Response { get; set; }

            /// <summary>
            /// Gets or sets the ResponseCostDataTotal.
            /// </summary>
            public static string ResponseCostDataTotal { get; set; }

            /// <summary>
            /// Gets or sets the ResponseCostDataPerVM.
            /// </summary>
            public static string ResponseCostDataPerVM { get; set; }

            /// <summary>
            /// Gets or sets the ResponseCostDataPerVMDisk.
            /// </summary>
            public static string ResponseCostDataPerVMDisk { get; set; }

            /// <summary>
            /// Gets or sets the ResponseCostDataPerVMNetwork.
            /// </summary>
            public static string ResponseCostDataPerVMNetwork { get; set; }
        }

        /// <summary>
        /// The GetAuthorizationToken.
        /// </summary>
        /// <param name="clientId">The clientId<see cref="string"/>.</param>
        /// <param name="clientSecret">The clientSecret<see cref="string"/>.</param>
        /// <param name="clientTenantId">The clientTenantId<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetAuthorizationToken(string clientId, string clientSecret, string clientTenantId)
        {
            try
            {
                ClientCredential cc = new ClientCredential(clientId, clientSecret);
                var context = new AuthenticationContext("https://login.microsoftonline.com/" + clientTenantId);
                var result = context.AcquireTokenAsync("https://management.azure.com/", cc);
                if (result == null)
                {
                    throw new InvalidOperationException("Failed to obtain the Access token");
                    _logger.Error("Failed to obtain the Access token to Azure");
                }
                return result.Result.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to obtain the Access token to Azure");
            }

            return null;
        }

        /// <summary>
        /// The GetTotalCost.
        /// </summary>
        /// <param name="accesstoken">The accesstoken<see cref="string"/>.</param>
        /// <param name="subscriptionid">The subscriptionid<see cref="string"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public static async Task GetTotalCost(string accesstoken, string subscriptionid)
        {
            try
            {
                string response = "";
                HttpClient client = new HttpClient();

                client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + subscriptionid + "/providers/Microsoft.CostManagement/query?api-version=2019-11-01&$top=5000");
                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accesstoken);

                var payload = CostManagementQuery.RequestCostManagementTotal;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                response = await PostRequestAsync(request, client);

                //ждём ответа.....
                await Task.Delay(10000);
                _logger.Info("GetTotalCost - ok");
                AzureDetails.ResponseCostDataTotal = response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Something wrong with GetTotalCost");
            }
        }

        /// <summary>
        /// The GetCostByVM.
        /// </summary>
        /// <param name="accesstoken">The accesstoken<see cref="string"/>.</param>
        /// <param name="subscriptionid">The subscriptionid<see cref="string"/>.</param>
        /// <param name="vmname">The vmname<see cref="string"/>.</param>
        /// <param name="groupname">The groupname<see cref="string"/>.</param>
        /// <param name="timeframe">The timeframe<see cref="string"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public static async Task GetCostByVM(string accesstoken, string subscriptionid, string vmname, string groupname, string timeframe)
        {
            try
            {
                //string srcRequestCostByVM = "{\"type\":\"ActualCost\",\"dataSet\":{\"granularity\":\"None\",\"aggregation\":{\"totalCost\":{\"name\":\"Cost\",\"function\":\"Sum\"},\"totalCostUSD\":{\"name\":\"CostUSD\",\"function\":\"Sum\"}},\"grouping\":[{\"type\":\"Dimension\",\"name\":\"ResourceId\"},{\"type\":\"Dimension\",\"name\":\"ResourceType\"},{\"type\":\"Dimension\",\"name\":\"ResourceLocation\"},{\"type\":\"Dimension\",\"name\":\"ChargeType\"},{\"type\":\"Dimension\",\"name\":\"ResourceGroupName\"},{\"type\":\"Dimension\",\"name\":\"PublisherType\"},{\"type\":\"Dimension\",\"name\":\"ServiceName\"},{\"type\":\"Dimension\",\"name\":\"ServiceTier\"},{\"type\":\"Dimension\",\"name\":\"Meter\"}],\"include\":[\"Tags\"],\"filter\":{\"And\":[{\"Dimensions\":{\"Name\":\"PublisherType\",\"Operator\":\"In\",\"Values\":[\"azure\"]}},{\"Dimensions\":{\"Name\":\"ServiceName\",\"Operator\":\"In\",\"Values\":[\"virtual machines\"]}},{\"Dimensions\":{\"Name\":\"ResourceId\",\"Operator\":\"In\",\"Values\":[\"\\/subscriptions\\/[SubscriptionID]\\/resourcegroups\\/[GroupsName]\\/providers\\/microsoft.compute\\/virtualmachines\\/[VmName]\"]}},{\"Dimensions\":{\"Name\":\"BillingPeriod\",\"Operator\":\"In\",\"Values\":[\"202109(2021-08-14 - 2021-09-13)\"]}}]}}}";
                string srcRequestCostByVM = "{\"type\":\"ActualCost\",\"dataSet\":{\"granularity\":\"None\",\"aggregation\":{\"totalCost\":{\"name\":\"Cost\",\"function\":\"Sum\"},\"totalCostUSD\":{\"name\":\"CostUSD\",\"function\":\"Sum\"}},\"grouping\":[{\"type\":\"Dimension\",\"name\":\"ResourceId\"},{\"type\":\"Dimension\",\"name\":\"ResourceType\"},{\"type\":\"Dimension\",\"name\":\"ResourceLocation\"},{\"type\":\"Dimension\",\"name\":\"ChargeType\"},{\"type\":\"Dimension\",\"name\":\"ResourceGroupName\"},{\"type\":\"Dimension\",\"name\":\"PublisherType\"},{\"type\":\"Dimension\",\"name\":\"ServiceName\"},{\"type\":\"Dimension\",\"name\":\"ServiceTier\"},{\"type\":\"Dimension\",\"name\":\"Meter\"}],\"include\":[\"Tags\"],\"filter\":{\"And\":[{\"Dimensions\":{\"Name\":\"ResourceType\",\"Operator\":\"In\",\"Values\":[\"microsoft.compute\\/virtualmachines\"]}},{\"Dimensions\":{\"Name\":\"ResourceId\",\"Operator\":\"In\",\"Values\":[\"\\/subscriptions\\/[SubscriptionID]\\/resourcegroups\\/[GroupsName]\\/providers\\/microsoft.compute\\/virtualmachines\\/[VmName]\"]}}]}},\"timeframe\":\"[TimeFrame]\"}";

                var requestCostByVM = srcRequestCostByVM.Replace("[SubscriptionID]", subscriptionid).Replace("[GroupsName]", groupname.ToLower()).Replace("[VmName]", vmname.ToLower()).Replace("[TimeFrame]", timeframe);

                HttpClient client = new HttpClient();

                client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + subscriptionid + "/providers/Microsoft.CostManagement/query?api-version=2019-11-01&$top=5000");
                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accesstoken);

                var payload = requestCostByVM;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                string response = await PostRequestAsync(request, client);

                //ждём ответа.....
                await Task.Delay(10000);
                _logger.Info("GetCostByVM - ok");
                AzureDetails.ResponseCostDataPerVM = response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Something wrong with GetCostByVM");
            }
        }

        /// <summary>
        /// The GetCostByVMDisk.
        /// </summary>
        /// <param name="accesstoken">The accesstoken<see cref="string"/>.</param>
        /// <param name="subscriptionid">The subscriptionid<see cref="string"/>.</param>
        /// <param name="vmdiskname">The vmdiskname<see cref="string"/>.</param>
        /// <param name="groupname">The groupname<see cref="string"/>.</param>
        /// <param name="timeframe">The timeframe<see cref="string"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public static async Task GetCostByVMDisk(string accesstoken, string subscriptionid, string vmdiskname, string groupname, string timeframe)
        {
            try
            {
                //string srcRequestCostByVMDisk = "{\"type\":\"ActualCost\",\"dataSet\":{\"granularity\":\"None\",\"aggregation\":{\"totalCost\":{\"name\":\"Cost\",\"function\":\"Sum\"},\"totalCostUSD\":{\"name\":\"CostUSD\",\"function\":\"Sum\"}},\"grouping\":[{\"type\":\"Dimension\",\"name\":\"ResourceId\"},{\"type\":\"Dimension\",\"name\":\"ResourceType\"},{\"type\":\"Dimension\",\"name\":\"ResourceLocation\"},{\"type\":\"Dimension\",\"name\":\"ChargeType\"},{\"type\":\"Dimension\",\"name\":\"ResourceGroupName\"},{\"type\":\"Dimension\",\"name\":\"PublisherType\"},{\"type\":\"Dimension\",\"name\":\"ServiceName\"},{\"type\":\"Dimension\",\"name\":\"ServiceTier\"},{\"type\":\"Dimension\",\"name\":\"Meter\"}],\"include\":[\"Tags\"],\"filter\":{\"And\":[{\"Dimensions\":{\"Name\":\"ResourceType\",\"Operator\":\"In\",\"Values\":[\"microsoft.compute\\/disks\"]}},{\"Dimensions\":{\"Name\":\"ResourceId\",\"Operator\":\"In\",\"Values\":[\"\\/subscriptions\\/[SubscriptionID]\\/resourcegroups\\/[GroupsName]\\/providers\\/microsoft.compute\\/disks\\/[VmDiskName]\"]}}]}},\"timeframe\":\"Custom\",\"timePeriod\":{\"from\":\"2021-09-01T00:00:00+00:00\",\"to\":\"2021-09-30T23:59:59+00:00\"}}";
                string srcRequestCostByVMDisk = "{\"type\":\"ActualCost\",\"dataSet\":{\"granularity\":\"None\",\"aggregation\":{\"totalCost\":{\"name\":\"Cost\",\"function\":\"Sum\"},\"totalCostUSD\":{\"name\":\"CostUSD\",\"function\":\"Sum\"}},\"grouping\":[{\"type\":\"Dimension\",\"name\":\"ResourceId\"},{\"type\":\"Dimension\",\"name\":\"ResourceType\"},{\"type\":\"Dimension\",\"name\":\"ResourceLocation\"},{\"type\":\"Dimension\",\"name\":\"ChargeType\"},{\"type\":\"Dimension\",\"name\":\"ResourceGroupName\"},{\"type\":\"Dimension\",\"name\":\"PublisherType\"},{\"type\":\"Dimension\",\"name\":\"ServiceName\"},{\"type\":\"Dimension\",\"name\":\"ServiceTier\"},{\"type\":\"Dimension\",\"name\":\"Meter\"}],\"include\":[\"Tags\"],\"filter\":{\"And\":[{\"Dimensions\":{\"Name\":\"ResourceType\",\"Operator\":\"In\",\"Values\":[\"microsoft.compute\\/disks\"]}},{\"Dimensions\":{\"Name\":\"ResourceId\",\"Operator\":\"In\",\"Values\":[\"\\/subscriptions\\/[SubscriptionID]\\/resourcegroups\\/[GroupsName]\\/providers\\/microsoft.compute\\/disks\\/[VmDiskName]\"]}}]}},\"timeframe\":\"[TimeFrame]\"}";

                var requestCostByVMDisk = srcRequestCostByVMDisk.Replace("[SubscriptionID]", subscriptionid).Replace("[GroupsName]", groupname.ToLower()).Replace("[VmDiskName]", vmdiskname.ToLower()).Replace("[TimeFrame]", timeframe);

                HttpClient client = new HttpClient();

                client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + subscriptionid + "/providers/Microsoft.CostManagement/query?api-version=2019-11-01&$top=5000");
                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accesstoken);

                var payload = requestCostByVMDisk;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                string response = await PostRequestAsync(request, client);

                //ждём ответа.....
                await Task.Delay(10000);
                //Console.WriteLine("DEBUG RESPONSE GetCostByVMDisk: " + response);
                _logger.Info("GetCostByVMDisk - ok");
                AzureDetails.ResponseCostDataPerVMDisk = response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Something wrong with GetCostByVMDisk");
            }
        }

        /// <summary>
        /// The GetCostByVMNetwork.
        /// </summary>
        /// <param name="accesstoken">The accesstoken<see cref="string"/>.</param>
        /// <param name="subscriptionid">The subscriptionid<see cref="string"/>.</param>
        /// <param name="vmnetworkname">The vmnetworkname<see cref="string"/>.</param>
        /// <param name="groupname">The groupname<see cref="string"/>.</param>
        /// <param name="timeframe">The timeframe<see cref="string"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public static async Task GetCostByVMNetwork(string accesstoken, string subscriptionid, string vmnetworkname, string groupname, string timeframe)
        {
            try
            {
                //string srcRequestCostByVMNetwork = "{\"type\":\"ActualCost\",\"dataSet\":{\"granularity\":\"None\",\"aggregation\":{\"totalCost\":{\"name\":\"Cost\",\"function\":\"Sum\"},\"totalCostUSD\":{\"name\":\"CostUSD\",\"function\":\"Sum\"}},\"grouping\":[{\"type\":\"Dimension\",\"name\":\"ResourceId\"},{\"type\":\"Dimension\",\"name\":\"ResourceType\"},{\"type\":\"Dimension\",\"name\":\"ResourceLocation\"},{\"type\":\"Dimension\",\"name\":\"ChargeType\"},{\"type\":\"Dimension\",\"name\":\"ResourceGroupName\"},{\"type\":\"Dimension\",\"name\":\"PublisherType\"},{\"type\":\"Dimension\",\"name\":\"ServiceName\"},{\"type\":\"Dimension\",\"name\":\"ServiceTier\"},{\"type\":\"Dimension\",\"name\":\"Meter\"}],\"include\":[\"Tags\"],\"filter\":{\"And\":[{\"Dimensions\":{\"Name\":\"PublisherType\",\"Operator\":\"In\",\"Values\":[\"azure\"]}},{\"Dimensions\":{\"Name\":\"ResourceType\",\"Operator\":\"In\",\"Values\":[\"microsoft.network\\/publicipaddresses\"]}},{\"Dimensions\":{\"Name\":\"ResourceId\",\"Operator\":\"In\",\"Values\":[\"\\/subscriptions\\/[SubscriptionID]\\/resourcegroups\\/[GroupsName]\\/providers\\/microsoft.network\\/publicipaddresses\\/[VmDiskNetwork]\"]}},\"timeframe\":\"[TimeFrame]\"}";

                string srcRequestCostByVMNetwork = "{\"type\":\"ActualCost\",\"dataSet\":{\"granularity\":\"None\",\"aggregation\":{\"totalCost\":{\"name\":\"Cost\",\"function\":\"Sum\"},\"totalCostUSD\":{\"name\":\"CostUSD\",\"function\":\"Sum\"}},\"grouping\":[{\"type\":\"Dimension\",\"name\":\"ResourceId\"},{\"type\":\"Dimension\",\"name\":\"ResourceType\"},{\"type\":\"Dimension\",\"name\":\"ResourceLocation\"},{\"type\":\"Dimension\",\"name\":\"ChargeType\"},{\"type\":\"Dimension\",\"name\":\"ResourceGroupName\"},{\"type\":\"Dimension\",\"name\":\"PublisherType\"},{\"type\":\"Dimension\",\"name\":\"ServiceName\"},{\"type\":\"Dimension\",\"name\":\"ServiceTier\"},{\"type\":\"Dimension\",\"name\":\"Meter\"}],\"include\":[\"Tags\"],\"filter\":{\"And\":[{\"Dimensions\":{\"Name\":\"ResourceType\",\"Operator\":\"In\",\"Values\":[\"microsoft.network\\/publicipaddresses\"]}},{\"Dimensions\":{\"Name\":\"ResourceId\",\"Operator\":\"In\",\"Values\":[\"\\/subscriptions\\/[SubscriptionID]\\/resourcegroups\\/[GroupsName]\\/providers\\/microsoft.network\\/publicipaddresses\\/[VmNetwork]\"]}}]}},\"timeframe\":\"[TimeFrame]\"}";

                var requestCostByVMNetwork = srcRequestCostByVMNetwork.Replace("[SubscriptionID]", subscriptionid).Replace("[GroupsName]", groupname.ToLower()).Replace("[VmNetwork]", vmnetworkname.ToLower()).Replace("[TimeFrame]", timeframe);

                HttpClient client = new HttpClient();

                client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + subscriptionid + "/providers/Microsoft.CostManagement/query?api-version=2019-11-01&$top=5000");
                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accesstoken);

                var payload = requestCostByVMNetwork;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                string response = await PostRequestAsync(request, client);

                //ждём ответа.....
                await Task.Delay(10000);
                //Console.WriteLine("DEBUG RESPONSE GetCostByVMNetwork: " + response);
                _logger.Info("GetCostByVMNetwork - ok");
                AzureDetails.ResponseCostDataPerVMNetwork = response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Something wrong with GetCostByVMNetwork");
            }
        }

        /// <summary>
        /// The PostRequestAsync.
        /// </summary>
        /// <param name="postRequest">The postRequest<see cref="HttpRequestMessage"/>.</param>
        /// <param name="client">The client<see cref="HttpClient"/>.</param>
        /// <returns>The <see cref="Task{string}"/>.</returns>
        public static async Task<string> PostRequestAsync(HttpRequestMessage postRequest, HttpClient client)
        {
            var response = await client.SendAsync(postRequest);
            var responseString = string.Empty;
            try
            {
                response.EnsureSuccessStatusCode();
                responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex);
            }
            return responseString;
        }
    }
}
