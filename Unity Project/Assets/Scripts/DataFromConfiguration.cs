using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using UnityEngine;

public class DataFromConfiguration : MonoBehaviour {
    public TextAsset GameAsset;

    XmlDocument document;

    void Awake()
    {
        document = new XmlDocument();
        document.LoadXml(GameAsset.text);   //Load file
        XmlNode root = document.DocumentElement;

        XmlNode options = root.SelectSingleNode("options");
        XmlNode volume = options.SelectSingleNode("volume");
        XmlNode generation = options.SelectSingleNode("generation");
        XmlNode pso = root.SelectSingleNode("pso");
        XmlNode astronautPso = pso.SelectSingleNode("astronaut");
        XmlNode alienPso = pso.SelectSingleNode("alien");
        XmlNode entity = root.SelectSingleNode("entity");
        XmlNode astronautEntity = entity.SelectSingleNode("astronaut");
        XmlNode alienEntity = entity.SelectSingleNode("alien");
        XmlNode world = root.SelectSingleNode("world");
        XmlNode noise = world.SelectSingleNode("noise");
        //Music
        XmlNode music = volume.SelectSingleNode("music");
        DataBetweenScenes.setMusicVolume(float.Parse(music.InnerText));
        //
        //Sounds
        XmlNode sounds = volume.SelectSingleNode("sounds");
        DataBetweenScenes.setSoundsVolume(float.Parse(sounds.InnerText));
        //
        //Size
        XmlNode size = generation.SelectSingleNode("size");
        DataBetweenScenes.setSize(int.Parse(size.InnerText));
        //
        //Seed
        XmlNode seed = generation.SelectSingleNode("seed");
        DataBetweenScenes.setSeed(int.Parse(seed.InnerText));
        //
        //C1 Astronaut
        XmlNode c1Astronaut = astronautPso.SelectSingleNode("c1");
        DataBetweenScenes.setC1Astronaut(float.Parse(c1Astronaut.InnerText));
        //
        //C2 Astronaut
        XmlNode c2Astronaut = astronautPso.SelectSingleNode("c2");
        DataBetweenScenes.setC2Astronaut(float.Parse(c2Astronaut.InnerText));
        //
        //C1 Alien
        XmlNode c1Alien = alienPso.SelectSingleNode("c1");
        DataBetweenScenes.setC1Alien(float.Parse(c1Alien.InnerText));
        //
        //C2 Alien
        XmlNode c2Alien = alienPso.SelectSingleNode("c2");
        DataBetweenScenes.setC2Alien(float.Parse(c2Alien.InnerText));
        //
        //MaxSpeed Astronaut
        XmlNode maxSpeedAstronaut = astronautEntity.SelectSingleNode("maxSpeed");
        DataBetweenScenes.setMaxSpeedAstronaut(float.Parse(maxSpeedAstronaut.InnerText));
        //
        //MaxSpeed Alien
        XmlNode maxSpeedAlien = alienEntity.SelectSingleNode("maxSpeed");
        DataBetweenScenes.setMaxSpeedAlien(float.Parse(maxSpeedAlien.InnerText));
        //
        //MaxDistanceToShoot Alien
        XmlNode maxDistanceToShoot = alienEntity.SelectSingleNode("maxDistanceToShoot");
        DataBetweenScenes.setMaxDistanceToShoot(float.Parse(maxDistanceToShoot.InnerText));
        //
        //HeightMultiplier
        XmlNode heightMultiplier = world.SelectSingleNode("heightMultiplier");
        DataBetweenScenes.setHeightMultiplier(float.Parse(heightMultiplier.InnerText));
        //
        //Scale
        XmlNode scale = noise.SelectSingleNode("scale");
        DataBetweenScenes.setScale(float.Parse(scale.InnerText));
        //
        //Octaves
        XmlNode octaves = noise.SelectSingleNode("octaves");
        DataBetweenScenes.setOctaves(int.Parse(octaves.InnerText));
        //
        //Persistance
        XmlNode persistance = noise.SelectSingleNode("persistance");
        DataBetweenScenes.setPersistance(float.Parse(persistance.InnerText));
        //
        //Lacunarity
        XmlNode lacunarity = noise.SelectSingleNode("lacunarity");
        DataBetweenScenes.setLacunarity(float.Parse(lacunarity.InnerText));
        //
    }
}
