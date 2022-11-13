using System.Runtime.InteropServices;
using System.Text;

namespace GMLSharp;

public enum JsonValueType {
	Null,
	Int32,
	UnsignedInt32,
	Int64,
	UnsignedInt64,
	Double,
	Bool,
	String,
	Array,
	Object,
}

public struct JsonValue {
	public struct JsonValueValue {
		public string String;
		public int Int32;
		public uint UnsignedInt32;
		public long Int64;
		public ulong UnsignedInt64;
		public bool Bool;
		public double Double;
		public JsonValue[] Array;
		public JsonObject Object;
	}
	
	public JsonValueType  Type  = JsonValueType.Null;
	public JsonValueValue Value = new JsonValueValue();

	public JsonValue(JsonValue other) {
		this.Type  = other.Type;
		this.Value = other.Value;
	}
}
