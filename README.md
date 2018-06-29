# Introduction
Dexr is an open-source decentralized cryptocurrency exchange platform developed on a uniquely designed blockchain protocol. The project's goal is to enable high throughput, fast commit of transactions, while also enabling programmable integration with 3rd party applications. 

This project is currently a working prototype and is not a production ready product. See production readiness section for details.

# Features
* **Token Creation**: Enables creation of new tokens on the blockchain.
* **Token Transfer**: Enables transfer of tokens between addresses (public keys) on the blockchain.
* **Limit Orders**: Enables placing limit orders for any token pairs in the order book. 
* **Market Orders**: Enables placing market orders for any token pairs.
* **Order Cancellation**: Allows the owner of an order to cancel an existing order in the order book.
* **On-chain Order Matching**: The matching engine is built-in on the blockchain that ensures true decentralization and data immutability.
* **Adaptive Network Fees**: Enables network fees that automatically scale to the load of the network.
* **Query Indexing**: Allows querying directly on the blockchain for indexed information including wallet balances, token information, and order books.
* **Blockchain Security**: Signature verification, duplicate message handling, pre-commit transactions cross-referencing.

# Technologies & System Specification
* Supported OS: Windows, Linux, OSX
* Language: C#
* Framework: .NET Core 2.0
* Block Interval (Confirmation Time): 15 seconds

# Web API
Dexr's built-in web API server readily enables integration with external party applications. Any application that can send an HTTP request can programmatically access the platform with ease. All transactions sent to the network are transmitted through the web API.

# Getting Started
To get started, see the [Getting Started guide](https://github.com/jnlewis/dexr/blob/master/Dexr%20-%20Getting%20Started.pdf) to set up a Command Line Interface (CLI) node on your machine. Once you have set up your node successfully, you may then send requests to the applications built in API to interact with the blockchain.
For the complete API documentation, please see [Dexr API Documentation](https://documenter.getpostman.com/view/469639/dexr-api/RWEmKcUy).

# Consensus Protocol
The consensus protocol employed in Dexr is Proof-of-Stake (PoS) with Delegated Byzantine Fault Tolerant (dBFT) algorithm, which has been proven competitive in performance and reliability in many existing blockchain platforms today such as Neo, Dash and Ethereum’s Casper. In dBFT algorithms, a speaker node is selected from the network to be responsible for new block creation.

Dexr further enhances this mechanism by leveraging on asynchronous message transmission and non-voting speaker selection. 
* Asynchronous communication eliminates the need to maintain active connections between network nodes. This mechanism also removes the static timer usually required to keep track of block creation intervals. 
* A non-voting speaker selection allows for lower network overhead by eliminating the voting process which requires transmission involving all nodes.

### Network Flow

![Figure: Dexr Network Flow](http://anno.network/docs/dexr-network-flow.jpg)
Figure: Dexr Network Flow

In dBFT algorithms, a speaker is voted in as a delegate to create and propose new blocks for the network. In Dexr, the "Select Speaker" stage does not involve any network overheads or exchange of messages between nodes. 

### Proof-of-Stake
In each epoch, every node on the network runs a selection algorithm locally which produces a common speaker based on Proof-of-Stake and block index. As each node will come to the same conclusion, there is no need for inter-communication. As with Proof-of-Stake protocols, nodes which has a higher stake (native token balance) will have more occurrence to getting selected as speaker.

In Dexr, Proof-of-Stake is linear where a node with 100 token balance will have twice the number of occurrence to be selected as speaker as compared to a node with 50 token balance.

### Consensus Lifecycle

![Figure: Dexr Consensus Lifecycle](http://anno.network/docs/dexr-consensus-lifecycle.jpg)
Figure: Dexr Consensus Lifecycle


# Architecture

### Clients
Clients are any applications that can send an HTTP web request to nodes on the network. All interactions with the blockchain are done via the HTTP web requests other than the following operation: Creating a new chain (building the genesis block).

### Network Nodes
There are two types of nodes on the network; Viewer Nodes and Consensus Nodes. 
Viewer nodes are nodes that serve API requests to clients for the purpose of viewing blockchain state. This includes viewing wallet balance, exchange orders and token information.

Consensus nodes are nodes that participate in the building and maintenance of the blockchain. This includes receiving transactions, creating new blocks and extending the blockchain.

### Transactions Relay
Any consensus node connected to the network can receive transaction requests from clients. When a node receive a transaction, it validates the transaction, adds the transaction to its local pending transactions, and forward the requests to all other connected nodes. In the case where a node receives a transaction but is not ready to validate it (in the middle of syncing chain), it will just forward the request to other nodes.

### Network Fees
All nodes on the network will be compensated with the network fee attached to each transaction. The network fee is awarded to the current speaker node at the end of each block, hence nodes with higher stake (native token balance) are usually awarded higher fees as they are selected as speakers more often (with proof-of-stake protocol).

The minimum fee for each block is adjusted based on the number of transactions from the last block. A larger number of transactions will result in higher fees. Clients are expected to attach at least the minimum fee amount when sending new transactions to the network.

# Production Readiness
The project at its current stage is a working prototype with most of its intended features implemented. However, there are still several areas of improvement before it can be ready to serve in a production environment.

Code restructuring is one area to be looked into, in particular the consensus protocol and order matching engine can be further abstracted and detached from the core libraries to improve maintainability and to aid developers in understanding the code structure.

Block synchronization should also be improved upon by allowing block downloads from multiple nodes in parallel.

Future enhancement can also look into the use of Honey Badger BFT's  (https://eprint.iacr.org/2016/199.pdf) network subset protocol to improve transactions relay to all nodes on the network. This may allow for better scalability of consensus nodes, as currently when any node receives a transaction, it has to broadcast to each and every other node on the network.

# Conclusion
Dexr's unique use of proof-of-stake protocol and delegated byzantine fault tolerant algorithm allows for improved transaction throughput and reduced network overhead. This performance enhancement affords a strong foundation for a reliable decentralized cryptocurrency exchange platform.

