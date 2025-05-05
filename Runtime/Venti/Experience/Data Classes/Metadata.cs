using System;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Events;

namespace Venti.Experience
{
    [Serializable]
    public class Metadata
    {
        [Header("Experience Info")]
        public string experienceId;
        public string name;
        public string description;
        public string icon;
        public SemVer version;
        public string publisher;
        public string[] tags;

        [Header("Experience Profile Page")]
        public string coverImageDesktop;
        public string coverImageMobile;
        public string[] screenshots;
        public string showReelVideo;

        [Header("Builds Info")]
        public BuildInfo[] builds;

        [field: NonSerialized][field: ReadOnly] public string hash {  get; private set; }

        [HideInInspector]
        private string[] plugins;

        public void SetPlugins()
        {
            //this.plugins = plugins;
        }
    }
}