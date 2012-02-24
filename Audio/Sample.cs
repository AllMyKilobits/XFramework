using System;
using System.Collections.Generic;
using SA = SFML.Audio;

namespace XF
{
    public class Sample
    {
        internal readonly string name;
        internal readonly string path;
        internal readonly int    index;
        internal SA.SoundBuffer buffer;
        public   float          volume_modifier;

        static private int count;

        public Sample(string path)
        {
            this.path = path;
            // determine name
            var s = path.LastIndexOf('\\');
            name = path.Substring(s + 1);
            s = name.LastIndexOf('.');
            if (s >= 0) name = name.Substring(0, s);

            // aight, create buffer
            this.buffer = new SA.SoundBuffer(path);
            this.volume_modifier = 1f;
                        
            this.index = count;
            count++;
        }

        static public implicit operator Sample(string id)
        {
            return Audio.find(id);
        }

    }
}
