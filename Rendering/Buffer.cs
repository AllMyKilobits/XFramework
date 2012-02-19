using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SFML.Graphics;

namespace XF
{
    static partial class Graphics
    {
        /// <summary>
        /// SCREEN BUFFER - a render target.
        /// </summary>
        
        public class ScreenBuffer
        {
            internal bool is_main_window;
            public uint w { get; internal set; }
            public uint h { get; internal set; }
            internal float scaler;
            internal  bool autoscales;

            private SFML.Graphics.RenderImage _image;

            internal ScreenBuffer(bool iswindow, uint w, uint h)
            {   
                is_main_window = iswindow;
                
                this.w = w;
                this.h = h;
                if (!is_main_window)
                {
                    var z = Graphics.Pipeline.print_buffers;
                    _image = new RenderImage(w, h);
                }

                autoscales = false;
            }

            internal ScreenBuffer(float scaler)
            {
                this.is_main_window = false;
                this.scaler = scaler;
                this.autoscales = true;
                this.w = (uint)(screen_w * scaler);
                this.h = (uint)(screen_h * scaler);
                _image = new RenderImage(w, h);
            }   

            internal RenderTarget render_image { get 
            {
                if (is_main_window) return win;
                return _image;
            }}


            internal Image get_image { get {
                return _image.Image;
            }}

            internal void on_resize()
            {
                if (autoscales)
                {
                    w = (uint)(screen_w * scaler);
                    h = (uint)(screen_h * scaler);
                    _image = new RenderImage(w,h);                    
                }
            }

            public static implicit operator ScreenBuffer(string name) { return Pipeline.get_buffer(name); }

            public void force_resize(uint w, uint h)
            {
                this.w = w;
                this.h = h;
                _image = new RenderImage(w, h);                
            }

            internal Layer dedicated_layer; // usually at null; 

            public void dismiss()
            {
                if (_image != null)
                {   
                    _image.Dispose();
                    //_image = null;
                }
                this.w = 0;
                this.h = 0;
            }

            public bool export_to_png(string full_path)
            {
                if (_image == null || _image.Image == null) { Debug.Log("Attempting to export a non-created buffer"); }
                return _image.Image.SaveToFile(full_path);
            }   
        }
    }
}
