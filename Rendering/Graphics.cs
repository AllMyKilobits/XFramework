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
        /////////////////////////////////////////////////////////////////////////////////////

        #region Lists and lookups
        public static List<Layer> layers;
        public static Dictionary<string, Texture> textures;
        public static Dictionary<string, Shader> shaders;
        public static Dictionary<string, BitmapFont> fonts; 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region States
        public static BitmapFont default_font;
        public static BitmapFont current_font; 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Screen height, width and calculations
        private static uint scrw;
        private static uint scrh;
        private static bool screenshot_signal;
        private static bool _fullscreen;
        //private static Shader empty_shader;

        public static uint screen_w { get { return scrw; } set { scrw = value; } }
        public static uint screen_h { get { return scrh; } set { scrh = value; } }

        public static float scrW2 { get { return (float)(screen_w / 2); } }
        public static float scrH2 { get { return (float)(screen_h / 2); } }
        public static bool fullscreen { get { return _fullscreen; } }

        public static uint desktop_w { get { return SFML.Window.VideoMode.DesktopMode.Width; } }
        public static uint desktop_h { get { return SFML.Window.VideoMode.DesktopMode.Height; } }

        private static uint last_windowed_h = 0;
        private static uint last_windowed_w = 0; 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Constructor
        static Graphics()
        {
            layers = new List<Layer>();
            textures = new Dictionary<string, Texture>();
            fonts = new Dictionary<string, BitmapFont>();
            shaders = new Dictionary<string, Shader>();

        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Setting a window
        private static RenderWindow win;
        public static uint back_color;
        public static void set_window(uint w = 0, uint h = 0, bool fullscreen = false)
        {
            if (w == 0 || h == 0)
            {
                if (fullscreen)
                {
                    w = desktop_w;
                    h = desktop_h;
                }
                else
                {
                    w = last_windowed_w;
                    h = last_windowed_h;
                    if (w == 0) w = desktop_w;
                    if (h == 0) h = desktop_h;
                    if (w > desktop_w) w = desktop_w;
                    if (h > desktop_h) h = desktop_h;
                }
            }

            _fullscreen = fullscreen;

            if (!fullscreen)
            {
                last_windowed_h = h; last_windowed_w = w;
            }   

            Debug.Log("Setting window to: " + w + " x " + h + (fullscreen ? " FULLSCREEN " : ""), Debug.priorities.normal);

            if (win != null) win.Close();

            screen_w = w;
            screen_h = h;

            Pipeline.main_window.force_resize(w, h);

            Styles style = fullscreen ?
                Styles.Fullscreen
                :
                Styles.Titlebar | Styles.Close;

            win = new RenderWindow(new SFML.Window.VideoMode(w, h), Application.title, style);

            win.Closed += new EventHandler(Session.on_close_signal);
            win.KeyPressed += new EventHandler<KeyEventArgs>(Input.Events.on_key_press);
            win.KeyReleased += new EventHandler<KeyEventArgs>(Input.Events.on_key_depress);
            win.MouseMoved += new EventHandler<MouseMoveEventArgs>(Input.Events.on_mouse_move);
            win.MouseWheelMoved += new EventHandler<MouseWheelEventArgs>(Input.Events.on_mouse_wheel);
            win.MouseButtonPressed += new EventHandler<MouseButtonEventArgs>(Input.Events.on_key_press);
            win.MouseButtonReleased += new EventHandler<MouseButtonEventArgs>(Input.Events.on_key_depress);

            Graphics.Pipeline.on_resolution_change();

            Debug.Log("Window set!", Debug.priorities.normal);

            if (_selected_icon != null)
                win.SetIcon(64, 64, _selected_icon.image.Pixels);
        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Icon stuff
        static private Texture _selected_icon;
        public static void select_icon(Texture t) { _selected_icon = t; if (win != null) win.SetIcon(64, 64, _selected_icon.image.Pixels); } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Logic

        public static uint frame_index { get; set; }

        public static void tick()
        {
            if (win != null)
            {
                Input.Mouse.dx = 0;
                Input.Mouse.dy = 0;
                Input.Mouse.wheel = 0;
                win.DispatchEvents();
            }

            if (Input.key(Input.Keys.P).presstime == 1)
            {
                if (Input.ctrl)
                {
                    screenshot_signal = true;
                }
            }


            if (Input.alt && Input.key(Input.Keys.Enter).pressed)
            {
                set_window(fullscreen: !Graphics.fullscreen);
            }

            if (reload_queue.Count > 0)
            {
                _reload_counter++;
                if (_reload_counter >= 10)
                {
                    _reload_counter = 0;
                    reload_queue.Dequeue().reload();
                }
            }
        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Reload management
        static private int _reload_counter = 0;
        static public void enqueue_texture_reload(Texture t) { if (!reload_queue.Contains(t)) reload_queue.Enqueue(t);}
        static private Queue<Texture> reload_queue = new Queue<Texture>();        
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Color conversions
        public static uint lerp_colors(uint color1, uint color2, float amount)
        {
            uint a1 = ((color1 & 0xff000000) >> 24);
            uint r1 = ((color1 & 0x00ff0000) >> 16);
            uint g1 = ((color1 & 0x0000ff00) >> 8);
            uint b1 = ((color1 & 0x000000ff) >> 0);

            uint a2 = ((color2 & 0xff000000) >> 24);
            uint r2 = ((color2 & 0x00ff0000) >> 16);
            uint g2 = ((color2 & 0x0000ff00) >> 8);
            uint b2 = ((color2 & 0x000000ff) >> 0);

            int a3 = (int)a1 + (int)(((int)a2 - (int)a1) * amount);
            int r3 = (int)r1 + (int)(((int)r2 - (int)r1) * amount);
            int g3 = (int)g1 + (int)(((int)g2 - (int)g1) * amount);
            int b3 = (int)b1 + (int)(((int)b2 - (int)b1) * amount);

            return (uint)(
                ((byte)a3 << 24) |
                ((byte)r3 << 16) |
                ((byte)g3 << 8) |
                ((byte)b3 << 0)
            );
        }

        public static crds4 transform_color(uint color)
        {
            uint a1 = ((color & 0xff000000) >> 24);
            uint r1 = ((color & 0x00ff0000) >> 16);
            uint g1 = ((color & 0x0000ff00) >> 8);
            uint b1 = ((color & 0x000000ff) >> 0);
            float z = 1f / 255;
            return new crds4
            (
                z * r1,
                z * g1,
                z * b1,
                z * a1
            );

        }
        public static uint transform_color(float r, float g, float b, float a)
        {
            return ((uint)XMath.floor(255f * r.choke01()) << 16) |
                   ((uint)XMath.floor(255f * g.choke01()) << 8 ) |
                   ((uint)XMath.floor(255f * b.choke01()) << 0 ) |
                   ((uint)XMath.floor(255f * a.choke01()) << 24);
        }

        private static Color familiarize_color(uint color)
        {
            return new Color(
                    (byte)((color & 0x00ff0000) >> 16),
                    (byte)((color & 0x0000ff00) >> 8),
                    (byte)((color & 0x000000ff) >> 0),
                    (byte)((color & 0xff000000) >> 24)
                );


        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

    }
}
