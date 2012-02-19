using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XF
{
    /// <summary>
    /// An abstract class used for building games in the framework.
    /// A XF.Session iterates through all registered states on tick and render.
    /// </summary>
    public abstract class State 
    {
        internal bool death_mark;

        public State()
        {
            death_mark = false;            
        }

        public void queue_termination() { death_mark = true; }

        // all these are virtual because a state does not NEED to implement all of them.

        virtual public void on_state_init() { }
        virtual public void on_tick() { }
        virtual public void on_render(float interpolation) { }
        virtual public void on_terminate() { }
        virtual public void on_tick_end()  { }
    }


    public static partial class Session
    {
        ///////////////////////////////////////////////////////////////////////////////////

        #region Time, timelapse, tick time, etc.
        public static long tick_index { get; private set; }
        public static long cycle_index { get; private set; }

        private static double   _tick_duration;
        public  static float     tick_duration { get { return (float)_tick_duration; } }
        public  static int ticks_per_second { get { return (int)(1d / _tick_duration); } }

        public static float last_frame_time { get; private set; }

        public static float elapsed_time { get { return (float)(_tick_duration * tick_index); } }  // timestep is miliseconds per tick, not ticks per second
        // seconds per tick = timestep / 1000
        // ticks per seconds = 1000 / timestep
        public static float interpolation { get; set; }
        /// <summary>  Change the time step and the number of executions per second. </summary>        
        public static void set_tick_interval(float new_duration)
        {
            _tick_duration = new_duration;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////////////        

        #region Flow, states, exit signals
        
        private static bool exit_signal;

        private static Dictionary<string, State> states = new Dictionary<string, State>();

        public static State get_state(string id)
        {
            State result = null;
            states.TryGetValue(id, out result);
            return result;
        } 
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////        

        #region State management, adding, and removal
        static List<KeyValuePair<string, State>> states_to_add = new List<KeyValuePair<string, State>>();

        /// <summary>  Add a state to the execution loop </summary>       
        public static void add_state(string key, State state)
        {
            states.Add(key, state);
            state.on_state_init();
        }

        /// <summary>  Retrieve a state from the execution loop. The state is removed on tick end</summary>
        public static void remove_state(string key)
        {
            var state = get_state(key);
            if (state == null) return;
            state.queue_termination();
        }

        // local - actually kills the state
        private static void kill_state(string key)
        {
            var state = get_state(key);
            if (state == null) return;
            state.on_terminate();
            states.Remove(key);
        } 
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////        
        
        /// <summary>  Call this exactly ONCE and do so immediately when the framework starts </summary>        
        public static void prepare()
        {
            set_tick_interval(1f / XF.Globals.default_timestep);

            #region Process root
            var root = System.Windows.Forms.Application.StartupPath;
            if (root.EndsWith("\\Debug") || root.EndsWith("\\Release")) root = root.Remove(root.LastIndexOf('\\'));
            if (root.EndsWith("\\bin")) root = root.Remove(root.LastIndexOf('\\') + 1);
            if (!root.EndsWith("\\")) root += '\\';
            Application.root_path = root; 
            #endregion

            Debug.on_init();
            Debug.Log("Application initialized on " + System.DateTime.Now);
            Debug.Log("Root path set to " + Application.root_path);

            XMath.initialize();
            Input.initialize();
            Audio.init();            
        }

        ///////////////////////////////////////////////////////////////////////////////////

        #region Session termination
        public static void on_close_signal(object sender, EventArgs e)
        {
            terminate();
        }

        /// <summary> Call this when you need the session to queue_termination. This will ENQUEUE the termination at the end of the tick.</summary>
        public static void terminate()
        {
            exit_signal = true;
            _queued_termination = true;
        }
        static bool _queued_termination;

        static private void execute_termination()
        {
            foreach (KeyValuePair<string, State> pair in states)
            {
                pair.Value.on_terminate();
            }
            states.Clear();

            Input.terminate();
            Audio.terminate();
            Debug.on_terminate();
        }

        private static void on_exit()
        {

        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////

        /// <summary>  Called upon application tick. </summary>        
        private static void tick()
        {
            tick_index++;

            Input.tick();

            // ITERATE THROUGH STATES.                        
            List<string> states_to_kill = new List<string>();
            foreach (var pair in states_to_add)
            {
                add_state(pair.Key, pair.Value);                
            }
            states_to_add.Clear();

            foreach (var pair in states)            
            {                
                pair.Value.on_tick();
                if (pair.Value.death_mark) states_to_kill.Add(pair.Key);
            }

            Graphics.tick();
            Audio.tick();
            Statistics.on_tick();

            foreach (var state in states) state.Value.on_tick_end();
            
            if (_queued_termination) execute_termination();

            foreach (string key in states_to_kill) kill_state(key);
                        
        }

        /// <summary>  Called when app wants to render. </summary>        
        private static void render(float interpolation = 0.0f)
        {
            //Debug.Log("Rendering states : ");

            //Debug.StartLogGroup();
                foreach (KeyValuePair<string, State> pair in states)
                {
            //        Debug.Log("Rendering " + pair.Key);
                    pair.Value.on_render(interpolation);
                }
            //Debug.EndLogGroup();

            Graphics.execute();
            Statistics.on_render();
            //Audio.print();
        }

        /// <summary>  Main loop. Setup your events before this and FIRE AWAY </summary>        
        public static void execute()
        {
            // setup the timer:

            Threading.timer.Reset();
            Threading.timer.Start();

            Debug.Log("Starting session.execute...");

            #if MULTITHREAD
                Threading.multi_thread();
            #else
                Threading.single_thread();
            #endif

        } // end void main_loop

        // called only in multithreaded environment
        static private void rendering_thread()
        {            
            
        }

        public static void queue_state_add(string key, State state)
        {
            states_to_add.Add(new KeyValuePair<string, State>(key, state));
        }
    } // end class Session

} // end namespace
