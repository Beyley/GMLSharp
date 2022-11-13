/*
 * Copyright (c) 2022, kleines Filmröllchen <filmroellchen@serenityos.org>.
 * Copyright (c) 2022, Beyley Thomas <ep1cm1n10n123@gmail.com>.
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;

namespace GMLSharp;

public abstract class Node {
	public override string ToString() {
		StringBuilder builder = new StringBuilder();

		this.Format(builder, 0, false);

		return builder.ToString();
	}

	public virtual void Format(StringBuilder builder, int indentation, bool isInline) {
		throw new Exception();
	}

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

		builder.Append($"{this.Key}: ");
		this.Value.Format(builder, indentation, true);

		if (!isInline)
			builder.Append('\n');
	}
	public override Node Clone() {
		return new KeyValuePair(this.Key, this.Value.Clone() as ValueNode);
	}
}

public class JsonValueNode : ValueNode {
	public object? Value;

	public JsonValueNode(string value) {
		this.Value = JsonConvert.DeserializeObject(value);
	}

	public override Node Clone() {
		return new JsonValueNode(JsonConvert.SerializeObject(this.Value));
	}

	public override void Format(StringBuilder builder, int indentation, bool isInline) {
		if (!isInline)
			Indent(builder, indentation);

		builder.Append(JsonConvert.SerializeObject(this.Value));

		if (!isInline)
			builder.Append('\n');
	}

	public override string ToString() {
		return JsonConvert.SerializeObject(this.Value);
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
		if (child is not KeyValuePair && child is not Comment)
			throw new ArgumentException("Sub object child must be an object or comment", nameof (child));

		this.Properties.Add(child.Clone());
	}

	private void ForEachProperty(Action<string, JsonValueNode> callback) {
		foreach (Node child in this.Properties)
			if (child is KeyValuePair property)
				if (property.Key != "layout" && property.Value is JsonValueNode jsonValueNode)
					callback(property.Key, jsonValueNode);
	}

	private void ForEachChildObject(Action<Object> callback) {
		foreach (Node child in this.SubObjects)
			// doesn't capture layout as intended, as that's behind a kv-pair
			if (child is Object @object)
				callback(@object);
	}

	private void ForEachChildObjectInterruptible(Func<Object, IterationDecision> callback) {
		foreach (Node child in this.SubObjects)
			// doesn't capture layout as intended, as that's behind a kv-pair
			if (child is Object @object)
				if (callback(@object) == IterationDecision.Break)
					return;
	}

	private Object? LayoutObject() {
		foreach (Node child in this.Properties)
			if (child is KeyValuePair property)
				if (property.Key == "layout") {
					// VERIFY(is<Object >(property->value().ptr()));
					Debug.Assert(property.Value is Object);
					return (Object)property.Value;
				}
		return null;
	}

	private ValueNode? GetProperty(string propertyName) {
		foreach (Node child in this.Properties)
			if (child is KeyValuePair property)
				if (property.Key == propertyName)
					return property.Value;
		return null;
	}

	public override void Format(StringBuilder builder, int indentation, bool isInline) {
		if (!isInline)
			Indent(builder, indentation);
		builder.Append('@');
		builder.Append(this.Name);
		builder.Append(" {");
		if (this.Properties.Count > 0 || this.SubObjects.Count > 0) {
			builder.Append('\n');

			foreach (Node? property in this.Properties)
				property.Format(builder, indentation + 1, false);

			if (this.Properties.Count > 0 && this.SubObjects.Count > 0)
				builder.Append("\n");

			// This loop is necessary as we need to know what the last child is.
			for (int i = 0; i < this.SubObjects.Count; ++i) {
				Node? child = this.SubObjects[i];
				child.Format(builder, indentation + 1, false);

				if (child is Object && i != this.SubObjects.Count - 1)
					builder.Append('\n');
			}

			Indent(builder, indentation);
		}
		builder.Append('}');
		if (!isInline)
			builder.Append('\n');
	}

	public override Node Clone() {
		return new Object(this.Name, this.Properties, this.SubObjects);
	}

	public override string ToString() {
		return this.Name;
	}
}

