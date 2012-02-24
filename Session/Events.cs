using System;
using System.Collections.Generic;

namespace XF
{
    public enum Event
    {
        tick,
        render,
        post_tick,
        ubound
    }

    public static partial class Session
    {
        static public class Events
        {
            #region Delegates
            public delegate void basic_event(); 
            #endregion

            #region Public functionality
            static internal void register(XF.Event which_event, basic_event method)
            {
                _eventbank[(int)which_event] += method;
            }
            static internal void unregister(XF.Event which_event, basic_event method)
            {
                _eventbank[(int)which_event] -= method;
            }
            static internal void trigger(XF.Event which_event)
            {
                basic_event e = find_event(which_event);

                if (e != null)
                    e();
            } 
            #endregion

            #region Private stuff
            static Events()
            {
                _eventbank = new basic_event[(int)XF.Event.ubound];

            }
            static private basic_event[] _eventbank;
            static private basic_event find_event(XF.Event e)
            {
                return _eventbank[(int)e];
            } 
            #endregion            
        }

        
    }
}
