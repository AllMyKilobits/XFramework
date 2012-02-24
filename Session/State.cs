using System;
using System.Collections.Generic;
using xf_event_delegate = XF.Session.Events.basic_event;

namespace XF
{
    /// <summary> An abstract class used for building games in the framework.
    /// States are registered in the game. </summary>
    public abstract class State
    {
        public State()
        {
            Session.add_state(this);
        }

        public virtual void on_terminate() {
            unregister_all_xf_events();
        }

        /// <summary> Register this state to be called by XFramework events</summary>
        /// <param name="which_event">Event to subscribe to</param>
        /// <param name="method">own method to be called</param>
        protected internal void register(Event which_event, xf_event_delegate method)
        {
            Session.Events.register(which_event, method);
            _registered_methods.Add(new methoddef(method, which_event));
        }
        /// <summary> Unregister from all XF events.</summary>
        protected internal void unregister_all_xf_events() {
            foreach (var _evt in _registered_methods) {
                Session.Events.unregister(_evt.which_event, _evt.method);                    
            }
            _registered_methods.Clear();
        }

        #region Private functionality to track what methods we subscribed to and to unregister if needed.
        private struct methoddef
        {
            internal readonly xf_event_delegate method;
            internal readonly Event which_event;
            public methoddef(xf_event_delegate method, Event which_event)
            {
                this.which_event = which_event;
                this.method = method;
            }
        }
        private List<methoddef> _registered_methods = new List<methoddef>();
        
        #endregion
        

    }
}
