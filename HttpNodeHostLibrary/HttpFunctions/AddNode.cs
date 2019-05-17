using System;
using SimpleBlockChain;

namespace BlockChainNodeHttpHost.HttpFunctions
{
    public class AddBlock : IHttpFunction
    {
        public string HttpAction(BlockChainNode Node, String data)
        {
            Console.WriteLine("Http host has received a request to add data to the block chain.");

            if (String.IsNullOrEmpty(data))
            {
                return "No data found, cannot add block with no data, that's pointless!";
            }

            long i = Node.SubmitData(data);

            if (i <= 0){
                return "Data not added";
            }
            else
            {
                return $"ADDED|{i}";
            }

  
        }
    }
}
