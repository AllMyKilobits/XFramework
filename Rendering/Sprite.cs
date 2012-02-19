using System;
using System.Collections.Generic;
using XF;

namespace XF
{
    public static partial class Graphics
    {       
        public class BlitData
        {
            /// <summary> how - how the texture will be blended </summary>
            public blend_mode           mode;

            /// <summary> tex : Texture to be rendered</summary>
            public Texture         t;
            public ScreenBuffer    buffer;

            public D2<uint>  sequence;
            public D2<float> scale;
            public float           rotation;            
            public uint            color;

            internal float[]  special_margins;
            internal bool _use_special_margins;

            public float alpha
            {
                get { return (float)((color & 0xff000000) >> 24) / 255.0f; }
                set {    value = value.choke(0.0f, 1.0f);
                         color = (color & 0x00ffffff) | (uint)((byte)(value * 255) << 24); // wat!
                }
            }

            public BlitData()
            {
                reset();
            }
            public void reset()
            {
                mode = blend_mode.normal;
                sequence.set(0, 0);
                scale.set(1.0f, 1.0f);
                color = 0xffffffff;
                rotation = 0.0f;
                t = null;
                buffer = null;
                _use_special_margins = false;
            }
            public void set_special_margins(float x0, float y0, float w, float h, bool expand)
            {
                _use_special_margins = true;
                special_margins = new float[] { x0, y0, w, h };

                if (expand) 
                {
                    scale.x /= w;
                    scale.y /= h;
                }

            }

        }
        public class Sprite
        {
            ///////////////////////////////////////////////////////////////////////////////////////
            
            #region Declarations
            public enum draw_mode
            {
                none,
                spr,
                text,
                poly
            }

            public class TxtData
            {
                public string txt;
                public float max_w;
                public BitmapFont font_id;
                public float additional_spacing; // for SPACE character only; used in justification algorithms
            } 
            #endregion
            
            ///////////////////////////////////////////////////////////////////////////////////////

            #region Public members
            public D2<float> coords;
            public BlitData blit = new BlitData();
            public TxtData txt = new TxtData();
            public draw_mode kind = draw_mode.spr;
            public crds2[] points;
            #endregion

            ///////////////////////////////////////////////////////////////////////////////////////

            #region Methods
            public void fit_rect(float x0, float y0, float w, float h, float superscale = 1f)
            {
                coords.x = x0 + w * 0.5f;
                coords.y = y0 + h * 0.5f;
                blit.scale.x = superscale * w / blit.t.size.x;
                blit.scale.y = superscale * h / blit.t.size.y;
                blit.rotation = 0f;
            }

            public crds2 upper_left
            {
                get
                {
                    return new crds2(coords.x - blit.scale.x * blit.t.size.x / 2,
                                     coords.y - blit.scale.y * blit.t.size.y / 2);
                }
            }
            public crds2 lower_right
            {
                get
                {
                    return new crds2(coords.x + blit.scale.x * blit.t.size.x / 2,
                                     coords.y + blit.scale.y * blit.t.size.y / 2);
                }
            }

            public void set_scale(float x, float y)
            {
                blit.scale.set(x, y);
            }
            public void set_scale(float scale)
            {
                blit.scale.set(scale, scale);
            }

            public void set_colors(uint color, float alpha)
            {
                blit.color = color;
                blit.alpha = alpha;
            }
            public void set_color(uint color)
            {
                blit.color = color;
                if ((color & 0xff000000) << 24 == 0) // most likely we only wish to set the color component, not the alpha component
                {
                    set_alpha(1.0f);
                }
            }
            public void set_alpha(float alpha)
            {
                blit.alpha = alpha;
            }

            public void set_sequence(float x, float y)
            {
                uint x1 = (uint)(x * blit.t.sequence.x);
                uint y1 = (uint)(y * blit.t.sequence.y);
                blit.sequence.set(x1, y1);
            }

            public void set_color(byte red, byte green, byte blue, byte alpha = 255)
            {
                blit.color = (uint)((alpha << 24) | (red << 16) | (green << 8) | (blue << 0));
            }

            public void set_color(float red, float green, float blue, float alpha = 1f)
            {
                blit.color = (uint)(
                              ((byte)(alpha.choke(0f, 1f) * 255) << 24) |
                              ((byte)(red.choke(0f, 1f) * 255) << 16) |
                              ((byte)(green.choke(0f, 1f) * 255) << 8) |
                              ((byte)(blue.choke(0f, 1f) * 255) << 0));
            }

            public int text_width
            {
                get
                {
                    if (this.kind != draw_mode.text) return -1;

                    float w = 0f;
                    for (int c = 0; c < txt.txt.Length; c++)
                    {
                        w += txt.font_id.char_data[txt.txt[c]].width;
                    }
                    return (int)w;
                }
            }

            public EffectInstance effect = new EffectInstance(); 
            #endregion

            ///////////////////////////////////////////////////////////////////////////////////////
            
        }

        
    }    
}