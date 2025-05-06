using System;

namespace Venti.Experience
{
    [Serializable]
    public class SemVer
    {
        public int major;
        public int minor;
        public int patch;
        public ReleaseStatus status;

        public SemVer(int major, int minor, int patch, ReleaseStatus status)
        {
            this.major = major;
            this.minor = minor;
            this.patch = patch;
            this.status = status;
        }

        public override string ToString()
        {
            if (status == ReleaseStatus.Stable)
                return $"{major}.{minor}.{patch}";
            else
                return $"{major}.{minor}.{patch}-{status}";
        }
    }
}