using System;
using System.Collections.Generic;

namespace XF
{
    static partial class Graphics
    {
        #region Layer creation and removal
        public static Layer create_layer(string name)
        {
            Layer layer = new Layer(name);
            layers.Add(layer);
            if (current_layer == null) current_layer = layer;
            return layer;
        }



        #endregion

        public class Layer
        {
            /////////////////////////////////////////////////////////////////////////////////////

            #region Identifiers
            internal readonly string name; 
            #endregion

            /////////////////////////////////////////////////////////////////////////////////////

            #region Constructor
            public Layer(string name)
            {
                this.name = name;
                current_sprite_id = 0;
                sprite_count = Globals.default_spritenum;
                sprites = new Sprite[Globals.default_spritenum];
                for (int s = 0; s < sprite_count; s++) sprites[s] = new Sprite();

                if (name == "")
                {
                    return;
                }
                else
                {
                    _lookup.Add(this.name, this);
                }
                
            } 
            #endregion

            /////////////////////////////////////////////////////////////////////////////////////

            #region Sprite management

            public XF.Graphics.Sprite[] sprites;
            private uint sprite_count;
            public uint current_sprite_id;

            public Sprite new_sprite
            {
                get
                {
                    #region (Array bound checks and resizes conditions)

                    if (current_sprite_id > Globals.maximum_sprites) current_sprite_id = Globals.maximum_sprites;
                    if (current_sprite_id >= sprite_count)
                    {
                        uint new_sprite_count = sprite_count * 2;
                        Sprite[] old_sprs = new Sprite[sprite_count];
                        for (uint s = 0; s < sprite_count; s++) old_sprs[s] = sprites[s];
                        sprites = new Sprite[new_sprite_count];
                        for (uint s = 0; s < sprite_count; s++) sprites[s] = old_sprs[s];
                        for (uint s = sprite_count; s < new_sprite_count; s++) sprites[s] = new Sprite();
                        sprite_count = new_sprite_count;
                    }
                    #endregion
                    Sprite spr = sprites[current_sprite_id];
                    spr.blit.reset(); // fill with defaults
                    spr.effect.clear();
                    spr.points = null;
                    current_sprite_id++;
                    return spr;
                }
            }

            public void reset()
            {
                current_sprite_id = 0;
            } 
            #endregion

            /////////////////////////////////////////////////////////////////////////////////////

            #region Lookup, finder
            static public Layer find(string name) { return _lookup[name]; }
            static private Dictionary<string, Layer> _lookup = new Dictionary<string, Layer>();
            static public implicit operator Layer(string name) { return _lookup[name]; }
            static public bool destroy(string id)
            {
                Layer l;
                if (_lookup.TryGetValue(id, out l))
                {
                    _lookup.Remove(id);
                    layers.Remove(l);
                    return true;
                }
                return false;
            }
            #endregion

            /////////////////////////////////////////////////////////////////////////////////////

        }
    }
}
