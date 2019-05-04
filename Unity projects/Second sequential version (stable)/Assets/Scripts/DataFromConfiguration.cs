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
    }

    void Start()
    {
        document.LoadXml(GameAsset.text);   //Load file
        XmlNode root = document.DocumentElement;

        //Options
        XmlNode options = root.SelectSingleNode("options");
        //Volume
        XmlNode volume = options.SelectSingleNode("volume");
        //Music
        XmlNode music = volume.SelectSingleNode("music");
        XmlNode musicDefaultValue = music.SelectSingleNode("defaultValue");
        print(musicDefaultValue.InnerText);
    }
}
