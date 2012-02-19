using System;
using System.Collections.Generic;
using XF;

namespace XF
{
    public class Crest
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Crest layer definitions
        public class Layer
        {
            public Graphics.Texture texture;
            public Graphics.blend_mode blend;
            public float scale;
            public float alpha;
            public int sequence_x;
            public int sequence_y;
            public uint color;
        }
        private List<Layer> _layers; 
        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Constructor, finalizer
        public Crest()
        {
            _layers = new List<Layer>();
            add_layer(Graphics.textures["empty_crest"]);
            _default = true;
        } 
        

        ~Crest() // finalizer
        {
            if (_id != null) crests.Remove(_id); 
        }
        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Logic, layer adding
        private bool _default;
        public void add_layer(Graphics.Texture t, float scale = 1f, float alpha = 1f, uint color = 0xffffffff, Graphics.blend_mode blend = Graphics.blend_mode.normal, int sequence_x = 0, int sequence_y = 0)
        {
            if (_default) { _layers.Clear(); _default = false; }
            var l = new Layer();
            l.texture = t;
            l.alpha = alpha;
            l.scale = scale;
            l.blend = blend;
            l.color = color;
            l.sequence_x = sequence_x;
            l.sequence_y = sequence_y;
            _layers.Add(l);
        }
        
        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Rendering
        public void render(crds2 screen_coords, float alpha_multiplier = 1f, float scale_multiplier = 1f, float temperature = 0f)
        {
            foreach (var layer in _layers)
            {
                var spr = Graphics.add_sprite(layer.texture, screen_coords.x, screen_coords.y);

                spr.set_scale(layer.scale * scale_multiplier);
                spr.set_colors(layer.color, layer.alpha * alpha_multiplier);
                spr.blit.sequence.x = (uint)layer.sequence_x;
                spr.blit.sequence.y = (uint)layer.sequence_y;
            }
        } 
        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Layer management
        public int layer_count { get { return _layers.Count; } }
        public Layer this[int index]
        {
            get { return _layers[index]; }
        } 
        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region identity, lookup, registration
        private string _id;
        private static Dictionary<string, Crest> crests = new Dictionary<string, Crest>();
        public void register(string id)
        {
            _id = id;
            crests.Add(_id, this);
        }

        public static Crest find(string id)
        {
            Crest crest;
            crests.TryGetValue(id, out crest);
            return crest;
        } 
        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
