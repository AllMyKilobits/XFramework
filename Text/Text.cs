using System.Collections.Generic;
using System.Text;

namespace XF
{
    /// <summary> 
    /// We call this class Text for convenience of calls and beauty of syntax-> i.e. Text.break(), Text.calculate_width("...") etc.
    /// </summary>
    static public class Text
    {
        /////////////////////////////////////////////////////////////////////////////////////

        #region Text Break Logic

        // first, we declare the result classes : 

        #region Result classes
        /// <summary> A fairly simple public-acces class with only two publicly accessible variables. </summary>
        public  class Line
        {
            public string text;
            public float width;

            public Line(string text, float width)
            {
                this.text = text;
                this.width = width;
            }
        }

        /// <summary>
        /// A class that returns the complex results of a break operation.
        /// </summary>
        public class BreakResult
        {
            internal Graphics.BitmapFont font;

            /// <summary> The array of returned lines. </summary>            
            public Line[] lines;

            /// <summary> A property that returns the total number of lines in the broken text. </summary>            
            public int num_lines { get { return lines.Length; } }

            /// <summary> The total height, in pixels, of the text. </summary>            
            public float total_height { get { return font.v_spacing * num_lines; } }

            /// <summary> The total width, in pixels, of the text. </summary>            
            public float max_width;
        } 
        #endregion
        
        #region Private fields 

        // all of these are helpers for the break_text method calls.
        static private List<Line> linelist = new List<Line>();
        static private StringBuilder line = new StringBuilder();
        static private StringBuilder word; 

        #endregion

        #region Definitions of separators

        private const string separators = " ";
        private const string line_break_chars = "|";

        #endregion

        #region The break method itself
        
        /// <summary> Breaks input text into lines based on font data. 
        /// It takes into account the width of each individual character as defined in font data to determine total width. It correctly takes into accountline break characters. </summary>
        /// <param name="input">the text to be broken into lines </param>
        /// <param name="max_width">maximum width of a single line</param>
        /// <param name="font">font</param>
        /// <returns>A neatly formated list of lines, including width data for each</returns>
        static public BreakResult break_text(string input, float max_width, Graphics.BitmapFont font)
        {
            var br = new BreakResult();
            br.font = font;

            float lw = 0f; // width of current line
            float ww = 0f; // width of current word

            word = new StringBuilder();

            linelist.Clear();
            line.Clear();

            float sw = font[' '].width; // space width

            for (int cursor = 0; cursor < input.Length; cursor++)
            {
                var c = input[cursor];

                if (c >= font.char_data.Length) throw new System.Exception("CHAR NOT SUPPORTED"); // this should never happen but even so the framework has catchers in place for graceful resuming 

                var last_letter = cursor == input.Length - 1;

                if (line_break_chars.IndexOf(c) >= 0)
                {
                    linelist.Add(new Line(line.ToString(), lw));     // add current line to new line:
                    line.Clear(); lw = 0f;                           // clear line vars:
                    word.Clear(); ww = 0f;
                }
                else if (separators.IndexOf(c) >= 0)
                {
                    if (lw + sw + ww > max_width) // can'tex add word without overshooting
                    {
                        // add to new line:
                        linelist.Add(new Line(line.ToString(), lw));
                        // clear line vars:
                        line.Clear(); lw = 0f;
                        // add current word to new line and set width:
                        line.Append(word); lw = ww;
                        // clear word:
                        word.Clear(); ww = 0f;
                    }
                    else // can add the word.
                    {
                        if (line.Length > 0) { line.Append(' '); lw += sw; }
                        line.Append(word); lw += ww;
                        word.Clear(); ww = 0f;
                    }

                }
                else
                {
                    word.Append(c);
                    ww += font[c].width;
                }

                if (last_letter) // SPECIAL CASE
                {
                    if (lw + sw + ww > max_width) // can'tex add word without overshooting
                    {

                        linelist.Add(new Line(line.ToString(), lw)); // add to new line:
                        line.Clear(); lw = 0f;          // clear line vars:                        
                        line.Append(word); lw = ww;     // add current word to new line and set width:                        
                        word.Clear(); ww = 0f;          // clear word:
                        linelist.Add(new Line(line.ToString(), lw));
                    }
                    else
                    {
                        if (line.Length > 0) { line.Append(' '); lw += sw; }
                        line.Append(word); lw += ww;
                        linelist.Add(new Line(line.ToString(), lw));
                    }

                }

            }

            br.lines = linelist.ToArray(); // after having this entire method iterate through hundreds of numbers, the comparative costs of returning a neatly formated array is negligible.
            foreach (var ll in br.lines) if (ll.width > br.max_width) br.max_width = ll.width;
            return br;
        }  
        #endregion

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Unit test

        static private string kzap = "";
        
