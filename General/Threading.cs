using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace XF
{
    public static partial class Session
    {
        static class Threading
        {
            static private double accumulated_time = 0f;
            static internal Stopwatch timer = new Stopwatch();

            static internal void single_thread()
            {
                Debug.Log("Entering single thread");
                accumulated_time = 0f;

                while (true)
                {
                    accumulated_time += timer.Elapsed.TotalSeconds;

                    cycle_index++;
                    
                    while (accumulated_time > _tick_duration)
                    {
                        if (exit_signal)
                        {
                            on_exit();
                            return;
                        }

                        accumulated_time -= _tick_duration;

                        //Debug.Log("Tick no. " + tick_index);

                        tick();

                        if (accumulated_time > _tick_duration) accumulated_time = 0f;
                        
                    }

                    interpolation = ((float)(accumulated_time / _tick_duration)).choke01();

                    time_since_last_render += timer.Elapsed.TotalSeconds;

                    var first_next_event = Utility.min(_tick_duration - accumulated_time, min_render_interval - time_since_last_render);
                    if (first_next_event > 0.001)
                    {
                        var ms = XMath.floor((float)first_next_event * 1000f) - 1;                        
                        //if (ms > 1) Thread.Sleep(ms);
                    }                    
                    timer.Restart();

                    //if (time_since_last_render > min_render_interval) {
    
                    render(interpolation); 
                    time_since_last_render = 0f;
                    
                    //}

                    last_frame_time = (float)(timer.Elapsed.TotalSeconds - accumulated_time);

                    Thread.Sleep(0);
                }
            }
            private const  double max_fps = 60;
            private const  double min_render_interval = 1.0 / 60;
            static private double time_since_last_render;

            static private object syncer = new object();

            static internal void multi_thread()
            {
                var logic = new Thread(Threading.logic_thread);
                var renders = new Thread(Threading.render_thread);
                logic.Name = "XF Logic Thread";
                renders.Name = "XF Rendering Thread";

                logic.Start();
                renders.Start();

                while (!exit_signal)
                {
                    accumulated_time += timer.Elapsed.TotalSeconds;                    
                    Session.interpolation = (float)(accumulated_time / _tick_duration);
                    timer.Restart();
                }                
            }

            static internal void logic_thread()
            {
                while (!exit_signal)
                {
                    while (accumulated_time > _tick_duration)
                    {
                        cycle_index++;

                        if (exit_signal)
                        {
                            on_exit();
                        }
                        else
                        {
                            tick();
                            //lock (syncer) 
                                accumulated_time -= _tick_duration;                            
                        }
                    }
                    //Thread.Sleep(0);
                }
            }

            static internal void render_thread()
            {
                Graphics.set_window(1280, 800, false);

                while (!exit_signal)
                {
                    render(Session.interpolation);
                    //Thread.Sleep(0);
                }
            }


        }
    }
}
