using System;
using SimpleBlockChain;


namespace BlockChainNodeHttpHost.HttpFunctions
{
    public class GetBlock : IHttpFunction
    {
        

        public string HttpAction(BlockChainNode Node, String data)
        {
            Console.WriteLine($"Http host has received a request get block number {data}");

            int BlockNumber;

            if (int.TryParse(data, out BlockNumber) != true)
            {
                return "Invalid block number. Block number must be integer";
            }

            if (Node.GetBlockChain().Count < BlockNumber)
            {
                return "Invalid Block. Block number doesn't exist";
            }

            return Node.GetBlockChain()[BlockNumber].Data;


        }
    }
}
