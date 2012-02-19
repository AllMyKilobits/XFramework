using System;
using SFK = SFML.Window.Keyboard.Key; // are you telling me you can set enum aliases like this? WTF!!!!

namespace XF
{
    static public class Input
    {
        /////////////////////////////////////////////////////////////////////////////////////

        #region Consts and definitions
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        //                      Key and axis definions
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum Keys
        {
            NoKey,

            A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
            n0, n1, n2, n3, n4, n5, n6, n7, n8, n9,

            Escape, Space, Enter, Tab,
            LCtrl, LShift, LAlt, LSystem,
            RCtrl, RShift, RAlt, RSystem,
            F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,

            Left, Right, Up, Down,

            Backspace, Delete, Insert,

            Mouse1, Mouse2, Mouse3, Mouse4, Mouse5, Mouse6, Mouse7,
            Joy1, Joy2, Joy3, Joy4, Joy5, Joy6, Joy7, Joy8,

            Count
        }

        public enum Axes
        {
            NoAxis,
            MouseX, MouseY, MouseWhl,
            Joy1, Joy2, Joy3, Joy4,
            Count
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        //				SOME BEHAVIOR CONSTS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        static private int repeat_time_early = 50;
        static private int repeat_time_late = 10;
        static private int repeats_before_latemode = 4; 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Public Methods
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //				METHODS
        //
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        static public void initialize()
        {

            Debug.Log("Initializing input...", Debug.priorities.low);

            key_data = new Key[(int)Keys.Count];
            for (Keys k = 0; k < Keys.Count; k++)
            {
                key_data[(int)k] = new Key();
            }
            conversion_table();
        }

        static public void terminate()
        {

        }

        static public void tick()
        {
            for (Keys k = Keys.NoKey; k < Keys.Count; k++) key_data[(int)k].tick();           

        }
        static public void on_frame_cleanup()
        {
            //Mouse.wheel = 0;
        }
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Mouse
        /// <summary>
        /// Mouse class - denotes, well, a mouse. Only one mouse is supported. (If you need more, you'redoingitwrong.)
        /// </summary>

        public static class Mouse
        {
            static public int x { get { return (int)coords.x; } }
            static public int y { get { return (int)coords.y; } }
            static public int dx;
            static public int dy;

            static public crds2 coords;

            static private int w; // field backing the wheel

            static public int wheel
            {
                get
                {
                    return w;
                    //int w1 = w; // temporary variable
                    //w = 0;      // reset wheel
                    //return w1;  // return wheel
                }
                set
                {
                    w = value;
                }
            }


            public static bool click { get { return Input.key(Keys.Mouse1).pressed; } }
        }  // end class Mouse 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Keys
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Key class - denotes a keyboard key or a game device button.
        /// </summary> 
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public class Key
        {
            private bool _pressed;
            private int _presstime;
            private int _release_time;
            public int presstime { get { return _presstime; } }
            public int release_time { get { return _release_time; } }

            /// <summary>
            /// returns whether or not a repeatable press event is generated this tick.
            /// </summary>
            public bool ticked
            {
                get
                {
                    if (_presstime == 0) return false;
                    if (_presstime == 1) return true;
                    int treshold = repeat_time_early * repeats_before_latemode;
                    if ((_presstime <= treshold) && (_presstime % repeat_time_early == 0)) return true;
                    else if ((_presstime - treshold) % repeat_time_late == 0) return true;
                    return false;
                }
            }
            /// <summary>
            /// Pressed - returns if the key has just been pressed.
            /// </summary>
            public bool pressed { get { return (_presstime == 1); } }

            public bool held { get { return (_presstime > 0); } }

            public bool released { get { return (_release_time == 1); } }
            public void on_press_start() { _pressed = true; }
            public void on_press_end() { _pressed = false; }
            public void tick()
            {
                if (_pressed)   { _presstime++; _release_time = 0; }
                else            { _presstime = 0; _release_time++; }
            }

        }

