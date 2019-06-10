using System.Collections.Generic;
using StackExchange.Redis;
using System;


namespace SimpleBlockChain{
    public class RedisChainStorageProvider : IChainStorageProvider
    {

        public string RedisIp;
        public string RedisPort;
        public string RedisUsername;
        public string RedisPassword;
    
        private Boolean FirstBlockAdded = false;
       
        ConnectionMultiplexer redis;

        public RedisChainStorageProvider(string Ip, string Port){
            RedisIp = Ip;
            RedisPort = Port;
        }

        public void initiate(){
            redis = ConnectionMultiplexer.Connect($"{RedisIp}:{RedisPort}");
        }

        public Boolean Add(Block block)
        {
            
            FirstBlockAdded = true;

           

            IDatabase db = redis.GetDatabase();
            db.HashSet((Count()+1).ToString(),new HashEntry[] { 
                new HashEntry("hash", block.BlockHash)
                ,new HashEntry("data", block.Data)
                ,new HashEntry("previousHash", block.PreviousBlockHash==null?"":block.PreviousBlockHash)
                ,new HashEntry("timeStamp", block.TimeStamp.Ticks)
            });
            db.StringIncrement("BlockCount");

            return true;
                        
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


            FirstBlockAdded = true;

            IDatabase db = redis.GetDatabase();

           foreach (var block in chain)
           {
                db.HashSet((Count()+1).ToString(),new HashEntry[] { 
                    new HashEntry("hash", block.BlockHash)
                    ,new HashEntry("data", block.Data)
                    ,new HashEntry("previousHash", block.PreviousBlockHash==null?"":block.PreviousBlockHash)
                    ,new HashEntry("timeStamp", block.TimeStamp.Ticks)
            });
            db.StringIncrement("BlockCount");
           }

            
        }
        public Block PopBlock(){
            return Retrieve(Count());
        }
        public Block Retrieve(long blockNumber)
        {

            if(blockNumber <= Count() && blockNumber > 0){
                 Block block = new Block();
                IDatabase db = redis.GetDatabase();
                
                HashEntry[] hashes = db.HashGetAll($"{blockNumber}");         
          
                block =  new Block(){
                    TimeStamp =  new DateTime(long.Parse(hashes[3].Value)),
                    Data = hashes[1].Value,
                    BlockHash = hashes[0].Value,
                    PreviousBlockHash = hashes[2].Value
                };

                return block;     
            }
            else{
                return null;
            }
                

        }

        public List<Block> RetrieveMany(int StartIndex, int Length){

            if((StartIndex + Length) > Count()){
                throw new Exception("Range exceed the blocks currently stored");
            }
            
            var t = new List<Block>();

            for (int i = StartIndex; i <= (StartIndex + Length - 1); i++)
            {
                t.Add(Retrieve(i));
            }

            return t;

        }



        public List<Block> RetrieveAll()
        {

            List<Block> list = new List<Block>();

            for (int i = 1; i <= this.Count(); i++)
            {
                list.Add(this.Retrieve(i));
            }

            return list;

        }
    }
}
