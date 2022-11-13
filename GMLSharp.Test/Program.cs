using GMLSharp;
using KeyValuePair = GMLSharp.KeyValuePair;
using Object = GMLSharp.Object;

public static class Program {
    public static string GML = @"
@GUI::Widget {
    fixed_width: 260
    fixed_height: 85
    fill_with_background_color: true
    layout: @GUI::VerticalBoxLayout {
        margins: [4]
    }

    @GUI::Widget {
        fixed_height: 24
        layout: @GUI::HorizontalBoxLayout {}

        @GUI::Label {
            text: ""Title:""
            text_alignment: ""CenterLeft""
            fixed_width: 30
        }

        @GUI::TextBox {
            name: ""title_textbox""
        }
    }

    @GUI::Widget {
        fixed_height: 24
        layout: @GUI::HorizontalBoxLayout {}

        @GUI::Label {
            text: ""URL:""
            text_alignment: ""CenterLeft""
            fixed_width: 30
        }

        @GUI::TextBox {
            name: ""url_textbox""
        }
    }

    @GUI::Widget {
        fixed_height: 24
        layout: @GUI::HorizontalBoxLayout {}

        @GUI::Layout::Spacer {}

        @GUI::Button {
            name: ""ok_button""
            text: ""OK""
            fixed_width: 75
        }

        @GUI::Button {
            name: ""cancel_button""
            text: ""Cancel""
            fixed_width: 75
        }
    }
}";
	public static void Main(string[] args) {
        Parser parser = new Parser();

        GMLFile file = parser.Parse(GML);
        
        void PrintObject(Object obj, int indentation) {
            Console.Write(new string(' ', 4 * indentation));
            Console.WriteLine($"Object Name: {obj.Name}");
 
            foreach (Node subObjs in obj.SubObjects) {
                Console.Write(new string(' ', 4 * (indentation + 1)));
            
                if(subObjs is Object subsubObject)
                    PrintObject(subsubObject, indentation + 1);
                else if (subObjs is KeyValuePair pair) {
                    if (pair.Value is Object subsubsubObject) {
                        PrintObject(subsubsubObject, indentation + 1);
                    }
                    else {
                        Console.WriteLine($"{pair.Key}:{pair.Value}");
                    }
                }
                
            } 
        }
        
        PrintObject(file.MainClass, 0);
    }
}