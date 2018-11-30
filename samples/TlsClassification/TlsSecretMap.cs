
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    public class TlsMasterSecretMap
    {
        readonly Dictionary<string, string> m_dictionary;
        public TlsMasterSecretMap()
        {
            m_dictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }
        public string GetMasterSecret(string clientRandom)
        {

            if (m_dictionary.TryGetValue(clientRandom, out string value))
                return value;
            else
                return null;
        }
        /// <summary>
        /// Loads key log from file.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <remarks>
        /// The keylog file has the following format:
        /// # Cipher Suite ECDHE-RSA-AES256-GCM-SHA384
        /// CLIENT_RANDOM 52362c10a2665e323a2adb4b9da0c10d4a8823719272f8b4c97af24f92784812 9F9A0F19A02BDDBE1A05926597D622CCA06D2AF416A28AD9C03163B87FF1B0C67824BBDB595B32D8027DB566EC04FB25
        /// </remarks>
        public static TlsMasterSecretMap LoadFromFile(string path)
        {
            var msm = new TlsMasterSecretMap();
            foreach (var line in File.ReadAllLines(path).Where(s => !(s.Trim().StartsWith('#') || String.IsNullOrWhiteSpace(s))))
            {
                var parts = line.Split(' ');
                if (String.Equals(parts[0], "CLIENT_RANDOM"))
                {
                    var clientRandom = parts[1];
                    var premaster = parts[2];
                    msm.m_dictionary.Add(clientRandom, premaster);
                }
            }
            return msm;
        }
    }
}
