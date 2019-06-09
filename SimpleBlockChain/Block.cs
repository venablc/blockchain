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
       

        public DateTime TimeStamp;
        public string Data;
        public string BlockHash;
        
        public string PreviousBlockHash;



        public Boolean Save(IChainStorageProvider StorageProvider,System.Security.Cryptography.HashAlgorithm _HashAlgorithm){

            
            TimeStamp = DateTime.Now;
            this.PreviousBlockHash = StorageProvider.PopBlock()?.BlockHash;
            this.BlockHash = _generateHashOfSelf(_HashAlgorithm);

            return StorageProvider.Add(this);

        }

        private string _generateHashOfSelf(System.Security.Cryptography.HashAlgorithm _HashAlgorithm)
        {
            return System.Text.Encoding.ASCII.GetString(_HashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(TimeStamp.ToString() + Data + PreviousBlockHash))); 
        }

        internal bool ValidateBlock(System.Security.Cryptography.HashAlgorithm _HashAlgorithm)
        {
            return this.BlockHash.Equals(this._generateHashOfSelf(_HashAlgorithm));
        }
    }

}