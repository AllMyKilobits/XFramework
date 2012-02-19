using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SFML.Graphics;
using SFML.Window;

namespace XF
{
    static partial class Graphics
    {
        public class Texture
        {
            //string          file_path;     // if empty it is dynamic
            public uint     mask_color;    // defaults to black
            public D2<uint> size_full;     //
            public D2<uint> sequence;      //
            public D2<uint> size;          // size of a single sprite
            public int      margins;       // margin size, for when a sprite is drawn using margins.

            public bool     is_font;       

            public string   identifier;

            internal bool    manipulate_smooth;
            internal bool    manipulate_white_to_alpha;
            
            

            public SFML.Graphics.Image    image;          // implementation!
            //public SFML.Graphics.RenderImage 
            
            public Texture()
            {
                sequence.set(1, 1);
                size.set(0, 0);
                size_full.set(0, 0);
                mask_color = 0x00000000;
                manipulate_smooth         = false;
                manipulate_white_to_alpha = false;
            }

            public void generate(string unique_identifier, uint w, uint h, uint color)
            {
                image = new SFML.Graphics.Image(w, h, familiarize_color(color));
                size_full.set(w, h);
                set_sprite_grid(1, 1);
                identifier = unique_identifier;
                
            }
            /// <summary> Sets sprite sequence max </summary>
            /// <param name="x_num"> sprites in x axis</param>
            /// <param name="y_num"> sprites in y axis</param>
            public void set_sprite_grid(uint x_num, uint y_num)
            {
                sequence.set(x_num, y_num);
                size.set(size_full.x / x_num, size_full.y / y_num);                
            }
            
            public void assign_path(string str)
            {                
                _filepath = str;
                identifier = id_from_path(str);
                reload();
            }
            private bool   _loaded = false;
            private string _filepath = "";

            static public string id_from_path(string path)
            {
                return path.Substring(path.LastIndexOf('\\') + 1, path.LastIndexOf('.') - path.LastIndexOf('\\') - 1);
            }

            internal bool loading_in_progress {get; private set;}
                        
            internal  void reload()
            {
                loading_in_progress = true;
                var path = _filepath;

                if (path == "") 
                    throw new Exception("Trying to reload a texture that hasn't been properly loaded, derp");
                
                //Debug.Log("Loading texture from file... " + path);

                string ext = path.Substring(path.LastIndexOf('.') + 1);
                string def_file = path;
                def_file = def_file.Remove(def_file.LastIndexOf('.'));
                def_file += ".def";

                string alpha_file = path;
                alpha_file = alpha_file.Remove(alpha_file.LastIndexOf('.'));
                alpha_file += "_alpha." + ext;

                bool has_alpha_tex = System.IO.File.Exists(alpha_file);

                if (!_loaded)                
                if (System.IO.File.Exists(def_file))
                {
                    parse_graphics_def_file(this, def_file);                    
                }

                if (this.image != null) this.image.Bind();
                //this.render_image = null;
                this.image = new Image(path);
                size_full.set(image.Width, image.Height);
                set_sprite_grid(sequence.x, sequence.y);
                image.Smooth = manipulate_smooth;               
                _loaded = true;
                loading_in_progress = false;

            }

            public static implicit operator Texture(string str)
            {                   
                return Graphics.textures[str];
            }

            public bool export_to_png(string full_path)
            {
                return image.SaveToFile(full_path);
            }                        
        }
    }
}
