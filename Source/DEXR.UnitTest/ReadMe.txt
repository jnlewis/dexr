---------------------------------------------
Single-node Test:
---------------------------------------------
- In config.json, set Port to 8080.
- In config.json, set Seed address to "http://localhost:8080" (or use IP address)
- Run Release_1 CLI.exe:
	1. Create new wallet "wallet_owner.json"
	2. Create new chain
	3. Start consensus

(Manual Test/Normal Usage)
- You may now send http requests/transactions to the node api.

(Automated Simulation Test)
- Run the UnitTest application to begin simulation.

---------------------------------------------
Multi-node Test:
---------------------------------------------
- Duplicate the CLI project output (Debug/Release) folder into a folder as follow:
	Multinode/Release_1
	Multinode/Release_2
	Multinode/Release_3
	Multinode/Release_4

- Configure the config.json file as follow:
	Release_1 - Port: 8080, Seed: ["http://localhost:8080"]
	Release_2 - Port: 8081, Seed: ["http://localhost:8080"]
	Release_3 - Port: 8082, Seed: ["http://localhost:8080"]
	Release_4 - Port: 8083, Seed: ["http://localhost:8080"]

- Run Release_1 CLI.exe:
	1. Create new wallet "wallet_owner.json"
	2. Create new chain
	3. Start consensus

- Run Release_2, Release_3, Release_4
	1. Create new wallet "wallet_<number 1,2,3>.json" eg: wallet_1.json
	2. Start consensus

- Give it up to a minute until all nodes is in sync.
- You may now send http requests/transactions to any of the above nodes.
