using System.Collections.Generic;

namespace SimpleBlockChain{
    public interface IChainStore
    {
        void initiate();
        void Add(Block block);
        long Count();
        Block Retrieve(string blockNumber);
        List<Block> RetrieveAll();
        void Restore(List<Block> chain);
    }
}
