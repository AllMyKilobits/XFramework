using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace XF
{
    public static partial class Session
    {
        public static class Threading
        {
            ///////////////////////////////////////////////////////////////////////////////

            #region Private and internal control variables
            private static double _tick_duration;
            static private double accumulated_time = 0f;
            static private DateTime execution_start_time;            
            static private double min_render_interval { get { return 1d / target_renders_per_second; } }
            static private DateTime last_render_started_on;
            static private double time_since_last_render { get { return (DateTime.Now - last_render_started_on).TotalSeconds; } }
            static internal Stopwatch timer = new Stopwatch();
            #endregion

            ///////////////////////////////////////////////////////////////////////////////

            #region Exposed properties
            static public float tick_duration { get { return (float)_tick_duration; } }
            static public float empiric_time_elapsed { get { return (float)((DateTime.Now - execution_start_time).TotalSeconds); } } 
            #endregion

            ///////////////////////////////////////////////////////////////////////////////

            #region Tick And Render Policies
            public enum TickPolicy
            {
                /// <summary>a maximum of max_ticks_per_cycle will be processed before each render</summary>
                YieldAfterMaxTicks,
                /// <summary>after max_ticks_per_cycle ticks, the remaining accumulated time is flushed.</summary>
                YieldAndFlushExtraTime,
                /// <summary>the engine will process as many ticks as it can in a cycle, even if it means locking up the processor</summary>
                StrictEnforceTPS,

            }
            static public TickPolicy tick_loop_policy = TickPolicy.YieldAfterMaxTicks;
            public enum RenderPolicy
            {
                /// <summary>Will pass up rendering more than target_renders_per_second.</summary>                
                ThrottleToDesiredFPS,
                /// <summary> Will render as many frames per second as possible. Note that it will still  </summary>
                SqueezeOutAllPossibleFPS,

            }
            static public RenderPolicy render_loop_policy = RenderPolicy.ThrottleToDesiredFPS;
            public enum ProcessorPolicy
            {
                /// <summary>After each render cycle the process will yield to the operating system briefly</summary>
                YieldToOSEveryCycle,                
                /// <summary>The process will sleep in anticipation of the next needed cycle. 
                /// Note that this policy will be ineffective if your render policy is to squeeze out all possible fps</summary>
                YieldAndSleepUnnecessaryCycles,
                /// <summary>There will be no yields to the OS. This is not recommended.</summary>
                GreedyLoop,
            }
            static public ProcessorPolicy processor_loop_policy = ProcessorPolicy.YieldToOSEveryCycle;

            /// <summary>Maximum of tick backlog that can be processed before the cycle yields to the renderer.</summary>
            static public int max_ticks_per_cycle = 3;

            /// <summary>The renderer will be content to render this many per second, or less if pressed, and will yield when satisfied.
            /// Only works if Render Policy is set to Throttle to target FPS</summary>
            static public double target_renders_per_second = 60.0;

            public static void set_tick_interval(float new_duration)
            {                
                _tick_duration = new_duration;
            }
            
            static private void check_policies()
            {
                if (processor_loop_policy == ProcessorPolicy.YieldAndSleepUnnecessaryCycles && render_loop_policy == RenderPolicy.SqueezeOutAllPossibleFPS)
                    Debug.Log("Threading policies warning : there won't be any unnecessary cycles to sleep since RenderPolicy will try to squeeze out all possible FPS.");
            }

            #endregion

            ///////////////////////////////////////////////////////////////////////////////

            static internal void single_thread()
            {
                Debug.Log("Entering single thread");
                accumulated_time = 0f;
                execution_start_time = DateTime.Now;

                check_policies();
               
                while (true)
                {
                    // new cycle
                    cycle_index++;

                    accumulated_time += timer.Elapsed.TotalSeconds;
                    timer.Restart();

                    var num_repeats = 0;
                    var tpc = max_ticks_per_cycle;
                    if (tick_loop_policy == TickPolicy.StrictEnforceTPS) tpc = 9999; // can't go above this captain.

                    while (accumulated_time > _tick_duration && num_repeats < tpc)
                    {
                        if (_quit_signal) { /*on_exit(); */ return; }

                        tick();

                        accumulated_time -= _tick_duration;
                        num_repeats++;                        
                    }

                    if (tick_loop_policy == TickPolicy.YieldAndFlushExtraTime) if (accumulated_time > _tick_duration) accumulated_time = _tick_duration * 0.99f;  
                    
                    interpolation = ((float)(accumulated_time / _tick_duration)).choke01();

                    bool do_render = true;

                    if (render_loop_policy == RenderPolicy.ThrottleToDesiredFPS)
                    {
                        if (time_since_last_render < min_render_interval) do_render = false;
                    }

                    if (do_render)
                    {
                        last_render_started_on = DateTime.Now;
                        render();                        
                    }

                    if (processor_loop_policy == ProcessorPolicy.YieldAndSleepUnnecessaryCycles)
                    {
                        var time_to_next_tick = _tick_duration - accumulated_time;
                        var time_to_next_render = min_render_interval - time_since_last_render;
                        if (render_loop_policy == RenderPolicy.SqueezeOutAllPossibleFPS) time_to_next_render = 0;
                        var first_next_event = Utility.min(time_to_next_tick, time_to_next_render);
                        if (first_next_event > 0.001)
                        {
                            var ms = XMath.floor((float)first_next_event * 1000f) - 1;
                            if (ms > 1)
                            {
                                Statistics.report_about_to_sleep(ms);
                                Thread.Sleep(ms);
                            }
                            else
                            {
                                Thread.Sleep(0);
                            }
                        }
                    }
                    else if (processor_loop_policy == ProcessorPolicy.YieldToOSEveryCycle)
                    {
                        Thread.Sleep(0);
                    }
                }
            }

            ///////////////////////////////////////////////////////////////////////////////
            
            #region Multithread code - NOT POSSIBLE
            //static private object syncer = new object();

            //static internal void multi_thread()
            //{
            //    var logic = new Thread(Threading.logic_thread);
            //    var renders = new Thread(Threading.render_thread);
            //    logic.Name = "XF Logic Thread";
            //    renders.Name = "XF Rendering Thread";

            //    logic.Start();
            //    renders.Start();

            //    while (!_quit_signal)
            //    {
            //        accumulated_time += timer.Elapsed.TotalSeconds;                    
            //        Session.interpolation = (float)(accumulated_time / _tick_duration);
            //        timer.Restart();
            //    }                
            //}

            //static internal void logic_thread()
            //{
            //    while (!_quit_signal)
            //    {
            //        while (accumulated_time > _tick_duration)
            //        {
            //            cycle_index++;

            //            if (_quit_signal) { /*on_exit();*/ return; }
            //            else
            //            {
            //                tick();
            //                accumulated_time -= _tick_duration;
            //            }                        
            //        }                    
            //    }
            //}

            //static internal void render_thread()
            //{
            //    Graphics.set_window(1280, 800, false);

            //    while (!_quit_signal)
            //    {
            //        render(Session.interpolation);
            //        //Thread.Sleep(0);
            //    }
            //} 
            #endregion

            ///////////////////////////////////////////////////////////////////////////////            
        }
    }
}
