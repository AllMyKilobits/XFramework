using System;
using System.Collections.Generic;

namespace XF
{
    public static partial class Session
    {
        ///////////////////////////////////////////////////////////////////////////////////

        #region Time, timelapse, tick time, etc.

        /// <summary> Returns the index of the current or last executed TICK</summary>
        public static long tick_index { get; private set; }

        /// <summary>Returns the index of the current GAME LOOP CYCLE </summary>
        public static long cycle_index { get; private set; }

        /// <summary>Returns interpolation of the game between two ticks. </summary>
        public  static float        interpolation { get; internal set; }

        public static float time { get { return Threading.empiric_time_elapsed; } }

        #endregion

        ///////////////////////////////////////////////////////////////////////////////////        

        #region State management - private   
        static private List<State> _states = new List<State>();
        static private List<State> _sdl = new List<State>(); // _sdl = state death list
        static internal void add_state(State state)
        {
            _states.Add(state);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////        

        #region State management - exposed
        /// <summary>Enqueues a state for termination. state.on_terminate will be called at the end of tick</summary>
        static public void terminate_state(State state)
        {
            if (!_sdl.Contains(state)) 
                _sdl.Add(state);
        } 
        #endregion
        
        ///////////////////////////////////////////////////////////////////////////////////        

        #region Preparation and execution
        /// <summary>  Call this exactly ONCE and do so immediately when the framework starts </summary>        
        public static void prepare()
        {
            

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

            Session.Threading.target_renders_per_second = 60;
            Session.Threading.max_ticks_per_cycle = 1;
            Session.Threading.set_tick_interval(0.025f);
                
            Session.Threading.tick_loop_policy      = Threading.TickPolicy.YieldAndFlushExtraTime;
            Session.Threading.render_loop_policy    = Threading.RenderPolicy.ThrottleToDesiredFPS;
            Session.Threading.processor_loop_policy = Threading.ProcessorPolicy.YieldAndSleepUnnecessaryCycles;

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

        }  
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////

        #region Session termination
        /// <summary> </summary>
        public static void on_close_signal(object sender, EventArgs e)
        {
            terminate_application();
        }

        /// <summary> Call this when you need the session to queue_termination. This will ENQUEUE the termination at the end of the tick.</summary>
        public static void terminate_application()
        {
            _quit_signal = true;

        }

        private static bool _quit_signal;
        static private void death_throes()
        {
            foreach (var state in _states) state.on_terminate();            
            _states.Clear();

            Input.terminate();
            Audio.terminate();
            Debug.on_terminate();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////

        #region Called by Threading on tick
        /// <summary>  Called upon application tick. </summary>        
        private static void tick()
        {
            tick_index++;

            Input.tick();

            // execute tick event.
            Events.trigger(Event.tick);

            Graphics.tick();
            Audio.tick();
            Statistics.on_tick();

            Events.trigger(Event.post_tick);
            
            // execute post-tick event
            if (_quit_signal) death_throes();

            // Clear states marked for termination:
            foreach (var state in _sdl) {
                state.on_terminate(); _states.Remove(state); 
            }
            _sdl.Clear();             
        } 
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////

        #region Called by Threading Cycler on render
        /// <summary>  Called when app wants to render. </summary>        
        private static void render()
        {
            Events.trigger(Event.render);
            Graphics.execute();
            Statistics.on_render();
        } 
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////        
    }
} 
