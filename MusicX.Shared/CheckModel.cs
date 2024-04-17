namespace MusicX.Shared
{
    public class CheckModel
    {
        public bool IsUpdated { get; set; }

        public List<Version> Versions { get; set; }

        public Version LastVersion { get; set; }

    }
}
