using System;
using Xunit;
using System.Net.Http;
using System.Collections.Generic;
using StackExchange.Redis;

namespace SimpleBlockChain.Tests
{
    public class NodeTests
    {
        static String RedisIp = "127.0.0.1";
        static String RedisPort = "6379";

        private List<IChainStorageProvider> stores = new List<IChainStorageProvider>(){
            new SimpleBlockChain.InMemoryChainStorageProvider()
            ,new SimpleBlockChain.RedisChainStorageProvider(RedisIp,RedisPort)
        };
        
        // If required, this routine will empty the blockchain storage prior to each test
        public void ResetStore(IChainStorageProvider store){
            switch (store.GetType().ToString())
            {
                case "SimpleBlockChain.RedisChainStorageProvider":
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"{RedisIp}:{RedisPort},allowAdmin=true");
                IServer server = redis.GetServer($"{RedisIp}:{RedisPort}");
                server.FlushDatabase(0);
                break;  
               
            }
        }

        [Fact]
        public void BlockChainNodeTests()
        {
            foreach (IChainStorageProvider store in stores)
            {
                ResetStore(store);

                var Node = new BlockChainNode();
                Node.Initiate(null,store);
                Node.SubmitData("This is my first block of data! :-)");
                Assert.Equal(2, Node.GetChainSize()); // (2 may seem unintuative but this includes the genesis block created as part of initiation
            }

           
        
        }


        [Fact]
        public void InitiateAndAddFiftyBlocks()
        {

            foreach (IChainStorageProvider store in stores)
            {
                ResetStore(store);

                var Node = new BlockChainNode();
                Node.Initiate(null,store);

                for (int i = 1; i < 51; i++)
                {
                    Node.SubmitData($"Block number {i}");
                }

                
                Assert.Equal(51, Node.GetChainSize()); 

            }

        }

        [Fact]
        public void InitiateAndAddFiftyBlocksAndValidate()
        {

            foreach (IChainStorageProvider store in stores)
            {
                ResetStore(store);

                var Node = new BlockChainNode();
                Node.Initiate(null,store);

                for (int i = 1; i < 51; i++)
                {
                    Node.SubmitData($"Block number {i}");
                }


                Assert.Equal(true, Node.ValidateNode());
            }
           
            
        }



        [Fact]
        public void InitiateAndAddOneThousandBlocks()
        {
            foreach (IChainStorageProvider store in stores)
            {
                ResetStore(store);

                var Node = new BlockChainNode(new System.Security.Cryptography.SHA512Managed());
                Node.Initiate(null,store);

                for (int i = 1; i < 1001; i++)
                {
                    Node.SubmitData($"Block number {i}");
                }
                
                Assert.Equal(1001, Node.GetChainSize());
            }
        }


        [Fact]
        public void InitiateAndAddOneHundredBlocksAndPersist()
        {

            foreach (IChainStorageProvider store in stores)
            {
                ResetStore(store);

                var Node = new BlockChainNode(new System.Security.Cryptography.SHA512Managed());
                Node.Initiate(null,store);

                for (int i = 1; i < 101; i++)
                {
                    Node.SubmitData($"Block number {i}");
                }

                var FileName =  DateTime.Now.Ticks.ToString();
                Node.Persist(FileName);
                
                Assert.Equal(true, System.IO.File.Exists(FileName) );

            }

        }

        [Fact]
        public void InitiateAndAddOneHundredBlocksAndPersistAndRestore()
        {

            foreach (IChainStorageProvider store in stores)
            {
                ResetStore(store);

                var Node = new BlockChainNode(new System.Security.Cryptography.SHA512Managed());
                Node.Initiate(null,store);

                for (int i = 1; i < 101; i++)
                {
                    Node.SubmitData($"Block number {i}");
                }
                var FileName = DateTime.Now.Ticks.ToString();
                Node.Persist(FileName);
                var Node2 = new BlockChainNode(new System.Security.Cryptography.SHA512Managed());
                Node2.Initiate(null,null,true);
                Node2.Restore(FileName);

                Assert.Equal(101, Node2.GetChainSize());

            }

        }

        [Fact]
        public void AddFiftyBlocksAndCheckIndividualRetrieval()
        {

            foreach (IChainStorageProvider store in stores)
            {
                ResetStore(store);

                var Node = new BlockChainNode(new System.Security.Cryptography.SHA512Managed());
                Node.Initiate(null,store);

                for (int i = 0; i < 50; i++)
                {
                    Node.SubmitData($"Block number {i+1}");
                }
                
                Block block = Node.RetrieveSingleBlock(22);


                // the reason we expect this to be block with data "21" 
                //  is because of the additional geness block
                Assert.Equal("Block number 21", block.Data); 

            }
        }

