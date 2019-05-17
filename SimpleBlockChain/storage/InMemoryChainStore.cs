using System.Collections.Generic;

namespace SimpleBlockChain{
    public class InMemoryChainStore : IChainStore
    {
    
        private List<Block> chain;

        public void initiate(){
            chain = new List<Block>();
        }

        public void Add(Block block)
        {
            chain.Add(block);
        }

        public long Count()
        {
            return (long)chain.Count;
        }

        public void Restore(List<Block> chain)
        {
            this.chain = chain;
        }

        public Block Retrieve(string blockNumber)
        {
            int i;

            if(int.TryParse(blockNumber,out i)){
                return chain[i];
            }

            return null;
            
        }

        public List<Block> RetrieveAll()
        {
            return chain;
        }
    }
}
