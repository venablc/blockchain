using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SimpleBlockChain;
using System;
using System.Net.Http;
using System.Diagnostics;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class BlockChainNodeTests
    {
        [TestMethod]
        public void InitiateAndAddOneBlock()
        {
            var Node = new BlockChainNode();
            Node.Initiate();
            Node.SubmitData("This is my first block of data! :-)");
            Assert.AreEqual(2, Node.GetChainSize()); // (2 may seem unintuative but this includes the genesis block created as part of initiation
        }

        [TestMethod]
        public void InitiateAndAddFiftyBlocks()
        {
            var Node = new BlockChainNode();
            Node.Initiate();

            for (int i = 1; i < 51; i++)
            {
                Node.SubmitData($"Block number {i}");
            }

            
            Assert.AreEqual(51, Node.GetChainSize()); 
        }

        [TestMethod]
        public void InitiateAndAddFiftyBlocksAndValidate()
        {
            var Node = new BlockChainNode();
            Node.Initiate();

            for (int i = 1; i < 51; i++)
            {
                Node.SubmitData($"Block number {i}");
            }


            Assert.AreEqual(true, Node.ValidateNode());
            
        }


        [TestMethod]
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

            Assert.AreEqual(false, Node.ValidateNode());

        }


        [TestMethod]
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

            Assert.AreEqual(false, Node.ValidateNode());

        }


        [TestMethod]
        public void InitiateAndAddOneMillionBlocks()
        {
            var Node = new BlockChainNode(new System.Security.Cryptography.SHA512Managed());
            Node.Initiate();

            for (int i = 1; i < 1000001; i++)
            {
                Node.SubmitData($"Block number {i}");
            }
            
            Assert.AreEqual(1000001, Node.GetChainSize());
        }


        [TestMethod]
        public void InitiateAndAddOneHundredBlocksAndPersist()
        {
            var Node = new BlockChainNode(new System.Security.Cryptography.SHA512Managed());
            Node.Initiate();

            for (int i = 1; i < 101; i++)
            {
                Node.SubmitData($"Block number {i}");
            }

            var FileName =  Node.Persist();
            
            Assert.AreEqual(true, File.Exists(FileName) );
        }

        [TestMethod]
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

            Assert.AreEqual(101, Node2.GetChainSize());
        }

    }

    [TestClass]
    public class NodeHttpHostTests
    {

        [TestMethod]
        public void CreateHostAndCheckResponsive()
        {
            var Node = new BlockChainNode();
            Node.Initiate();
            var HttpHost = new BlockChainNodeHttpHost.HttpHost(Node);
            HttpHost.Start();
            using (var client = new HttpClient())
            {
                var response = client.PostAsync("http://localhost:8080/count/", null).Result;
                Assert.AreEqual(true,response.IsSuccessStatusCode);
            }
        }

        [TestMethod]
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
                Assert.AreEqual("ADDED|2", contents);
            }
        }

        [TestMethod]
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
                Assert.AreEqual("No data found, cannot add block with no data, that's pointless!", contents);
            }
        }

        [TestMethod]
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
                Assert.AreEqual("1", contents);
            }
        }

        [TestMethod]
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
                Assert.AreEqual("11", contents);
            }
        }

       



        private void WaitForValueMatch(Func<int?> val1, Func<int?> val2, int MaxSeconds)
        {
            System.DateTime Start = System.DateTime.Now;
            Debug.WriteLine($"WFV Start: {Start}");
            System.DateTime MaxWait = System.DateTime.Now.AddSeconds(MaxSeconds);
            Debug.WriteLine($"WFV Max Wait: {MaxWait}");
            while (!val1.Invoke().Equals(val2.Invoke()) && MaxWait > System.DateTime.Now)
            {
                System.Threading.Thread.Sleep(100);
            }
            Debug.WriteLine($"WFV Completed after (ms): {(System.DateTime.Now - Start).Milliseconds}");
            return;
        }

        private void WaitForValueMatch(Func<int?> val1, int val2, int MaxSeconds)
        {
            System.DateTime Start = System.DateTime.Now;
            Debug.WriteLine($"WFV Start: {Start}");
            System.DateTime MaxWait = System.DateTime.Now.AddSeconds(MaxSeconds);
            Debug.WriteLine($"WFV Max Wait: {MaxWait}");
            while (!val1.Invoke().Equals(val2) && MaxWait > System.DateTime.Now)
            {
                System.Threading.Thread.Sleep(100);
            }
            Debug.WriteLine($"WFV Completed after (ms): {(System.DateTime.Now - Start).Milliseconds}");
            return;
        }

    }
}
