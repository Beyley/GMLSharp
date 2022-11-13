/*
 * Copyright (c) 2020, Andreas Kling <kling@serenityos.org>
 * Copyright (c) 2022, Beyley Thomas <ep1cm1n10n123@gmail.com>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

using System.Collections.Generic;
using System.Diagnostics;

namespace GMLSharp;

public enum TokenType {
	Unknown,
	Comment,
	ClassMarker,
	ClassName,
	LeftCurly,
	RightCurly,
	Identifier,
	Colon,
	JsonValue
}

public class Lexer {
	private readonly string _input;

	public readonly List<Token> Tokens;

	private int      _index;
	private Position _position;

	public Lexer(string input) {
		this._input = input.Replace("\r\n", "\n");

		this.Tokens = new List<Token>();
	}

	public char Peek(long offset = 0) {
		if (this._index + offset >= this._input.Length)
			return (char)0;

		return this._input[(int)(this._index + offset)];
	}

	public char Consume() {
		Debug.Assert(this._index < this._input.Length);

		char ch = this._input[this._index++];

		if (ch == '\n') {
			this._position.Line++;
			this._position.Column = 0;
		}
		else {
			this._position.Column++;
		}

		return ch;
	}

	private static bool IsValidIdentifierStart(char ch) {
		return char.IsLetter(ch) || ch == '_';
	}

	private static bool IsValidIdentifierCharacter(char ch) {
		return char.IsLetterOrDigit(ch) || ch == '_';
	}

	private static bool IsValidClassCharacter(char ch) {
		return char.IsLetterOrDigit(ch) || ch is '_' or ':';
	}

	public void Lex() {
		int      tokenStartIndex    = 0;
		Position tokenStartPosition = new Position();

		void BeginToken() {
			tokenStartIndex    = this._index;
			tokenStartPosition = this._position;
		}

		void CommitToken(TokenType type) {
			Token token;
			token.View  = this._input.Substring(tokenStartIndex, this._index - tokenStartIndex);
			token.Type  = type;
			token.Start = tokenStartPosition;
			token.End   = this._position;
			this.Tokens.Add(token);
		}

		void ConsumeClass() {
			BeginToken();
			this.Consume();
			CommitToken(TokenType.ClassMarker);
			BeginToken();
			while (IsValidClassCharacter(this.Peek()))
				this.Consume();
			CommitToken(TokenType.ClassName);
		}

		while (this._index < this._input.Length) {
			if (char.IsWhiteSpace(this.Peek())) {
				BeginToken();
				while (char.IsWhiteSpace(this.Peek()))
					this.Consume();
				continue;
			}

			// C++ style comments
			if (this.Peek() != 0 && this.Peek() == '/' && this.Peek(1) == '/') {
				BeginToken();
				while (this.Peek() != 0 && this.Peek() != '\n')
					this.Consume();
				CommitToken(TokenType.Comment);
				continue;
			}

			if (this.Peek() == '{') {
				BeginToken();
				this.Consume();
				CommitToken(TokenType.LeftCurly);
				continue;
			}

			if (this.Peek() == '}') {
				BeginToken();
				this.Consume();
				CommitToken(TokenType.RightCurly);
				continue;
			}

			if (this.Peek() == '@') {
				ConsumeClass();
				continue;
			}

			if (IsValidIdentifierStart(this.Peek())) {
				BeginToken();
				this.Consume();
				while (IsValidIdentifierCharacter(this.Peek()))
					this.Consume();
				CommitToken(TokenType.Identifier);
				continue;
			}

			if (this.Peek() == ':') {
				BeginToken();
				this.Consume();
				CommitToken(TokenType.Colon);

				while (char.IsWhiteSpace(this.Peek()))
					this.Consume();

				if (this.Peek() == '@') {
					ConsumeClass();
				}
				else {
					BeginToken();
					while (this.Peek() != 0 && this.Peek() != '\n')
						this.Consume();
					CommitToken(TokenType.JsonValue);
				}
				continue;
			}

			this.Consume();
			CommitToken(TokenType.Unknown);
		}
	}

	public struct Position {
		public int Line;
		public int Column;
	}

	public struct Token {
		public TokenType Type = TokenType.Unknown;
		public string    View;
		public Position  Start;
		public Position  End;

		public override string ToString() {
			return this.Type.ToString();
		}

		public Token() {
			this.View  = "";
			this.Start = new Position();
			this.End   = new Position();
		}
	}
}


