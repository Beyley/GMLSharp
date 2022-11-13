/*
 * Copyright (c) 2022, kleines Filmröllchen <filmroellchen@serenityos.org>.
 * Copyright (c) 2022, Beyley Thomas <ep1cm1n10n123@gmail.com>.
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

using System;
using System.Collections.Generic;

namespace GMLSharp;

public class GMLFile : Node {
	public List<Comment> LeadingComments = new List<Comment>();
	public Object        MainClass;
	public List<Comment> TrailingComments = new List<Comment>();

	public void AddChild(Node child) {
		if (!this.HasMainClass()) {
			if (child is Comment comment) {
				this.LeadingComments.Add(comment);
				return;
			}
			if (child is Object @object) {
				this.MainClass = @object;
				return;
			}
			throw new Exception("Unexpected data before main class");
		}
		// After the main class, only comments are allowed.
		if (child is not Comment trailingComment)
			throw new Exception("Data not allowed after main class");
		this.TrailingComments.Add(trailingComment);
	}

	public bool HasMainClass() {
		return this.MainClass != null;
	}

	public override Node Clone() {
		throw new NotImplementedException();
	}
}

