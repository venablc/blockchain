using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using SimpleBlockChain.Connectors;

namespace SimpleBlockChain
{

    public class BlockChainNode
    {

        
        private IChainStorageProvider BlockChain;
        private Boolean Initiated = false;
        private HashAlgorithm _HashAlgorithm;
        private INodeConnector NodeConnector;


        public BlockChainNode()
        {
            _HashAlgorithm = new SHA1CryptoServiceProvider();
        }

        public BlockChainNode(HashAlgorithm _HashAlgorithm)
        {
            this._HashAlgorithm = _HashAlgorithm;
        }

        public void Initiate(INodeConnector NodeConnector = null, IChainStorageProvider ChainStore = null, Boolean SkipGenesisBlock = false)
        {
            if (ChainStore == null) { ChainStore = new InMemoryChainStorageProvider(); };
            
            BlockChain = ChainStore;

            BlockChain.initiate();

            if(NodeConnector != null)
            {
                this.NodeConnector = NodeConnector;
                //this.NodeConnector.RegisterNode(this);
            }

            if((NodeConnector == null || NodeConnector?.TotalNodeCount() == 0) && SkipGenesisBlock != true)
            {
                // This node either has no connector (is therefore independent) or has a connector but
                // is only listening (hasn't connected to an existing network, possibly the first for a new network)
                AddGenesisBlock();
            }
            else
            {
                // This node if part of a network of nodes, populate our blockchain with that
                // of the primary connected node.


                // BlockChain = NodeConnector.RequestInitialBlockChain();
                // if(!(BlockChain?.Count > 0))
                // {
                //     throw new Exception("Unable to retrieve initial blockchain from connected node");
                // }
                
            }

            

            Initiated = true;
        
        }

        private void AddGenesisBlock()
        {

         
            GenerateBlock("DGPKMANWJQKPDFN1237234PKSDFMSDFH39394").Save(BlockChain,_HashAlgorithm);
            
        }

        public long SubmitData(String data)
        {

            if (Initiated != true)
                throw new NodeNotReadyException();

            // todo: make this function thread safe

            var block = GenerateBlock(data);
            block.Save(BlockChain,_HashAlgorithm);
            if (NodeConnector != null)
            {
                //NodeConnector.BroadcastNewBlockAdd(block);
            }
            

            return BlockChain.Count();
        }

        public Block GenerateBlock(string data)
        {

            var Block = new Block()
            {
                Data = data
            };

            return Block;

        }

        
        public class  NodeNotReadyException : Exception { }

        public long? GetChainSize()
        {
            return BlockChain.Count();
        }

        public void SetBlockChain(List<Block> chain)
        {
            this.BlockChain.Restore(chain);
        }
        
        public List<Block> GetBlockChain()
        {
            return this.BlockChain.RetrieveAll();
        }

        internal void PutBroadcastedBlock(Block data)
        {

            if (data.PreviousBlockHash.Equals(BlockChain.Retrieve(BlockChain.Count() - 1).BlockHash))
            {
                BlockChain.Add(data);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Rejecting received broadcasted block due to hash mismatch.");
            }
            
        }


        public Boolean ValidateNode()
        {
            string lastHash = "";

            foreach (Block item in BlockChain.RetrieveAll())
            {
                if (!item.ValidateBlock(_HashAlgorithm))
                {
                    return false;
                }

                if(!string.IsNullOrEmpty(lastHash))
                {
                    if (lastHash != item.PreviousBlockHash)
                    {
                        return false;
                    }
                }

                lastHash = item.BlockHash;


            }
            return true;
        }


        
        public string Persist()
        {
            var Filename = DateTime.Now.Ticks.ToString();
            try
            {
                Persist(Filename);
                return Filename;
            }
            catch (Exception)
            {

                throw;
            }
            
            
        }

        public void Persist(string FileName)
        {
            using (FileStream fs = File.Open(FileName, FileMode.Create))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, BlockChain.RetrieveAll());
                }
                catch (SerializationException ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public bool Restore(string Filename)
        {
            using (FileStream fs = File.OpenRead(Filename))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    BlockChain.Restore((List<Block>)bf.Deserialize(fs));
                }
                catch (SerializationException ex)
                {
                    // Error handling
                    return false;
                }
            }
            return true;
        }

    }



}

