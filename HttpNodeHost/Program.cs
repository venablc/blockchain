using System;
using SimpleBlockChain;


namespace HttpNodeHost
{
    class Program
    {
        static void Main(string[] args)
        {
             var Node = new BlockChainNode();
             Node.Initiate();
             var HttpHost = new BlockChainNodeHttpHost.HttpHost(Node);
             HttpHost.Start();
             Console.WriteLine("Server running, press any key to quit");
             Console.ReadLine();
        }
    }
}
