using System;
using System.Collections.Generic;

namespace GMDCore.Graphics;

public class Animation
{
    public List<TextureRegion> Frames { get; set; }
    public TimeSpan Delay { get; set; }
    public bool Loop { get; set; } = true;

    public Animation(List<TextureRegion> frames, TimeSpan delay, bool loop = true)
    {
        Frames = frames;
        Delay = delay;
        Loop = loop;
    }
}