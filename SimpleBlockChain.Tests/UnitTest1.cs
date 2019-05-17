using System;
using Xunit;
using System.Net.Http;

namespace SimpleBlockChain.Tests
{
    public class NodeTests
    {
        [Fact]
        public void BlockChainNodeTests()
        {

            var Node = new BlockChainNode();
            Node.Initiate();
            Node.SubmitData("This is my first block of data! :-)");
            Assert.Equal(2, Node.GetChainSize()); // (2 may seem unintuative but this includes the genesis block created as part of initiation
        

        }


        [Fact]
        public void InitiateAndAddFiftyBlocks()
        {
            var Node = new BlockChainNode();
            Node.Initiate();

            for (int i = 1; i < 51; i++)
            {
                Node.SubmitData($"Block number {i}");
            }

            
            Assert.Equal(51, Node.GetChainSize()); 
        }

        [Fact]
        public void InitiateAndAddFiftyBlocksAndValidate()
        {
            var Node = new BlockChainNode();
            Node.Initiate();

            for (int i = 1; i < 51; i++)
            {
                Node.SubmitData($"Block number {i}");
            }


            Assert.Equal(true, Node.ValidateNode());
            
        }


        [Fact]
        public void InitiateAndAddFiftyBlocksAndTamperAndValidate()
        {
            var Node = new BlockChainNode();
            Node.Initiate();

            for (int i = 1; i < 51; i++)
            {
                Node.SubmitData($"Block number {i}");
            }



            Node.SetBlockChain(new System.Collections.Generic.List<Block>() {new Block(){
                     BlockHash= "bad block",
                     Data = "bad block",
                     Index= 0,
                     PreviousBlockHash= "bad block",
                     TimeStamp= DateTime.Now } });

            Assert.Equal(false, Node.ValidateNode());

        }


        [Fact]
        public void InitiateAndAddFiftyBlocksAndTamperAndValidate2()
        {
            var Node = new BlockChainNode();
            Node.Initiate();

            for (int i = 1; i < 51; i++)
            {
                Node.SubmitData($"Block number {i}");
            }

            var Node2 = new BlockChainNode();
            Node2.Initiate();
            Node2.SubmitData("This is my first block of data! :-)");

            var BadChain = Node2.GetBlockChain();
            var BadBlock = new Block()
            {
                Data = "bad block",
                Index = 0,
                PreviousBlockHash = "bad block",
                TimeStamp = DateTime.Now
            };
            BadBlock.GenerateHash(new System.Security.Cryptography.SHA1CryptoServiceProvider());
            BadChain.AddRange(new System.Collections.Generic.List<Block>() { BadBlock });
            

            Node.SetBlockChain(BadChain);

            Assert.Equal(false, Node.ValidateNode());

        }


        [Fact]
        public void InitiateAndAddOneMillionBlocks()
        {
            var Node = new BlockChainNode(new System.Security.Cryptography.SHA512Managed());
            Node.Initiate();

            for (int i = 1; i < 1000001; i++)
            {
                Node.SubmitData($"Block number {i}");
            }
            
            Assert.Equal(1000001, Node.GetChainSize());
        }


        [Fact]
        public void InitiateAndAddOneHundredBlocksAndPersist()
        {
            var Node = new BlockChainNode(new System.Security.Cryptography.SHA512Managed());
            Node.Initiate();

            for (int i = 1; i < 101; i++)
            {
                Node.SubmitData($"Block number {i}");
            }

            var FileName =  Node.Persist();
            
            Assert.Equal(true, System.IO.File.Exists(FileName) );
        }

        [Fact]
        public void InitiateAndAddOneHundredBlocksAndPersistAndRestore()
        {
            var Node = new BlockChainNode(new System.Security.Cryptography.SHA512Managed());
            Node.Initiate();

            for (int i = 1; i < 101; i++)
            {
                Node.SubmitData($"Block number {i}");
            }
            var FileName = Node.Persist();
            var Node2 = new BlockChainNode(new System.Security.Cryptography.SHA512Managed());
            Node2.Initiate();
            Node2.Restore(FileName);

            Assert.Equal(101, Node2.GetChainSize());
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
