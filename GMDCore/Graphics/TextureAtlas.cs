using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace GMDCore.Graphics;

public class TextureAtlas
{
    private readonly Dictionary<string, TextureRegion> _regions = [];
    public Texture2D Texture { get; set; }

    public TextureAtlas(Texture2D texture)
    {
        Texture = texture;
    }

    public void AddRegion(string name, int x, int y, int width, int height)
    {
        _regions.Add(name, new TextureRegion(Texture, x, y, width, height));
    }

    public TextureRegion GetRegion(string name) => _regions[name];

    // Creates a TextureAtlas by splitting a texture into a uniform grid of frames named frame_0, frame_1, etc.
    public static TextureAtlas FromGrid(Texture2D texture, int frameWidth, int frameHeight)
    {
        var atlas = new TextureAtlas(texture);
        int cols  = texture.Width  / frameWidth;
        int rows  = texture.Height / frameHeight;
        int index = 0;

        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                atlas.AddRegion($"frame_{index++}", x * frameWidth, y * frameHeight, frameWidth, frameHeight);

        return atlas;
    }

    // Creates an Animation from a list of frame indices into this atlas.
    public Animation CreateAnimation(int[] frameIndices, double intervalSeconds, bool loop = true)
    {
        var frames = new List<TextureRegion>(frameIndices.Length);
        foreach (int i in frameIndices)
            frames.Add(GetRegion($"frame_{i}"));

        return new Animation(frames, TimeSpan.FromSeconds(intervalSeconds), loop);
    }
}
