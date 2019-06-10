using System.Collections.Generic;
using System;

namespace SimpleBlockChain{
    public class InMemoryChainStorageProvider : IChainStorageProvider
    {
    
        private List<Block> chain;

        private Boolean FirstBlockAdded = false;

        public void initiate(){
            chain = new List<Block>();
        }

        public Boolean Add(Block block)
        {
            FirstBlockAdded = true;

            chain.Add(block);

            return true;
        }

        public Block PopBlock(){
            return Retrieve(Count());
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

        public Block Retrieve(long blockNumber)
        { 
            if(blockNumber <= Count() && blockNumber > 0){
                return chain[(int)blockNumber-1];
            }
            else{
                return null;
            }
           
        }

        public List<Block> RetrieveAll()
        {
            return chain;
        }

        public List<Block> RetrieveMany(int StartIndex, int Length){

            return chain.GetRange(StartIndex-1,Length);

        }

    }
}
