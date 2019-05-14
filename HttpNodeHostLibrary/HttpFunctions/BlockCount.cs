using System;
using SimpleBlockChain;


namespace BlockChainNodeHttpHost.HttpFunctions
{
    public class BlockCount : IHttpFunction
    {
        public string HttpAction(BlockChainNode Node, String data)
        {
            Console.WriteLine("Http host has received a request to count chain size.");
            
            int i = Node.GetBlockChain().Count;

            return i.ToString();


        }
    }
}
