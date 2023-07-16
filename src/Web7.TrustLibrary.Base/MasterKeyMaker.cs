using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web7.TrustLibrary.NBitcoin.BIP39;
using Web7.TrustLibrary.NBitcoin;

namespace Web7.TrustLibrary.Base
{
    public class MasterKeyMaker
    {
        Mnemonic mnemonic = null;
        string wordString;

        public string WordString { get => wordString; }

        public byte[] RandomMasterKey(string masterPassPhrase)
        {
            if (RandomUtils.Random == null)
            {
                RandomUtils.Random = new UnsecureRandom();
            }

            Mnemonic mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
            string wordString = String.Join(" ", mnemonic.Words);

            return MakeMasterKey(masterPassPhrase, wordString);
        }

        public byte[] MakeMasterKey(string masterPassphrase, string wordString)
        {
            byte[] masterKey = null;

            if (RandomUtils.Random == null)
            {
                RandomUtils.Random = new UnsecureRandom();
            }

            mnemonic = new Mnemonic(wordString, Wordlist.English);
            masterKey = mnemonic.DeriveSeed(masterPassphrase); // Compute masterKey (salt)

            this.wordString = wordString;

            return masterKey;
        }
    }
}
