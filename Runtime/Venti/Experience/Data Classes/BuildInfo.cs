using System;

namespace Venti.Experience
{
  [Serializable]
  public class BuildInfo
  {
    public string buildFileName;
    public BuildType buildType;
    public Resolution[] resolutions;
    public Orientation orientation;
    // public string[] plugins;
  }
}