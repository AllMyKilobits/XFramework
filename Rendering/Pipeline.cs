using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XF
{
    static partial class Graphics
    {
        public static ScreenBuffer buffer(string id, uint w = 0, uint h = 0)
        {
            var buff = Pipeline.get_buffer(id);
            if (buff == null) {buff = Pipeline.create_fixed_size_buffer(id, w, h);}
            if (w > 0) if (buff.w != w || buff.h != h) buff.force_resize(w, h);            
            return buff;
        }

        public static class Pipeline // you could feasibly have more than one pipeline in which case de-static this.
        {
            public enum Directives
            {
                none,
                blit,           // blits sphere_center buffer to destination buffer.
                render_layers,  // renders layers onto destination buffer.
                clear_buffer,   // clears sphere_center buffer
                other
            }

            public static ScreenBuffer create_fixed_size_buffer(string identifier, uint w, uint h)
            {
                ScreenBuffer new_buffer = new ScreenBuffer(false, w, h);
                buffers.Add(identifier, new_buffer);
                return new_buffer;
            }

            public static ScreenBuffer create_autoscaling_buffer(string identifier, float scale)
            {                
                ScreenBuffer new_buffer = new ScreenBuffer(scale);
                buffers.Add(identifier, new_buffer);
                return new_buffer;
            }

            public static ScreenBuffer get_buffer(string identifier)
            {
                ScreenBuffer b;
                buffers.TryGetValue(identifier, out b);
                return b;
            }

            public static bool destroy_buffer(string identifier)
            {
                ScreenBuffer b;
                if (!buffers.TryGetValue(identifier, out b)) return false;
                buffers.Remove(identifier);                
                return true;
            }

            public static void on_resolution_change()
            {
                foreach (var buffer in buffers)                
                    if (buffer.Value.autoscales)                    
                        buffer.Value.on_resize();
            }

            
            private static List         <Directive>             program;            
            public  static ScreenBuffer                         main_window {get; private set;}
            private static Dictionary<String, ScreenBuffer>     buffers;

            public static string print_buffers { get {
                var sb = new System.Text.StringBuilder("Buffer report : |");
                sb.Append("total of " + buffers.Count + " buffers |");
                var total_pixels = 0;
                foreach (var buffer in buffers) { //sb.Append(buffer.Key); sb.Append(": "); sb.Append(buffer.Value.w.ToString() + "x" + buffer.Value.h.ToString()); sb.Append(" |");
                total_pixels += (int)(buffer.Value.w * buffer.Value.h); };
                sb.Append("Total : " + ((total_pixels * 4) / 1024).ToString() + "k in buffers");
                
                return sb.ToString();                
            }}
         
            

            //////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary> DIRECTIVE class - an integral part of the pipeline! </summary>
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            public class Directive
            {
                private Directives      do_what;
                private ScreenBuffer    source_buffer;
                private ScreenBuffer    target_buffer;
                private List<Layer>     layers;                
                private EffectInstance  effect;
                private uint            color;

                public Directive(Directives wat, ScreenBuffer source, ScreenBuffer target, List<Layer> layers, EffectInstance effect = null, uint color = 0xff000000)
                {
                    this.do_what        = wat;
                    this.source_buffer  = source;
                    this.target_buffer  = target;
                    this.layers         = layers;
                    this.color          = color;
                    this.effect         = effect;                    
                    if (this.effect == null) this.effect = EffectInstance.empty;
                }

                public void include_layer(Layer layer) { if (layers == null) layers = new List<Layer>(); layers.Add(layer); }

                public void execute()
                {
                    switch (do_what)
                    {   
                        case Directives.render_layers:
                        {
                            Graphics.render_target = target_buffer.render_image;

                            foreach (var layer in layers)
                            {
                                for (uint s = 0; s < layer.current_sprite_id; s++)
                                {
                                    var spr = layer.sprites[s];
                                    if      (spr.kind == Sprite.draw_mode.spr) render_sprite(spr);
                                    else if (spr.kind == Sprite.draw_mode.text) render_text(spr); 
                                    else if (spr.kind == Sprite.draw_mode.poly) render_poly(spr);
                                }
                            }
                            if (Graphics.render_target != win) (Graphics.render_target as SFML.Graphics.RenderImage).Display();
                        } break;

                        case Directives.blit:
                        {
                            Graphics.render_buffer(source_buffer, target_buffer, effect);  
                        } break;

                        case Directives.clear_buffer:
                        {
                            target_buffer.render_image.Clear(familiarize_color(color));
                        } break;

                    } // end switch 
                    
                } // end execute


                public bool temporary { get; set; }                
            } // end class Directive

            //////////////////////////////////////////////////////////////////////////////////////////////////////

            public static void default_program()
            {
                program.Clear();
                add_directive(Directives.render_layers, Pipeline.main_window, Graphics.layers, null);
            }

            static Pipeline()
            {
                program = new List<Directive>();
                buffers = new Dictionary<string, ScreenBuffer>();
                main_window = new ScreenBuffer(true, screen_w, screen_h);
                main_window.autoscales = true;
                main_window.scaler = 1f;
               
                default_program();                
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////

            public static Directive add_directive(Directives which_directive, ScreenBuffer target_buffer = null, List<Layer> layers = null, ScreenBuffer source_buffer = null, EffectInstance shader = null, uint color = 0xff000000)
            {
                Directive D = new Directive(which_directive, source_buffer, target_buffer, layers, shader, color);
                program.Add(D);
                return D;
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////

            public static void clear() // clears the pipeline;
            {
                Pipeline.program.Clear();
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////

            public static void execute()
            {
                foreach (Directive D in program) D.execute(); 
                for (int i = program.Count - 1; i >= 0; i--) if (program[i].temporary) program.RemoveAt(i); // remove TEMPORARY directives                

                foreach (Layer layer in Graphics.layers)
                {                      
                    layer.reset();  // having dumped the sprites, it's only natural to reset it
                    // it MIGHT be feasible to have a flag disabling this reset in the future for some shader purposes.
                }

            }


        }
    }

}
