using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Net;
using System.Threading.Tasks;

namespace SimpleBlockChain
{

    public class BlockChainNode
    {

        
        private List<Block> BlockChain;
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

        

        public void Initiate(INodeConnector NodeConnector = null)
        {
            if (BlockChain == null) { BlockChain = new List<Block>(); };
            
            if(NodeConnector != null)
            {
                this.NodeConnector = NodeConnector;
                this.NodeConnector.RegisterNode(this);
            }

            if(NodeConnector == null || NodeConnector?.TotalNodeCount() == 0)
            {
                // This node either has no connector (is therefore independent) or has a connector but
                // is only listening (hasn't connected to an existing network, possibly the first for a new network)
                AddGenesisBlock();
            }
            else
            {
                // This node if part of a network of nodes, populate our blockchain with that
                // of the primary connected node.


                BlockChain = NodeConnector.RequestInitialBlockChain();
                if(!(BlockChain?.Count > 0))
                {
                    throw new Exception("Unable to retrieve initial blockchain from connected node");
                }
                
            }

            

            Initiated = true;
        
        }





        private void AddGenesisBlock()
        {

            var Block = new Block()
            {
                TimeStamp = DateTime.Now,
                Data = "DGPKMANWJQKPDFN1237234PKSDFMSDFH39394",
                Index = 1,
                PreviousBlockHash = null
            };
            Block.GenerateHash(_HashAlgorithm);
            BlockChain.Add(Block);
        }

        public int SubmitData(String data)
        {

            if (Initiated != true)
                throw new NodeNotReadyException();

            // todo: make this function thread safe

            var block = GenerateBlock(data);
            BlockChain.Add(block);
            if (NodeConnector != null)
            {
                NodeConnector.BroadcastNewBlockAdd(block);
            }
            

            return BlockChain.Count;
        }

        public Block GenerateBlock(string data)
        {

            var Block = new Block()
            {
                Data = data,
                TimeStamp = DateTime.Now,
                Index = BlockChain.Count,
                PreviousBlockHash = BlockChain[BlockChain.Count - 1].BlockHash
            };

            Block.GenerateHash(_HashAlgorithm);

            return Block;

        }

        
        public class  NodeNotReadyException : Exception { }

        public int? GetChainSize()
        {
            return BlockChain?.Count;
        }

        public void SetBlockChain(List<Block> chain)
        {
            this.BlockChain = chain;
        }
        
        public List<Block> GetBlockChain()
        {
            return this.BlockChain;
        }

        internal void PutBroadcastedBlock(Block data)
        {

            if (data.PreviousBlockHash.Equals(BlockChain[BlockChain.Count - 1].BlockHash))
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

            foreach (Block item in BlockChain)
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


    }



}

