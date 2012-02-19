using System;
using System.Collections.Generic;
using SA = SFML.Audio;

namespace XF
{
    static public partial class Audio
    {
        /////////////////////////////////////////////////////////////////////////////////////

        #region Ambient sounds

        public class AmbientSound
        {   
            internal readonly SA.Sound sound;
            internal readonly Sample   assigned_sample;
            internal float volume;
            internal float pitch;
            internal float pan;
            private  bool _played;
            private  bool _breath_of_life;
            private float _alpha;

            internal bool wanna_die { get { return (!_breath_of_life) && (_alpha < 0.01f); } }

            internal void tick()
            {
                if (_breath_of_life)
                {
                    if (!_played) { _played = true; sound.Play(); }
                    _alpha += 0.1f;
                }
                else
                {
                    _alpha -= 0.1f;                    
                }
                _alpha = _alpha.choke01();

                sound.Volume = volume * volume * _alpha * 100f;
                sound.Pitch = pitch;            
            }

            internal void post_tick()
            {
                _breath_of_life = false;
            }

            internal void nudge(float new_volume, float new_pan, float new_pitch)
            {
                volume = new_volume;
                pan = new_pan;
                pitch = new_pitch;
                _breath_of_life = true;
            }

            internal void on_die()
            {
                sound.Stop();
            }

            public AmbientSound(Sample sample)
            {
                this.volume = 0.5f;
                this.pan = 0f;
                this.pitch = 1f;
                this.assigned_sample = sample;

                // create sound object
                sound = new SA.Sound();
                sound.SoundBuffer = sample.buffer;
                sound.Loop = true;
                sound.Pitch = pitch;
                sound.Volume = volume * volume * sample.volume_modifier * 100f;                
            }

        }

        static private Dictionary<int, AmbientSound> ambient_sounds = new Dictionary<int, AmbientSound>();

        static public AmbientSound ambient_sound(int unique_id, Sample sample, float volume = 0.5f, float pan = 0f, float pitch = 1f)
        {
            AmbientSound s;

            var key = unique_id ^ (sample.index << 16);

            if (ambient_sounds.ContainsKey(key))    s = ambient_sounds[key];
            else                                    {s = new AmbientSound(sample); ambient_sounds.Add(key, s);};

            s.nudge(volume, pan, pitch);

            return s;
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Sample lists and lookups

        static internal List<Sample> samples = new List<Sample>();
        static private Dictionary<string, Sample> _lookup = new Dictionary<string, Sample>();
        static private List<SA.Sound> sounds = new List<SA.Sound>();        

        static public Sample find(string id)
        {
            Sample s;
            _lookup.TryGetValue(id, out s);
            return s;
        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Play sounds
        static public void play(Sample sample, float volume = 0.65f, float pan = 0f, float pitch = 1f)
        {
            if (sample == null) return;
            if (Application.audio_disabled) return;

            var sound = new SA.Sound();

            sounds.Add(sound);
            sound.SoundBuffer = sample.buffer;
            sound.Loop = false;
            sound.Pitch = pitch;
            
            sound.Volume = volume * volume * sample.volume_modifier * 100f;
            sound.Play();
        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Diagnostics
        static public void print()
        {
            var y = 80f;
            foreach (var sound in sounds)
            {
                Graphics.add_text(30f, y, "Sound : " + sound.Status.ToString());
                y += 12f;
            }
            foreach (var ambient_sound in ambient_sounds)
            {
                Graphics.add_text(30f, y, "Ambient : " + ambient_sound.Value.assigned_sample.name + " " + ambient_sound.Value.sound.Status.ToString());
                y += 12f;
            }
        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Logic - invoked by the framework
        static internal void init()
        {

        }
        static internal void tick()
        {
            for (int s = sounds.Count - 1; s >= 0; s--)
            {
                var snd = sounds[s];
                if (snd.Status == SA.SoundStatus.Stopped) sounds.RemoveAt(s);
            }

            foreach (var snd in ambient_sounds) snd.Value.tick();            
            var death_list = new List<int>();
            foreach (var snd in ambient_sounds) if (snd.Value.wanna_die) death_list.Add(snd.Key); // wanna_die means it wasn'tex nudged this tick
            foreach (var snd in ambient_sounds) snd.Value.post_tick(); // sets breath of life to false.
            foreach (var item in death_list) { ambient_sounds[item].on_die(); ambient_sounds.Remove(item); }

        }

        static internal void terminate()
        {

        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////        
    }
}
