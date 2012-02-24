using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;

namespace XF
{
    static partial class Graphics
    {
        /////////////////////////////////////////////////////////////////////////////////////

        public enum blend_mode
        {
            normal = 0,
            additive = 1
        }

        /////////////////////////////////////////////////////////////////////////////////////

        #region Pipeline Executions and States

        public static Layer current_layer { get; set; }
        static internal RenderTarget render_target;

        public static void execute() // executes the pipeline
        {
            if (win == null) return;

            Graphics.frame_index++;

            render_target = win;

            win.Clear(familiarize_color(back_color));

            Statistics.sprites_drawn = 0;
            Statistics.sprites_discarded = 0;

            Pipeline.execute();

            win.Display();

            if (screenshot_signal)
            {
                bool found = false; int ctr = 0; string path = "";
                while (!found)
                {
                    path = XF.Application.root_path +  "screenshot" + ctr + ".png";
                    found = !System.IO.File.Exists(path);
                    ctr++;
                }

                SFML.Graphics.Image screenshot = new SFML.Graphics.Image(screen_w, screen_h);
                screenshot.CopyScreen(win) ;
                screenshot.SaveToFile(path);

                screenshot_signal = false;
            }

        } 

        #endregion        
        
        /////////////////////////////////////////////////////////////////////////////////////

        #region RENDERING ALGORITHMS

        // We need a single SFML sprite object:
        private static SFML.Graphics.Sprite sfml_spr = new SFML.Graphics.Sprite();
        private static SFML.Graphics.Shape sfml_shape = new SFML.Graphics.Shape();

        private static void render_buffer(ScreenBuffer source_buffer, ScreenBuffer target_buffer, EffectInstance effect)
        {
            var img = source_buffer.render_image as SFML.Graphics.RenderImage;
            if (img != null)
            {
                SFML.Graphics.Sprite sfml_spr = new SFML.Graphics.Sprite(img.Image);

                //sfml_spr.FlipY(true); 
                sfml_spr.Scale = new Vector2f((float)screen_w / source_buffer.w, (float)screen_h / source_buffer.h);

                

                effect.prepare();
                var so = effect.shader.sfml_shader_object;
                if (so == null) target_buffer.render_image.Draw(sfml_spr);
                else            target_buffer.render_image.Draw(sfml_spr, so);
            }
        }

        private static void render_poly(Sprite spr)
        {
            sfml_shape = new Shape();
            sfml_shape.BlendMode = (BlendMode)spr.blit.mode;            
            var col = familiarize_color(spr.blit.color);
            sfml_shape.Color = SFML.Graphics.Color.White; 
            for (int i = 0, n = spr.points.Length; i < n; i++)
            {
                sfml_shape.AddPoint(new Vector2f(spr.points[i].x, spr.points[i].y), col);
            }
            spr.effect.prepare();

            //var rt = spr.render_target_override ?? render_target;

            render_target.Draw(sfml_shape, spr.effect.shader.sfml_shader_object);
        }

