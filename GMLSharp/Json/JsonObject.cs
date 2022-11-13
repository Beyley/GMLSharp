using System.Collections.Generic;
using System.Text;

namespace GMLSharp;

public class JsonObject {
	public readonly Dictionary<string, JsonValue> Members = new Dictionary<string, JsonValue>();
	public JsonObject(JsonObject other) {
		foreach (KeyValuePair<string, JsonValue> pair in other.Members)
			this.Members[pair.Key] = new JsonValue(pair.Value);
	}

	public void Serialize(StringBuilder builder) {
		JsonSerializer serializer = new JsonSerializer(builder);

		foreach (KeyValuePair<string, JsonValue> pair in this.Members)
			serializer.Add(pair.Key, pair.Value);

		serializer.Finialize();
	}
}

