using GMLSharp;

public static class Program {
	public static void Main(string[] args) {
		Lexer lexer = new Lexer(@"// A 20x200 coolbar button with text.
@GUI::Button {
    width: 200
    height: 20
    text: ""Operation failed successfully.\""
    button_style: ""Coolbar""
}

// Two tabs, named ""Tab 1"" and ""Tab 2"", each containing a label.
@GUI::TabWidget {
    min_width: 150
    min_height: 200

    @GUI::Label {
        title: ""Tab 1""
        text: ""This is the first tab""
    }

    @GUI::Label {
        title: ""Tab 2""
        text: ""This is the second tab. What did you expect?""
    }
}");
        
        lexer.Lex();
        
        foreach (Lexer.Token token in lexer.Tokens) {
            Console.WriteLine(token);
        }
	}
}