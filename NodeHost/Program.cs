using SimpleBlockChain;
using System;

namespace NodeHost
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting host");


            //var P2PConnector = new NodeP2PConnector(null,null,"",111);
            //P2PConnector.Initiate(3, 10);
            var Node = new SimpleBlockChain.BlockChainNode();
            Node.Initiate();
            var HttpHost = new BlockChainNodeHttpHost.HttpHost(Node);
            HttpHost.Start();



            //var Connector1 = new NodeP2PConnector(null, null, "127.0.0.1", 1001);
            //Connector1.Initiate();
            //var Node = new BlockChainNode();
            //Node.Initiate(Connector1);

            //var Connector2 = new NodeP2PConnector(System.Net.IPAddress.Parse("127.0.0.1"), 1001, "127.0.0.1", 1002);
            //Connector2.Initiate(1, 5);
            //var Node2 = new BlockChainNode();
            //Node2.Initiate(Connector2);

            //var Connector3 = new NodeP2PConnector(System.Net.IPAddress.Parse("127.0.0.1"), 1002, "127.0.0.1", 1003);
            //Connector3.Initiate(1, 5);
            //var Node3 = new BlockChainNode();
            //Node3.Initiate(Connector3);

            //while (1 == 1)
            //{

            //    Console.WriteLine("listening for 5 seconds");

            //    System.Threading.Thread.Sleep(5000);


            //}


        }
    }
}
