using System.Collections.Generic;
using StackExchange.Redis;
using System;


namespace SimpleBlockChain{
    public class RedisChainStore : IChainStore
    {

        public string RedisIp;
        public string RedisPort;
        public string RedisUsername;
        public string RedisPassword;
    
        private Boolean FirstBlockAdded = false;
       
        ConnectionMultiplexer redis;

        public RedisChainStore(string Ip, string Port){
            RedisIp = Ip;
            RedisPort = Port;
        }

        public void initiate(){
            redis = ConnectionMultiplexer.Connect($"{RedisIp}:{RedisPort}");
        }

        public void Add(Block block)
        {
            //TODO: we need to implement a locking system here to prevent issues when
            //      using multiple nodes and two similtanoues submissions from two
            //      seperate nodes take place.
            FirstBlockAdded = true;

            IDatabase db = redis.GetDatabase();
            db.HashSet((Count()+1).ToString(),new HashEntry[] { 
                new HashEntry("hash", block.BlockHash)
                ,new HashEntry("data", block.Data)
                ,new HashEntry("Index", block.Index)
                ,new HashEntry("previousHash", block.PreviousBlockHash==null?"":block.PreviousBlockHash)
                ,new HashEntry("timeStamp", block.TimeStamp.Ticks)
            });
            db.StringIncrement("BlockCount");
                        
        }

        public long Count()
        {

            IDatabase db = redis.GetDatabase();

            var i = db.StringGet("BlockCount");

            return (int)i;
        }

        public void Restore(List<Block> chain)
        {
            if(FirstBlockAdded){
                throw new Exception("Unable to restore chain, chain has already been started. You can only restore a chain to an empty node.");
            }

        }

        public Block Retrieve(string blockNumber)
        {
            Block block = new Block();
            IDatabase db = redis.GetDatabase();
            
            HashEntry[] hashes = db.HashGetAll($"{blockNumber}");         
          
            block =  new Block(){
                Index = (int)hashes[2].Value,
                TimeStamp =  new DateTime(long.Parse(hashes[4].Value)),
                Data = hashes[1].Value,
                BlockHash = hashes[0].Value,
                PreviousBlockHash = hashes[3].Value
            };

           return block;          

        }



        public List<Block> RetrieveAll()
        {

            List<Block> list = new List<Block>();

            for (int i = 1; i <= this.Count(); i++)
            {
                list.Add(this.Retrieve(i.ToString()));
            }

            return list;

        }
    }
}
