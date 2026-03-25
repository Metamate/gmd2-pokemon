using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Pokemon.Mons;

// Registry of all Mon species, loaded from data/pokemon_definitions.xml.
public static class PokemonDefinitions
{
    public static IReadOnlyList<PokemonSpecies> All { get; private set; }

    public static void LoadContent(ContentManager content)
    {
        string path = Path.Combine(content.RootDirectory, "data/pokemon_definitions.xml");
        using var stream = TitleContainer.OpenStream(path);
        var doc  = XDocument.Load(stream);
        var list = new List<PokemonSpecies>();

        foreach (var el in doc.Root.Elements("Mon"))
        {
            list.Add(new PokemonSpecies
            {
                Name              = el.Attribute("name").Value,
                BattleSpriteFront = el.Attribute("front").Value,
                BattleSpriteBack  = el.Attribute("back").Value,
                BaseHp      = int.Parse(el.Attribute("hp").Value),
                BaseAttack  = int.Parse(el.Attribute("attack").Value),
                BaseDefense = int.Parse(el.Attribute("defense").Value),
                BaseSpeed   = int.Parse(el.Attribute("speed").Value),
                HpIV      = int.Parse(el.Attribute("hpIV").Value),
                AttackIV  = int.Parse(el.Attribute("attackIV").Value),
                DefenseIV = int.Parse(el.Attribute("defenseIV").Value),
                SpeedIV   = int.Parse(el.Attribute("speedIV").Value),
            });
        }

        All = list;
    }

    public static PokemonSpecies GetRandom()
        => All[Random.Shared.Next(All.Count)];
}
