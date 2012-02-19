using System;
using System.Collections.Generic;
using XF;
using SFML.Window;

namespace XF
{
    public static partial class Graphics
    {
        private static void render_text(Sprite spr)
        {
            Texture t = spr.txt.font_id.tex;
            BitmapFont f = spr.txt.font_id;

            if (t == null) return;

            if (spr.txt.txt.Length > 0 && spr.txt.txt[0] == '@') {render_text_advanced(spr); return; }
            #region SFML sprite state set
            sfml_spr.Image = t.image;
            sfml_spr.BlendMode = (SFML.Graphics.BlendMode)spr.blit.mode;
            sfml_spr.Color = familiarize_color(spr.blit.color);
            sfml_spr.Rotation = 0.0f;
            sfml_spr.Origin = new Vector2f(0f, 0f);// new Vector2f(spr.blit.scale.x, spr.blit.scale.y);            
            sfml_spr.Scale = new Vector2f(spr.blit.scale.x, spr.blit.scale.y);
            #endregion
            float x_offset = 0.0f, y_offset = 0.0f;

            for (int c = 0, n = spr.txt.txt.Length; c < n; c++)
            {
                char a = spr.txt.txt[c];
                uint sx = f.char_data[a].seq_x; uint sy = f.char_data[a].seq_y;

                sfml_spr.Origin = new Vector2f(0f, 0f);
                sfml_spr.Position = new Vector2f(spr.coords.x + x_offset, spr.coords.y + y_offset);
                sfml_spr.SubRect = new SFML.Graphics.IntRect((int)(t.size.x * sx),
                                                             (int)(t.size.y * sy),
                                                             (int)(t.size.x),// * (sx + 1)), 
                                                             (int)(t.size.y));// * (sy + 1)));

                //if (spr.effect == null) render_target.Draw(sfml_spr); else

                render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);

                float w = f.char_data[a].width + f.h_spacing;
                if (a == ' ') w += spr.txt.additional_spacing;
                x_offset += (w) * spr.blit.scale.x;

            }

        }

        private static string find_directive_param(string str, int start, out int continue_at)
        {
            continue_at = start;
            var s = str.IndexOf('[', start); if (s == -1) return "";
            var e = str.IndexOf(']', s); if (e == -1)     return "";
            continue_at = e ;
            return str.Substring(s + 1, e - s + 1 - 2); 

        }

        private static void render_text_advanced(Sprite spr)
        {
            Texture t = spr.txt.font_id.tex;
            BitmapFont f = spr.txt.font_id;
            #region SFML sprite state set

            sfml_spr.Image = t.image;
            sfml_spr.BlendMode = (SFML.Graphics.BlendMode)spr.blit.mode;
            sfml_spr.Color = familiarize_color(spr.blit.color);
            sfml_spr.Rotation = 0.0f;
            sfml_spr.Origin = new Vector2f(0f, 0f);// new Vector2f(spr.blit.scale.x, spr.blit.scale.y);            
            sfml_spr.Scale = new Vector2f(spr.blit.scale.x, spr.blit.scale.y);
           
            #endregion

            float x_offset = 0.0f, y_offset = 0.0f;
            for (int c = 1, n = spr.txt.txt.Length; c < n; c++)
            {
                char a = spr.txt.txt[c];
                if (a == '|') // new line
                {
                    y_offset += (f.v_spacing) * spr.blit.scale.y;
                    x_offset = 0;
                }
                else if (a == '#') // DIRECTIVE
                {
                    char d = spr.txt.txt[c + 1];
                    if (d == 'C') // directive is COLOR
                    {
                        uint color = 0;
                        var param = find_directive_param(spr.txt.txt, c, out c);

                        if      (param == "0") color = spr.blit.color;
                        else if (param == "R") color = 0xff0000;
                        else if (param == "G") color = 0x00ff00;
                        else if (param == "B") color = 0x0000ff;
                        else if (param == "Y") color = 0xffff00;
                        else if (param == "P") color = 0xff00ff;
                        else if (param == "O") color = 0xff8000;

                        sfml_spr.Color = familiarize_color(0xff000000 | color);                        
                    }
                    else if (d == 'K') // Krest
                    {
                        var param = find_directive_param(spr.txt.txt, c, out c);
                        Crest crest =  Crest.find(param);                        
                        var crest_correction = new crds2(8f, 3f);
                        if (crest != null) crest.render(crest_correction + new crds2(spr.coords.x + x_offset, spr.coords.y + y_offset), (1f / 255f) * sfml_spr.Color.A);
                        x_offset += 24f;
                    }
                }                
                else // letter!
                {
                    uint sx = f.char_data[a].seq_x; uint sy = f.char_data[a].seq_y;

                    sfml_spr.Origin     = new Vector2f(0f, 0f);
                    sfml_spr.Position   = new Vector2f(spr.coords.x + x_offset, spr.coords.y + y_offset);
                    sfml_spr.SubRect    = new SFML.Graphics.IntRect(    (int)(t.size.x * sx),
                                                                        (int)(t.size.y * sy),
                                                                        (int)(t.size.x),// * (sx + 1)), 
                                                                        (int)(t.size.y));// * (sy + 1)));                    
                    render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);

                    x_offset += (f.char_data[a].width + f.h_spacing) * spr.blit.scale.x;                    
                }
            }
        }

        public static int text_width(BitmapFont font, string text)
        {
            float w = 0f;
            foreach (var c in text)
            {
                w += font.char_data[c].width;
            }            
            return (int)w;            
        }


    }
}
