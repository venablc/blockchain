using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBlockChain
{
    public interface INodeConnector
    {



        /// <summary>
        /// Broadcast the creation of a new block to all connected nodes.
        /// </summary>
        /// <param name="NewBlock"></param>
        void BroadcastNewBlockAdd(Block NewBlock);

        /// <summary>
        /// Validates the given node against all other connected nodes, if 
        ///  provided chain matches at least 80% of connected chains then
        ///  the node is considered valid
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        bool ValidateNode(BlockChainNode Node);

        /// <summary>
        /// Gets a copy of the blockchain from the primary connected node (that is the node
        /// for which details were provided as part of the INodeConnector constructor)
        /// </summary>
        /// <returns></returns>
        List<Block> RequestInitialBlockChain();


         int ConnectedNodeCount();

         int TotalNodeCount();
        void RegisterNode(BlockChainNode blockChainNode);
    }
}
