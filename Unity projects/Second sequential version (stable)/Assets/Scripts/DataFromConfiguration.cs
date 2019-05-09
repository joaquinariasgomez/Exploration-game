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
        //Music
        XmlNode music = volume.SelectSingleNode("music");
        DataBetweenScenes.setMusicVolume(float.Parse(music.InnerText));
        //
        //Sounds
        XmlNode sounds = volume.SelectSingleNode("sounds");
        DataBetweenScenes.setSoundsVolume(float.Parse(sounds.InnerText));
        //
        XmlNode generation = options.SelectSingleNode("generation");
        //Size
        XmlNode size = generation.SelectSingleNode("size");
        DataBetweenScenes.setSize(int.Parse(size.InnerText));
        //
        //Seed
        XmlNode seed = generation.SelectSingleNode("seed");
        DataBetweenScenes.setSeed(int.Parse(seed.InnerText));
        //
    }
}
