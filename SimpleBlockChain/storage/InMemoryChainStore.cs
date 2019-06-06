using System.Collections.Generic;
using System;

namespace SimpleBlockChain{
    public class InMemoryChainStore : IChainStore
    {
    
        private List<Block> chain;

        private Boolean FirstBlockAdded = false;

        public void initiate(){
            chain = new List<Block>();
        }

        public void Add(Block block)
        {
            FirstBlockAdded = true;
            
            chain.Add(block);
        }

        public long Count()
        {
            return (long)chain.Count;
        }

        public void Restore(List<Block> chain)
        {
            if(FirstBlockAdded){
                throw new Exception("Unable to restore chain, chain has already been started. You can only restore a chain to an empty node.");
            }

            this.chain = chain;
        }

        public Block Retrieve(string blockNumber)
        {
            int i;

            if(int.TryParse(blockNumber,out i)){
                return chain[i-1];
            }

            return null;
            
        }

        public List<Block> RetrieveAll()
        {
            return chain;
        }
    }
}
