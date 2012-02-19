using System;
using System.Collections.Generic;
using System.IO;

namespace XF
{
    static public partial class Audio
    {
        public static void load_batch_samples(string folder)
        {
            //Debug.Log("Loading audio clips from folder " + folder);
            Debug.StartLogGroup();

            DirectoryInfo dir = new DirectoryInfo(folder);
            if (dir.Exists)
            {
                var ogg_files = dir.GetFiles("*.ogg", SearchOption.AllDirectories);
                var wav_files = dir.GetFiles("*.wav", SearchOption.AllDirectories);
                //var mp3_files = dir.GetFiles("*.mp3", SearchOption.AllDirectories);

                var lst = new List<FileInfo>();
                lst.AddRange(ogg_files);
                lst.AddRange(wav_files);
                //lst.AddRange(mp3_files);
                
                foreach (var file in lst)
                {
                    var s = new Sample(file.FullName);
                    if (s.buffer == null) 
                        Debug.Log("Failed loading sample " + file.FullName);   
                    samples.Add(s);
                    _lookup.Add(s.name, s);
                }
            }
            Debug.EndLogGroup();
        }
        
    }
}
