using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XF
{
    static partial class Graphics
    {
        public class BitmapFont
        {
            public Texture tex;
            public float v_spacing;
            public float h_spacing;

            public class BitmapCharData
            {
                public uint  seq_x;
                public uint  seq_y;
                public float width;

                public BitmapCharData()
                {
                    seq_x = 0;
                    seq_y = 0;
                    width = 0.0f;
                }

            }
            public BitmapCharData this[int index]
            {
                get { return char_data[index]; }
                set { char_data[index] = value; }
            }

            public BitmapCharData[] char_data;

            public BitmapFont(Texture t, string def_file)
            {
                tex = t;
                char_data = new BitmapCharData[256];
                for (int i = 0; i < 256; i++)
                {
                    char_data[i] = new BitmapCharData();
                }
            }        
        }
    }
}
