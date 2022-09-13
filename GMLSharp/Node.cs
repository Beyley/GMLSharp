using System.Text;

namespace GMLSharp;

public class Node {
	public override string ToString() {
		StringBuilder builder = new();

		this.Format(builder, 0, false);

		return builder.ToString();
	}

	public virtual void Format(StringBuilder builder, int indentationLevel, bool isInline) {}

	public static void Indent(StringBuilder builder, int indentationLevel) {
		for (int i = 0; i < indentationLevel; i++)
			builder.Append("    ");
	}
}

public class ValueNode : Node {}

public class Comment : Node {
	private readonly string _text;

	public Comment(string text) {
		this._text = text;
	}

	public override void Format(StringBuilder builder, int indentationLevel, bool isInline) {
		if (isInline) {
			builder.Append(this._text);
		}
		else {
			Indent(builder, indentationLevel);
			builder.Append(this._text);
		}
		builder.Append('\n');
	}
}
