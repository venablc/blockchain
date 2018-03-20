using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SimpleBlockChain
{
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
                      
            BlockHash = System.Text.Encoding.ASCII.GetString( _HashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(Index.ToString() + TimeStamp.ToString() + Data + PreviousBlockHash )));
            
        }

    }

}