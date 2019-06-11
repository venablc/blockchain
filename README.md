# Blockchain

[![Build Status](https://dev.azure.com/venableschris/Blockchain/_apis/build/status/venablc.blockchain?branchName=master)](https://dev.azure.com/venableschris/Blockchain/_build/latest?definitionId=1&branchName=master)

An extensible and flexible C# blockchain implementation, the main purpose of which is to demonstrate basic blockchain concepts.

## Key solution elements / concepts

### Node
The core element is the node, responsible for the basic blockchain functionality (e.g. add, retrieve, validate etc.)

### Chain store
Every node needs to use a storage provider to store the blockchain, there are currently two chain store implementations, InMemory (default) and Redis.
#### InMemory
Blockchain is stored in application memory. This is the default and easiest option to use although doesn't provide any persistance between application restarts (although a function of a node is to "persist" a chain to file as a method of backup that can be easily restored on application restart). A node connector can also be used to replicate your blockchain between applications should one of your node quit unexpectedly.
#### Redis
Blockchain is stored to a specified redis instance, a high performance in memory data store. see https://redis.io/ .

### Node host
Using a node host is optional, you can always use a node directly in your own codebase. A node host acts as a wrapper for your node exposing its blockchain functionality in different way. 
#### HTTP
The "http" node host exposes an HTTP API with basic "GET", "ADD" and "COUNT" functions.

### Node connector
A node connector connects two or more nodes for purpose of replication to aid scalability, resiliency and redundancy. There are currently no completed connectors.

## Future developments
Additional chain stores:
* MongoDb

Additional connectors:
* P2Pconnector
* Mirror

Other:
* Dockerised blockchain node with HTTP node host
* Improved documentation
* Nuget package


## Contributions
If you are interested in contributing or taking part this project then contact venablc@gmail.com
