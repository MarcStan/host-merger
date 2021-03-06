namespace HostMerger.Logic
{
    public class HostSource
    {
        /// <summary>
        /// List of other host files to fetch
        /// </summary>
        public string[] Links { get; set; } = new string[0];

        /// <summary>
        /// Takes a raw input "blackholeip hostname", e.g. "0.0.0.0 ads.com"
        /// </summary>
        public string[] Raw { get; set; } = new string[0];

        /// <summary>
        /// Takes a list of host names to block.
        /// </summary>
        public string[] Hosts { get; set; } = new string[0];
    }
}
