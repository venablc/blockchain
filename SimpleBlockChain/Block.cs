using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SimpleBlockChain
{

    /// <summary>
    /// A Block is a single piece of information that gets added to the block chain. In addition to the data
    /// it also contains a reference to the previous block in the chain.
    /// </summary>
    [Serializable]
    public class Block
    {
        public int Index;
        public DateTime TimeStamp;
        public string Data;
        public string BlockHash;
        public string PreviousBlockHash;

        public void GenerateHash(System.Security.Cryptography.HashAlgorithm _HashAlgorithm)
        {

            BlockHash = _generateHashOfSelf(_HashAlgorithm);


        }

        private string _generateHashOfSelf(System.Security.Cryptography.HashAlgorithm _HashAlgorithm)
        {
            return System.Text.Encoding.ASCII.GetString(_HashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(Index.ToString() + TimeStamp.ToString() + Data + PreviousBlockHash))); 
        }

        internal bool ValidateBlock(System.Security.Cryptography.HashAlgorithm _HashAlgorithm)
        {
            return this.BlockHash.Equals(this._generateHashOfSelf(_HashAlgorithm));
        }
    }

}