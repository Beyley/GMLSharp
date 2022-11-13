/*
 * Copyright (c) 2020, Andreas Kling <kling@serenityos.org>
 * Copyright (c) 2022, kleines Filmröllchen <filmroellchen@serenityos.org>
 * Copyright (c) 2022, Idan Horowitz <idan.horowitz@serenityos.org>
 * Copyright (c) 2022, Beyley Thomas <ep1cm1n10n123@gmail.com>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace GMLSharp;

public class Parser {
	private static Object ParseGmlObject(Queue<Lexer.Token> tokens) {
		Object @object = new Object();

		TokenType Peek() {
			return tokens.Count == 0
				? TokenType.Unknown
				: tokens.Peek().Type;
		}

		while (Peek() == TokenType.Comment)
			@object.AddPropertyChild(Node.FromToken<Comment>(tokens.Dequeue()));

		if (Peek() != TokenType.ClassMarker)
			throw new Exception("Expected class marker");

		tokens.Dequeue();

		if (Peek() != TokenType.ClassName)
			throw new Exception("Expected class name");

		Lexer.Token className = tokens.Dequeue();
		@object.Name = className.View;

		if (Peek() == TokenType.LeftCurly) {
			tokens.Dequeue();

			LinkedList<Comment> pendingComments = new LinkedList<Comment>();
			for (;;) {
				if (Peek() == TokenType.RightCurly)
					// End of object
					break;

				if (Peek() == TokenType.ClassMarker) {
					// It's a child object.

					while (pendingComments.Count != 0) {
						@object.AddSubObjectChild(pendingComments.First());
						pendingComments.RemoveFirst();
					}

					@object.AddSubObjectChild(ParseGmlObject(tokens));
				}
				else if (Peek() == TokenType.Identifier) {
					// It's a property.

					while (pendingComments.Count != 0) {
						@object.AddPropertyChild(pendingComments.First());
						pendingComments.RemoveFirst();
					}

					Lexer.Token propertyName = tokens.Dequeue();

					if (propertyName.View.Length == 0)
						throw new Exception("Expected non-empty property name");

					if (Peek() != TokenType.Colon)
						throw new Exception("Expected ':'");

					tokens.Dequeue();

					ValueNode value = null;
					if (Peek() == TokenType.ClassMarker)
						value = ParseGmlObject(tokens);
					else if (Peek() == TokenType.JsonValue)
						value = new JsonValueNode(tokens.Dequeue().View);

					KeyValuePair property = new KeyValuePair(propertyName.View, value ?? throw new NullReferenceException("Value must not be null!"));
					@object.AddPropertyChild(property);
				}
				else if (Peek() == TokenType.Comment) {
					pendingComments.AddLast((Comment)Node.FromToken<Comment>(tokens.Dequeue()));
				}
				else {
					throw new Exception("Expected child, property, comment, or }}");
				}
			}

			// Insert any left-over comments as sub object children, as these will be serialized last
			while (pendingComments.Count != 0) {
				@object.AddSubObjectChild(pendingComments.First());
				pendingComments.RemoveFirst();
			}

			if (Peek() != TokenType.RightCurly)
				throw new Exception("Expected }}");

			tokens.Dequeue();
		}

		return @object;
	}

	public GMLFile Parse(string gml) {
		Lexer lexer = new Lexer(gml);

		lexer.Lex();

		Queue<Lexer.Token> tokens = new Queue<Lexer.Token>(lexer.Tokens.Count);

		lexer.Tokens.ForEach(x => tokens.Enqueue(x));

		GMLFile file = new GMLFile();

		TokenType Peek() {
			return tokens.Count == 0
				? TokenType.Unknown
				: tokens.Peek().Type;
		}

		while (Peek() == TokenType.Comment)
			file.AddChild(Node.FromToken<Comment>(tokens.Dequeue()));

		file.AddChild(ParseGmlObject(tokens));

		while (tokens.Count > 0)
			file.AddChild(Node.FromToken<Comment>(tokens.Dequeue()));

		return file;
	}
}

