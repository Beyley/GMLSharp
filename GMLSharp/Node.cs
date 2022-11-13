#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GMLSharp;

public abstract class Node {
	public override string ToString() {
		StringBuilder builder = new StringBuilder();

		this.Format(builder, 0, false);

		return builder.ToString();
	}

	public virtual void Format(StringBuilder builder, int indentation, bool isInline) {}

	public static void Indent(StringBuilder builder, int indentationLevel) {
		for (int i = 0; i < indentationLevel; i++)
			builder.Append("    ");
	}

	public abstract Node Clone();

	public static Node FromToken <T>(Lexer.Token dequeue) where T : Node {
		return (T)Activator.CreateInstance(typeof(T), dequeue.View);
	}
}

public abstract class ValueNode : Node {}

public class Comment : Node {
	public string Text;

	public Comment(string text) {
		this.Text = text;
	}

	public override void Format(StringBuilder builder, int indentation, bool isInline) {
		if (isInline) {
			builder.Append(this.Text);
		}
		else {
			Indent(builder, indentation);
			builder.Append(this.Text);
		}
		builder.Append('\n');
	}
	public override Node Clone() {
		return new Comment(this.Text);
	}
}

public class KeyValuePair : Node {
	public string    Key;
	public ValueNode Value;
	public KeyValuePair(string key, ValueNode value) {
		this.Key   = key;
		this.Value = value;
	}

	public override void Format(StringBuilder builder, int indentation, bool isInline) {
		if (!isInline)
			Indent(builder, indentation);

		builder.Append(this.Key);
		this.Value.Format(builder, indentation, true);

		if (!isInline)
			builder.Append('\n');
	}
	public override Node Clone() {
		return new KeyValuePair(this.Key, this.Value.Clone() as ValueNode);
	}
}

public class JsonValueNode : ValueNode {
	public JsonValue Value;

	public JsonValueNode(string value) {
		throw new NotImplementedException("Json Values are not implemented!");
	}
	
	private JsonValueNode(JsonValue value) {
		this.Value = value;
	}

	public override Node Clone() {
		return new JsonValueNode(this.Value);
	}

	public override void Format(StringBuilder builder, int indentation, bool is_inline) {
		// if (!is_inline)
		// 	Indent(builder, indentation);
		// if (this.Value.Type == JsonValueType.Array) {
		// 	// custom array serialization as AK's doesn't pretty-print
		// 	// objects and arrays (we only care about arrays (for now))
		// 	builder.Append('[');
		// 	bool first = true;
		// 	foreach (JsonValue value in this.Value.Value.Array) {
		// 		if (!first)
		// 			builder.Append(", ");
		// 		first = false;
		// 		value.Serialize(builder);	
		// 	}
		// 	builder.append(']');
		// } else {
		// 	serialize(builder);
		// }
		// if (!is_inline)
		// 	builder.append('\n');
	}
}

public class Object : ValueNode {
	public string     Name;
	public List<Node> Properties = new List<Node>();
	public List<Node> SubObjects = new List<Node>();

	public Object() {}

	public Object(string name, List<Node> properties, List<Node> subObjects) {
		this.Name = name;

		properties.ForEach(x => this.Properties.Add(x.Clone()));
		subObjects.ForEach(x => this.SubObjects.Add(x.Clone()));
	}

	public void AddSubObjectChild(Node child) {
		if (child is not Object && child is not Comment)
			throw new ArgumentException("Sub object child must be an object or comment", nameof (child));

		this.SubObjects.Add(child.Clone());
	}

	public void AddPropertyChild(Node child) {
		if (child is not Object && child is not Comment)
			throw new ArgumentException("Sub object child must be an object or comment", nameof (child));

		this.SubObjects.Add(child.Clone());
	}

	private void for_each_property(Action<string, JsonValueNode> callback) {
		foreach (Node child in this.Properties)
			if (child is KeyValuePair property)
				if (property.Key != "layout" && property.Value is JsonValueNode jsonValueNode)
					callback(property.Key, jsonValueNode);
	}

	private void for_each_child_object(Action<Object> callback) {
		foreach (Node child in this.SubObjects)
			// doesn't capture layout as intended, as that's behind a kv-pair
			if (child is Object @object)
				callback(@object);
	}

	private void for_each_child_object_interruptible(Func<Object, IterationDecision> callback) {
		foreach (Node child in this.SubObjects)
			// doesn't capture layout as intended, as that's behind a kv-pair
			if (child is Object @object)
				if (callback(@object) == IterationDecision.Break)
					return;
	}

	Object? layout_object() {
		foreach (Node child in this.Properties) {
			if (child is KeyValuePair property) {
				if (property.Key == "layout") {
					// VERIFY(is<Object >(property->value().ptr()));
					Debug.Assert(property.Value is Object);
					return (Object)property.Value;
				}
			}
		}
		return null;
	}

	ValueNode? get_property(string property_name) {
		foreach (Node child in this.Properties) {
			if (child is KeyValuePair property) {
				if (property.Key == property_name)
					return property.Value;
			}
		}
		return null;
	}

	public override Node Clone() {
		return new Object(this.Name, this.Properties, this.SubObjects);
	}
}
