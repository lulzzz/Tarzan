# Experiments

This file represents some preliminary experiments with Apache Ignite implementation:

# Single node
The table shows time needed to process input file of various size using a single node. 
Flow tracker analyzes input packets in memory and then populates local cache. 

| Source            | Size [B]      | Packets  | Flows    | Tracking [s] | Storing [s] | Total [s]  |
| ----------------- | ------------  | -------- | -------- |------------- | ----------- | ---------- | 
| testbed-12jun-64  | 64,001,244    | 93,421   | 3990     | 0.910        | 1.024     | 2.262        |
| testbed-12jun-128 | 128,001,714   | 188,694  | 10818    | 1.782        | 1.676      | 3.801       |    
| testbed-12jun-256 | 256,001,104   | 364,004  | 18,692   | 3.723        | 2.123      | 6.194       | 
| testbed-12jun-512 | 512,001,825   | 731,797  | 31,709   | 6.658        | 2.665      | 9.637       | 
| testbed-12jun-1024| 1,024,000,703 | 1,418,649 | 51,473  | 12.423       | 3.550     | 16.274       |     
| testbed-12jun-2048| 2,048,001,086 | 2,724,152 | 99,128  | 24.820       | 5.847     | 31.834       |    
| testbed-12jun     | 4,532,085,947 | 5,973,980 | 225,872 | 55.249       | 12.754     | 1:08.361    |     


# Multinode on single host
We measured time needed to complete the task of flow tracking for different chunk sizes and available computing nodes. 
The size of the workload is 5,973,980 packets stored in PCAPs of total size of 4,532,085,947 bytes.

| Source             | Chunk Size    | No Chunks | 1      |  2        | 4         |       8   |        16  |
| ------------------ | ------------- | --------- | ------ | --------  | --------- | --------- | ---------- |
| testbed-12jun-64   | 64,001,244    | 71        | 21.543 | 15.499    | 13.486    | 15.915    | |
| testbed-12jun-128  | 128,001,714   | 36        | 21.172 | 17.259    | | | |
| testbed-12jun-256  | 256,001,104   | 18        | 25.441 | 20.469    | | | |
| testbed-12jun-512  | 512,001,825   | 9         | 24.398 | 26.951    | | | |
| testbed-12jun-1024 | 1,024,000,703 | 5         | 24.004 | 36.628    | | | |
| testbed-12jun-2048 | 2,048,001,086 | 3         | 36.297 | 01:08.249 | | | |