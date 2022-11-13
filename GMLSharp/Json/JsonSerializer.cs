using System;
using System.Globalization;
using System.Text;
using System.Web;

namespace GMLSharp;

public class JsonSerializer {
	private readonly StringBuilder _builder;

	public bool Empty;

	public JsonSerializer(StringBuilder builder) {
		this._builder = builder;

		builder.Append('{');
	}

	public void Add(string key, string value) {
		this.BeginItem(key);

		this._builder.Append('"');
		this._builder.Append(HttpUtility.JavaScriptStringEncode(value));
		this._builder.Append('"');
	}

	private void Add(string key, JsonValue[] valueArray) {
		throw new NotImplementedException();
	}

	private void BeginItem(string key) {
		if (!this.Empty)
			this._builder.Append(',');
		this.Empty = false;

		this._builder.Append('"');
		this._builder.Append(HttpUtility.JavaScriptStringEncode(key));
		this._builder.Append("\":");
	}
	public void Add(string key, JsonValue value) {
		switch (value.Type) {
			case JsonValueType.String: {
				this.Add(key, $"\"{value.Value.String}\"");
				break;
			}
			case JsonValueType.Null:
				this.Add(key, (string)null);
				break;
			case JsonValueType.Int32:
				this.Add(key, value.Value.Int32.ToString());
				break;
			case JsonValueType.UnsignedInt32:
				this.Add(key, value.Value.UnsignedInt32.ToString());
				break;
			case JsonValueType.Int64:
				this.Add(key, value.Value.Int64.ToString());
				break;
			case JsonValueType.UnsignedInt64:
				this.Add(key, value.Value.UnsignedInt64.ToString());
				break;
			case JsonValueType.Double:
				this.Add(key, value.Value.Double.ToString(CultureInfo.InvariantCulture));
				break;
			case JsonValueType.Bool:
				this.Add(key, value.Value.Bool.ToString());
				break;
			case JsonValueType.Array:
				this.BeginItem(key);

				break;
			case JsonValueType.Object:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public void Finialize() {
		this._builder.Append('}');
	}
}


