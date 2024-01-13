# Bitfinex

The solution does not use RPC due to lack of time.
I have used SignalR

There are 2 components for this solution:

1.**Discovery Server:**
  - handles discovery of participants in auctions
  - can be queried to find connected peers
  - handles heartbeat to existing participants


2. **Auction Server**
  - connects to Discovery Server to find out existing peers (via SignalR)
  - is queried by discovery server on a regular basis (see if its alive , and responds to discovery server)
  - handles user actions such as :
    -initiate auctions  
    - place bid
    - finalize auction (if is owner of auction)

  - provides callbacks for user triggered events , or other connected participants:
      - own triggered events 
          - initiate auction 
          - place bid
          - finalize auction (will receive also the winner in a sync manner)
      - other participants triggered events
          - auction created
          - bid placed
          - auction finalized

 - implemented as an interface that could further be integrated in other services /apps


 Further developments:
 
 - currently each auction data is stored on each peer (in memory) , therefore each peer acts as a state machine , and all events must be validated 
 - add database layer (sqlite or a nosql database) to move auctions from memory to database (at least noncritical info)
    - to keep finished auctions and CRUD operations over them
    - analytics for different timeframes
- add caching for intensive auctions (with frequently dumping the state to database)
- implement actor model for each auction (signalR hub or microsoft orleans) to further isolate the auction domain from the user domain
    - each auction would be a hub , and clients would subscribe to hubs related to target auctions to receive selective events on them
    - could also be implemented as a kafka topic/rabbit mq queue , or redis pub/sub 
- implement and separate logic for command validation (place bid, create auction)

P.S: This was a really cool project and i really liked it , and would've gone way deeper and done more (also added grpc) if i had more time.
For any questions please let me know
