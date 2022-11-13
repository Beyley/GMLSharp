/*
 * Copyright (c) 2020, Andreas Kling <kling@serenityos.org>
 * Copyright (c) 2022, kleines Filmröllchen <filmroellchen@serenityos.org>
 * Copyright (c) 2022, Idan Horowitz <idan.horowitz@serenityos.org>
 * Copyright (c) 2022, Beyley Thomas <ep1cm1n10n123@gmail.com>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GMLSharp;

public class Parser {
	public Parser() {}

	private static Object parse_gml_object(Queue<Lexer.Token> tokens) {
		Object @object = new Object();

		TokenType peek() => tokens.Count == 0
			? TokenType.Unknown
			: tokens.Peek().Type;

		while (peek() == TokenType.Comment)
			@object.AddPropertyChild(Node.FromToken<Comment>(tokens.Dequeue()));

		if (peek() != TokenType.ClassMarker)
			throw new Exception("Expected class marker");

		tokens.Dequeue();

		if (peek() != TokenType.ClassName)
			throw new Exception("Expected class name");

		Lexer.Token class_name = tokens.Dequeue();
		@object.Name = class_name.View;

		if (peek() == TokenType.LeftCurly) {
			tokens.Dequeue();

			LinkedList<Comment> pending_comments = new LinkedList<Comment>();
			for (;;) {
				if (peek() == TokenType.RightCurly) {
					// End of object
					break;
				}

				if (peek() == TokenType.ClassMarker) {
					// It's a child object.

					while (pending_comments.Count != 0) {
						@object.AddSubObjectChild(pending_comments.First());
						pending_comments.RemoveFirst();
					}

					@object.AddSubObjectChild(parse_gml_object(tokens));
				}
				else if (peek() == TokenType.Identifier) {
					// It's a property.

					while (pending_comments.Count != 0) {
						@object.AddPropertyChild(pending_comments.First());
						pending_comments.RemoveFirst();
					}

					Lexer.Token property_name = tokens.Dequeue();

					if (property_name.View.Length == 0)
						throw new Exception("Expected non-empty property name");

					if (peek() != TokenType.Colon)
						throw new Exception("Expected ':'");

					tokens.Dequeue();

					ValueNode value = null;
					if (peek() == TokenType.ClassMarker)
						value = parse_gml_object(tokens);
					else if (peek() == TokenType.JsonValue)
						value = new JsonValueNode(tokens.Dequeue().View); //TODO: big todo here
					
					KeyValuePair property = new KeyValuePair(property_name.View, value ?? throw new NullReferenceException("Value must not be null!"));
					@object.AddPropertyChild(property);
				}
				else if (peek() == TokenType.Comment) {
					pending_comments.AddLast((Comment)Node.FromToken<Comment>(tokens.Dequeue()));
				}
				else {
					throw new Exception("Expected child, property, comment, or }}");
				}
			}

			// Insert any left-over comments as sub object children, as these will be serialized last
			while (pending_comments.Count != 0) {
				@object.AddSubObjectChild(pending_comments.First());
				pending_comments.RemoveFirst();
			}

			if (peek() != TokenType.RightCurly)
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

		TokenType Peek() => tokens.Count == 0
			? TokenType.Unknown
			: tokens.Peek().Type;

		while (Peek() == TokenType.Comment)
			file.add_child(Node.FromToken<Comment>(tokens.Dequeue()));

		file.add_child(parse_gml_object(tokens));

		while (tokens.Count > 0)
			file.add_child(Node.FromToken<Comment>(tokens.Dequeue()));

		return file;
	}
}