        static private Key[] key_data; 
        

        //				GETS - public key functions
        

        static public Key key(Keys index)
        {
            return key_data[(int)index];
        }

        static public bool alt   { get { return key(Keys.LAlt).held    || key(Keys.RAlt).held; } }
        static public bool ctrl  { get { return key(Keys.LCtrl).held   || key(Keys.RCtrl).held; } }
        static public bool shift { get { return key(Keys.LShift).held  || key(Keys.RShift).held; } }

        static private string _ichar = "";
        static public string input_char { get { var c = _ichar; _ichar = ""; return c; } }
        

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Conversion 
        static Keys[] conversion_array = new Keys[1024];

        static void conversion_table() // necessary to bind form events to conversion keys.
        {
            for (int i = 0; i < 1024; i++)
                conversion_array[i] = Keys.NoKey;

            for (SFK k = SFK.A; k <= SFK.Z; k++)
                conversion_array[(int)k] = (Keys)((int)Keys.A + (int)(k - SFK.A));

            for (SFK k = SFK.Num0; k <= SFK.Num9; k++)
                conversion_array[(int)k] = (Keys)((int)Keys.n0 + (int)(k - SFK.Num0));

            for (SFK k = SFK.F1; k <= SFK.F12; k++)
                conversion_array[(int)k] = (Keys)((int)Keys.F1 + (int)(k - SFK.F1));

            conversion_array[(int)SFK.Return] = Keys.Enter;
            conversion_array[(int)SFK.Space] = Keys.Space;
            conversion_array[(int)SFK.RAlt] = Keys.RAlt;
            conversion_array[(int)SFK.RControl] = Keys.RCtrl;
            conversion_array[(int)SFK.RShift] = Keys.RShift;
            conversion_array[(int)SFK.LAlt] = Keys.LAlt;
            conversion_array[(int)SFK.LControl] = Keys.LCtrl;
            conversion_array[(int)SFK.LShift] = Keys.LShift;
            conversion_array[(int)SFK.Tab] = Keys.Tab;
            conversion_array[(int)SFK.Left] = Keys.Left;
            conversion_array[(int)SFK.Right] = Keys.Right;
            conversion_array[(int)SFK.Up] = Keys.Up;
            conversion_array[(int)SFK.Down] = Keys.Down;
            conversion_array[(int)SFK.Back] = Keys.Backspace;
            conversion_array[(int)SFK.Delete] = Keys.Delete;

        }

        static public Keys decode(SFK input)
        {
            return conversion_array[(int)input];
        }         
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Events
        static public class Events
        {
            public static void on_key_press(object sender, SFML.Window.KeyEventArgs e)
            {
                key_data[(int)decode(e.Code)].on_press_start();
                if (e.Code == SFK.Space)  _ichar = " ";
                else if (e.Code == SFK.Return) _ichar = "|";
                else _ichar = e.Code.ToString();

            }

            public static void on_key_depress(object sender, SFML.Window.KeyEventArgs e)
            {
                key_data[(int)decode(e.Code)].on_press_end();
            }

            public static void on_key_press(object sender, SFML.Window.MouseButtonEventArgs e)
            {
                key_data[(int)Keys.Mouse1 + (int)e.Button].on_press_start();
            }

            public static void on_key_depress(object sender, SFML.Window.MouseButtonEventArgs e)
            {
                key_data[(int)Keys.Mouse1 + (int)e.Button].on_press_end();
            }

            public static void on_mouse_move(object sender, SFML.Window.MouseMoveEventArgs e)
            {
                Mouse.dx = e.X - Mouse.x; Mouse.dy = e.Y - Mouse.y;
                Mouse.coords = new crds2(e.X, e.Y);                
            }

            public static void on_mouse_wheel(object sender, SFML.Window.MouseWheelEventArgs e)
            {
                Mouse.wheel += e.Delta;
            }
        }
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////
    }
}