        /// <summary>
        /// A unit test that is to display the results on the screen.
        /// </summary>
        static public void break_unit_test()
        {
            kzap = kzap + Input.input_char;
            var break_result = break_text(kzap, 500f, Graphics.default_font);
            //var off_y = 0f;

            var spr = Graphics.add_sprite("white", 200f, 200f);
            spr.fit_rect(200f, 200f, 500f, 500f);
            spr.set_colors(0xff0000, 0.12f);

            foreach (var line in break_result.lines)
            {
                //Graphics.add_text(200f, 200f + off_y, line.text);
                //off_y += Graphics.default_font.v_spacing;
            }

            render_text_multiline(kzap, 200f, 200f, 500f, HorizontalAlignment.justify);
        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Alignment regulation

        /// <summary> Counts all the occurences of the space character in a given text. </summary>        
        /// <returns>the number of spaces. Minimum is one for the special purposes of this method</returns>
        static private int count_spaces(string text)
        {
            var counter = 0;
            for (int c = 0; c < text.Length; c++) if (text[c] == ' ') counter++;
            if (counter == 0) counter = 1;
            return counter;
        }

        public enum VerticalAlignment
        {
            top,
            center,
            bottom
        }

        public enum HorizontalAlignment
        {
            left,
            center,
            right,
            justify
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Rendering procedures
        /// <summary>Use this every time you need to render text that needs to be multiline</summary>
        /// <param name="text">The string to be rendered.</param>
        /// <param name="x">the left portion of the text rect</param>
        /// <param name="y">the vertical pivot of the text rect</param>
        /// <param name="max_width"> maximum width of the paragraph</param>
        /// <param name="horizontal_alignment">Where the bounding box is given by x, x+max_width</param>
        /// <param name="vertical_alignment"> In this overload, verticality hinges on the supplied y</param>
        /// <returns>An array of text sprites that were created to render this text</returns>
        static public Graphics.Sprite[] render_text_multiline(string text, float x, float y, float max_width,
                                                                HorizontalAlignment horizontal_alignment = HorizontalAlignment.left,
                                                                VerticalAlignment vertical_alignment = VerticalAlignment.top)
        {
            var broken_text = break_text(text, max_width, Graphics.current_font);

            var y_zero = 0f;
            if (vertical_alignment == VerticalAlignment.center) y_zero -= broken_text.total_height / 2;
            if (vertical_alignment == VerticalAlignment.bottom) y_zero -= broken_text.total_height;
            var _sprs = new Graphics.Sprite[broken_text.num_lines];

            for (int l = 0; l < broken_text.lines.Length; l++)
            {
                var line = broken_text.lines[l];

                var x_zero = 0f;

                if (horizontal_alignment == HorizontalAlignment.center) x_zero = (max_width - line.width) * 0.5f;
                if (horizontal_alignment == HorizontalAlignment.right) x_zero = (max_width - line.width);

                _sprs[l] = Graphics.add_text(x + x_zero, y + y_zero, line.text);

                // JUSTIFY ALGORITHM:
                if (horizontal_alignment == HorizontalAlignment.justify) if (l != broken_text.lines.Length - 1)
                        _sprs[l].txt.additional_spacing = (max_width - line.width) / count_spaces(line.text);

                y_zero += broken_text.font.v_spacing;
            }
            return _sprs;
        }


        /// <summary>Renders a text in a given box, with given alignments. Will break the text if necessary</summary>
        /// <param name="text">the string to be broken. The character | is used as a line break. </param>
        /// <param name="x0">Left coordinate of the box</param> <param name="y0"> Top coordinate of the box</param>
        /// <param name="w">width of the box</param><param name="h">height of the box</param>
        /// <param name="horizontal_alignment">Self-explanatory. Justified included! </param>
        /// <param name="vertical_alignment">Self-explanatory.</param>
        /// <returns>An array of sprites that comprise the new letters</returns>
        static public Graphics.Sprite[] render_text_in_box      (string text, float x0, float y0, float w, float h,
                                                                HorizontalAlignment horizontal_alignment = HorizontalAlignment.left,
                                                                VerticalAlignment vertical_alignment = VerticalAlignment.top)
        {

            var font = Graphics.current_font;

            var broken_text = break_text(text, w, font);
            var _sprs = new Graphics.Sprite[broken_text.num_lines];

            var total_h = Graphics.current_font.v_spacing * broken_text.lines.Length;

            #region Determine Y coord by alignment
            var zero_y = 0f;
            switch (vertical_alignment)
            {
                case VerticalAlignment.top: zero_y = y0; break;
                case VerticalAlignment.center: zero_y = y0 + h / 2 - total_h / 2; break;
                case VerticalAlignment.bottom: zero_y = y0 + h - Graphics.current_font.v_spacing; break;
            } 
            #endregion
            
            for (int l = 0; l < broken_text.lines.Length; l++)
            {
                var line = broken_text.lines[l];

                #region Determine X coord by alignment
                var x_zero = 0f;
                switch (horizontal_alignment)
                {
                    case HorizontalAlignment.left: x_zero = x0; break;
                    case HorizontalAlignment.center: x_zero = x0 + (w - line.width) / 2; break;
                    case HorizontalAlignment.right: x_zero = x0 + (w - line.width); break;
                    default: break; // we manage the other later
                } 
                #endregion

                _sprs[l] = Graphics.add_text(x_zero, zero_y, line.text);

                if (horizontal_alignment == HorizontalAlignment.justify)
                if (l != broken_text.lines.Length - 1)
                _sprs[l].txt.additional_spacing = (w - line.width) / count_spaces(line.text);
                zero_y += broken_text.font.v_spacing;
            }

            return _sprs;
        }
                                                                
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////
    }
}
