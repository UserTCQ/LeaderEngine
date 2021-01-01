﻿using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine
{
    public class AudioListener : Component
    {
        public override void Start()
        {
            AL.Listener(ALListenerf.Gain, 1.0f);
            AL.Listener(ALListener3f.Position, ref transform.Position);
        }
    }
}