            [Fact]
        public void AddFiftyBlocksAndCheckRangeRetrieval()
        {

            foreach (IChainStorageProvider store in stores)
            {
                ResetStore(store);

                var Node = new BlockChainNode(new System.Security.Cryptography.SHA512Managed());
                Node.Initiate(null,store);

                for (int i = 0; i < 50; i++)
                {
                    Node.SubmitData($"Block number {i+1}");
                }
                
                List<Block> blockList = Node.RetrieveMany(22,5);

                Assert.Equal(5,blockList.Count);
                // the reason we expect this to be block with data "21" 
                //  is because of the additional geness block
                for (int i = 0; i <= 4; i++)
                {
                    Assert.Equal($"Block number 2{i+1}", blockList[i].Data); 
                }
                

            }

        }





    }


    public class NodeHttpTests{

        [Fact]
        public void CreateHostAndCheckResponsive()
        {
            var Node = new BlockChainNode();
            Node.Initiate();
            var HttpHost = new BlockChainNodeHttpHost.HttpHost(Node);
            HttpHost.Start();
            using (var client = new HttpClient())
            {
                var response = client.PostAsync("http://localhost:8080/count/", null).Result;
                Assert.Equal(true,response.IsSuccessStatusCode);
            }
        }

        [Fact]
        public void SubmitValidData()
        {
            var Node = new BlockChainNode();
            Node.Initiate();
            var HttpHost = new BlockChainNodeHttpHost.HttpHost(Node);
            HttpHost.Start();
            using (var client = new HttpClient())
            {

                var myContent = "TestBlockData";
                var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                var byteContent = new ByteArrayContent(buffer);

                var response = client.PostAsync("http://localhost:8080/add/", byteContent).Result;
                var contents = response.Content.ReadAsStringAsync().Result;
                Assert.Equal("ADDED|2", contents);
            }
        }

        [Fact]
        public void SubmitInvalidData()
        {
            var Node = new BlockChainNode();
            Node.Initiate();
            var HttpHost = new BlockChainNodeHttpHost.HttpHost(Node);
            HttpHost.Start();
            using (var client = new HttpClient())
            {

                var myContent = "";
                var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                var byteContent = new ByteArrayContent(buffer);

                var response = client.PostAsync("http://localhost:8080/add/", byteContent).Result;
                var contents = response.Content.ReadAsStringAsync().Result;
                Assert.Equal("No data found, cannot add block with no data, that's pointless!", contents);
            }
        }

        [Fact]
        public void GetBlockCountWhenChainNew()
        {
            var Node = new BlockChainNode();
            Node.Initiate();
            var HttpHost = new BlockChainNodeHttpHost.HttpHost(Node);
            HttpHost.Start();
            using (var client = new HttpClient())
            {

                var response = client.PostAsync("http://localhost:8080/count/", null).Result;
                var contents = response.Content.ReadAsStringAsync().Result;
                Assert.Equal("1", contents);
            }
        }

        [Fact]
        public void SubmitNewBlocksAndGetCount()
        {
            var Node = new BlockChainNode();
            Node.Initiate();
            var HttpHost = new BlockChainNodeHttpHost.HttpHost(Node);
            HttpHost.Start();
            using (var client = new HttpClient())
            {

                var myContent = "TestBlockData";
                var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                var byteContent = new ByteArrayContent(buffer);

                for (int i = 0; i < 10; i++)
                {
                    client.PostAsync("http://localhost:8080/add/", byteContent).Wait();
                }

                var response = client.PostAsync("http://localhost:8080/count/", null).Result;
                var contents = response.Content.ReadAsStringAsync().Result;
                Assert.Equal("11", contents);
            }
        }

       



        private void WaitForValueMatch(Func<int?> val1, Func<int?> val2, int MaxSeconds)
        {
            System.DateTime Start = System.DateTime.Now;
            Console.WriteLine($"WFV Start: {Start}");
            System.DateTime MaxWait = System.DateTime.Now.AddSeconds(MaxSeconds);
            Console.WriteLine($"WFV Max Wait: {MaxWait}");
            while (!val1.Invoke().Equals(val2.Invoke()) && MaxWait > System.DateTime.Now)
            {
                System.Threading.Thread.Sleep(100);
            }
            Console.WriteLine($"WFV Completed after (ms): {(System.DateTime.Now - Start).Milliseconds}");
            return;
        }

        private void WaitForValueMatch(Func<int?> val1, int val2, int MaxSeconds)
        {
            System.DateTime Start = System.DateTime.Now;
            Console.WriteLine($"WFV Start: {Start}");
            System.DateTime MaxWait = System.DateTime.Now.AddSeconds(MaxSeconds);
            Console.WriteLine($"WFV Max Wait: {MaxWait}");
            while (!val1.Invoke().Equals(val2) && MaxWait > System.DateTime.Now)
            {
                System.Threading.Thread.Sleep(100);
            }
            Console.WriteLine($"WFV Completed after (ms): {(System.DateTime.Now - Start).Milliseconds}");
            return;
        }

    }

    
}