        private static void render_sprite(Sprite spr)
        {
            Texture tex  = spr.blit.t;
            var     buff = spr.blit.buffer;
            if (tex != null && tex.loading_in_progress) return;
            if (tex == null && buff == null) return;

            uint sizex = 0, sizey = 0;
            if (tex != null) { sizex = tex.size.x; sizey = tex.size.y; }
            else if (buff != null) { sizex = buff.w; sizey = buff.h; }

            float k = Utility.max(spr.blit.scale.x * sizex, spr.blit.scale.y * sizey);
            float mw = k * sizex; float mh = k * sizey;

            Statistics.sprites_discarded++;

            if ((spr.coords.x < -mw) || (spr.coords.x > mw + screen_w)) return;
            if ((spr.coords.y < -mh) || (spr.coords.y > mh + screen_h)) return;

            Statistics.sprites_drawn++;
            Statistics.sprites_discarded--;

            if (tex != null) sfml_spr.Image = tex.image;
            else
            {
                sfml_spr.Image = (buff.render_image as SFML.Graphics.RenderImage).Image;
            }
            
            sfml_spr.BlendMode = (BlendMode)spr.blit.mode;
            sfml_spr.Color = familiarize_color(spr.blit.color);
            sfml_spr.Rotation = spr.blit.rotation;

            if (tex!=null && tex.margins > 0)
            {
                #region MARGIN ALGORITHM

                // ul corner

                var x = spr.coords.x - spr.blit.scale.x * tex.size.x / 2;
                var y = spr.coords.y - spr.blit.scale.y * tex.size.y / 2;
                var w = spr.blit.scale.x * tex.size.x;
                var h = spr.blit.scale.y * tex.size.y;
                var sx = (int)tex.size.x; var sy = (int)tex.size.y;
                var m = tex.margins;
                var fx = (w - 2 * m) / (sx - 2 * m);
                var fy = (h - 2 * m) / (sy - 2 * m);

                sfml_spr.Scale = new Vector2f(1f, 1f);
                sfml_spr.Origin = new Vector2f(0f, 0f);

                spr.effect.prepare();

                sfml_spr.Scale = new Vector2f(1f, 1f); // for corners

                #region A bunch of footwork you don'tex need to look at, really

                var fill = true;
                if (fill)
                {
                    #region Fill algorithm
                    // Upper left corner                
                    sfml_spr.Position = new Vector2f(x, y);
                    sfml_spr.SubRect = new SFML.Graphics.IntRect(0, 0, m, m);
                    render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);
                    // Upper right corner:
                    sfml_spr.Position = new Vector2f(x + w - m, y);
                    sfml_spr.SubRect = new SFML.Graphics.IntRect(sx - m, 0, m, m);
                    render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);
                    // lower left corner:
                    sfml_spr.Position = new Vector2f(x, y + h - m);
                    sfml_spr.SubRect = new SFML.Graphics.IntRect(0, sy - m, m, m);
                    render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);
                    // lower right corner:
                    sfml_spr.Position = new Vector2f(x + w - m, y + h - m);
                    sfml_spr.SubRect = new SFML.Graphics.IntRect(sx - m, sy - m, m, m);
                    render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);

                    // upper bar:
                    sfml_spr.Position = new Vector2f(x + m, y);
                    sfml_spr.SubRect = new SFML.Graphics.IntRect(m, 0, sx - 2 * m, m);
                    sfml_spr.Scale = new Vector2f(fx, 1f);
                    render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);
                    // lower bar:
                    sfml_spr.Position = new Vector2f(x + m, y + h - m);
                    sfml_spr.SubRect = new SFML.Graphics.IntRect(m, sy - m, sx - 2 * m, m);
                    sfml_spr.Scale = new Vector2f(fx, 1f);
                    render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);
                    // left bar:
                    sfml_spr.Position = new Vector2f(x, y + m);
                    sfml_spr.SubRect = new SFML.Graphics.IntRect(0, m, m, sy - 2 * m);
                    sfml_spr.Scale = new Vector2f(1f, fy);
                    render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);
                    // right bar:
                    sfml_spr.Position = new Vector2f(x + w - m, y + m);
                    sfml_spr.SubRect = new SFML.Graphics.IntRect(sx - m, m, m, sy - 2 * m);
                    sfml_spr.Scale = new Vector2f(1f, fy);
                    render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);

                    // fill : 
                    sfml_spr.Position = new Vector2f(x + m, y + m);
                    sfml_spr.SubRect = new SFML.Graphics.IntRect(m, m, sx - 2 * m, sy - 2 * m);
                    sfml_spr.Scale = new Vector2f(fx, fy);
                    render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object); 
                    #endregion
                }
                else
                {
                    #region Tile algorithm

                    sfml_spr.Position = new Vector2f(x, y);
                    sfml_spr.SubRect = new SFML.Graphics.IntRect(0, 0, m, m);
                    render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);
                    // Upper right corner:
                    sfml_spr.Position = new Vector2f(x + w - m, y);
                    sfml_spr.SubRect = new SFML.Graphics.IntRect(sx - m, 0, m, m);
                    render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);
                    // lower left corner:
                    sfml_spr.Position = new Vector2f(x, y + h - m);
                    sfml_spr.SubRect = new SFML.Graphics.IntRect(0, sy - m, m, m);
                    render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);
                    // lower right corner:
                    sfml_spr.Position = new Vector2f(x + w - m, y + h - m);
                    sfml_spr.SubRect = new SFML.Graphics.IntRect(sx - m, sy - m, m, m);
                    render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);

                    for (float xx = m; xx < w - m; xx += sx - 2 * m)
                    {
                        int g = sx - 2 * m;
                        if (xx + g > w - m) g = (int)(w - m - xx);

                        sfml_spr.Position = new Vector2f(x + xx, y);                        
                        sfml_spr.SubRect = new IntRect(m, 0, g, m);
                        render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);

                        sfml_spr.Position = new Vector2f(x + xx, y+h-m);
                        sfml_spr.SubRect = new IntRect(m, (int)sy-m, g, m);
                        render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);
                    }

                    for (float yy = m; yy < h -  m; yy += sy - 2 * m)
                    {
                        int f = sy - 2 * m;
                        if (yy + f > h - m) 
                            f = (int)(h - m - yy);

                        sfml_spr.Position = new Vector2f(x, y + yy);
                        sfml_spr.SubRect = new IntRect(0, m, m, f);
                        render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);

                        sfml_spr.Position = new Vector2f(x + w - m, y + yy);
                        sfml_spr.SubRect = new IntRect((int)sx-m, m, m, f);
                        render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);
                    }

                    for (float xx = m; xx < w - m; xx += sx - 2 * m)
                    for (float yy = m; yy < h - m; yy += sy - 2 * m)
                    {
                        int g = sx - 2 * m;
                        if (xx + g > w - m) g = (int)(w - m - xx);
                        int f = sy - 2 * m;
                        if (yy + f > h - m) f = (int)(h - m - yy);

                        sfml_spr.Position = new Vector2f(x + xx, y + yy);
                        sfml_spr.SubRect = new IntRect(m, m, g, f);
                        render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);

                    }

                    
                    #endregion

                    

                    //var is = new crds2(
                    // one - to - one 
                    
                }

                #endregion

                #endregion
            }
            else
            {
                #region STANDARD ALGORITHM

                sfml_spr.Position = new Vector2f(spr.coords.x, spr.coords.y);
                sfml_spr.Scale = new Vector2f(spr.blit.scale.x, spr.blit.scale.y);

                uint sequence_x = spr.blit.sequence.x;
                uint sequence_y = spr.blit.sequence.y;

                //sfml_spr.FlipY(tex == null);
                
                if (spr.blit._use_special_margins)
                {
                    //sizex = (uint)(sizex * spr.blit.special_margins[2]);
                    //sizey = (uint)(sizey * spr.blit.special_margins[3]);

                    sfml_spr.Origin = new Vector2f(0.5f * sizex
                                                        * spr.blit.special_margins[2], 
                                                   0.5f * sizey 
                                                        * spr.blit.special_margins[3]);

                    sfml_spr.SubRect = new SFML.Graphics.IntRect((int)(sizex * (sequence_x + spr.blit.special_margins[0])),
                                                                 (int)(sizey * (sequence_y + spr.blit.special_margins[1])),
                                                                 (int)(sizex * spr.blit.special_margins[2]),
                                                                 (int)(sizey * spr.blit.special_margins[3]));
                }  else  
                {
                    sfml_spr.Origin = new Vector2f(0.5f * sizex, 0.5f * sizey);
                    sfml_spr.SubRect = new SFML.Graphics.IntRect((int)(sizex * sequence_x),(int)(sizey * sequence_y), (int)(sizex),(int)(sizey));
                }
                spr.effect.prepare();

                render_target.Draw(sfml_spr, spr.effect.shader.sfml_shader_object);

                #endregion
            }

        }
                
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region ADDING ALGORITHMS

        public static Sprite last_sprite { get; private set; }

        #region Add Line
        public static Sprite add_line(Texture t, float x0, float y0, float x1, float y1, float w) 
        {
            Sprite spr = current_layer.new_sprite;
            spr.coords.set((x0 + x1) / 2f, (y0 + y1) / 2f);
            spr.blit.t = t;
            float d = (float)Math.Sqrt((x0 - x1) * (x0 - x1) + (y0 - y1) * (y0 - y1));
            if (d <= 0.5f) d = 0.5f;
            var h = w / t.size.y;            
            spr.set_scale(d / t.size.x, h);
            spr.blit.rotation = XMath.get_angle(x1 - x0, y1 - y0);
            spr.kind = Sprite.draw_mode.spr;
            last_sprite = spr;            
            return spr;
        }
        #endregion

        static public Sprite[] add_capsule(Texture t, float x0, float y0, float x1, float y1, float w) // capsule extends the line outwards from the points
        {
            var center = current_layer.new_sprite;
            var left = current_layer.new_sprite;
            var right = current_layer.new_sprite;

            var array = new Sprite[] { left, center, right };

            var c = new crds2((x0 + x1) / 2, (y0 + y1) / 2);
            var d = (float)Math.Sqrt((x0 - x1) * (x0 - x1) + (y0 - y1) * (y0 - y1)); if (d <= 0.5f) d = 0.5f;
            var h = w / t.size.y;
            var aspect = (float)t.size.x / t.size.y;
            var invaspect = 1f / aspect;
            var v = invaspect / 2;
            var rot_angle = XMath.get_angle(x1 - x0, y1 - y0);
            var d1 = crds2.deflect(c, new crds2(x0, y0), w / 4f);
            var d2 = crds2.deflect(c, new crds2(x1, y1), w / 4f);

            var scalex = d / t.size.x;

            center.coords.set(c.x, c.y);
            center.set_scale(scalex, h);
            center.blit.set_special_margins(v, 0f, 1f - v * 2, 1f, true);

            left.coords.set(d1.x, d1.y);            
            left.blit.set_special_margins(0f, 0f, v, 1f, true);
            left.set_scale(h, h);

            right.coords.set(d2.x, d2.y);
            right.blit.set_special_margins(1f - v, 0f, v, 1f, true);
            right.set_scale(h, h);
            
            foreach (var spr in array) {                
                spr.blit.t = t;
                spr.blit.rotation = rot_angle;
                spr.kind = Sprite.draw_mode.spr;
            }
            //left.set_colors(0xff0000, 0.5f);
            //center.set_colors(0x00ff00, 0.5f);
            //right.set_colors(0x0000ff, 0.5f);
            last_sprite = right;
            return array;
            
        }

        #region Add sprite

        public static Sprite add_offscreen_sprite(ScreenBuffer buffer, float x, float y)
        {
            Sprite spr = current_layer.new_sprite;
            spr.coords.set(x, y);
            spr.blit.buffer = buffer;
            spr.blit.t = null;
            spr.kind = Sprite.draw_mode.spr;
            last_sprite = spr;
            return spr;
        }

        public static Sprite add_sprite(Texture t, float x, float y)
        {
            Sprite spr = current_layer.new_sprite;
            spr.coords.set(x, y);
            spr.blit.t = t;
            spr.kind = Sprite.draw_mode.spr;
            last_sprite = spr;
            return spr;
        }

        public static Sprite add_sprite(BlitData b, float x, float y)
        {
            Sprite spr = current_layer.new_sprite;

            spr.coords.set(x, y);            

            spr.blit.mode       = b.mode;
            spr.blit.color      = b.color;
            spr.blit.rotation   = b.rotation;
            spr.blit.scale      = b.scale;
            spr.blit.sequence   = b.sequence;
            spr.blit.t          = b.t;

            spr.kind    = Sprite.draw_mode.spr;
            last_sprite = spr;

            return spr;
        }

        #endregion

        #region Add Shape

        public static Sprite add_shape(crds2[] coords, uint color = 0xffffffffu)
        {
            Sprite spr = current_layer.new_sprite;
            spr.coords.set(0f, 0f);
            spr.kind = Sprite.draw_mode.poly;
            spr.blit.color = color;
            spr.points = coords;
            last_sprite = spr;
            return spr;
        }
        #endregion

        #region Add Text
        public static Sprite add_text(float x, float y, string text)
        {
            Sprite spr = current_layer.new_sprite;
            spr.coords.set(x, y);
            spr.txt.txt = text;
            spr.txt.font_id = Graphics.current_font;
            spr.kind = Sprite.draw_mode.text;
            spr.txt.max_w = -1.0f;            
            spr.txt.additional_spacing = 0f;
            last_sprite = spr;
            return spr;
        }

        #endregion 

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Adding sprites in batches
        public static Sprite[] tile_sprites(Texture t, float x0, float y0, float w, float h, float scale_x, float scale_y)
        {
            List<Sprite> list = new List<Sprite>();

            float w1 = scale_x * t.size.x;
            float h1 = scale_y * t.size.y;

            for (float cy = y0; cy <= y0 + h; cy += h1)
                for (float cx = x0; cx <= x0 + w; cx += w1)
                {
                    var spr = add_sprite(t, cx, cy);
                    spr.set_scale(scale_x, scale_y);
                    spr.fit_rect(cx, cy, w1, h1, 1f);
                    list.Add(spr);
                }

            return list.ToArray();
        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////
    }
}
