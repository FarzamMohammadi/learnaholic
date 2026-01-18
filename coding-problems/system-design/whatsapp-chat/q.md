# CHAT SYSTEM - INTERVIEWER GUIDE

## PHASE 1: PROBLEM STATEMENT (5 min)

**You say:**
> "Design a chat system like WhatsApp or Slack. Users can send messages 1-on-1 or in groups. Start by clarifying the requirements."

**What you're looking for:**
- Do they ask about scale? (users, messages/day)
- Do they clarify features? (group size limits, media, notifications)
- Do they ask about trade-offs? (consistency vs availability, latency requirements)

**Good candidates ask:**
- How many users?
- 1-on-1 only or groups? How large?
- Media support needed?
- Message history requirements?
- Real-time delivery requirement?
- Read receipts / online status?

**Give them:**
- 100M daily active users
- Groups up to 100 people
- Messages must be delivered in real-time (< 1 sec)
- Support images/videos
- Message history should persist
- Read receipts + online status wanted

---

## PHASE 2: HIGH-LEVEL DESIGN (15-20 min)

**You say:**
> "Draw out the high-level architecture. What are the main components?"

**Expected components they should mention:**
- Client (mobile/web)
- API Gateway / Load Balancer
- WebSocket servers (for real-time)
- REST API servers (for HTTP requests)
- Message service
- User service
- Database(s)
- Cache (Redis)
- Message queue
- Storage for media (S3/CDN)
- Notification service

**Red flags:**
- Only REST APIs (no WebSocket/long polling for real-time)
- Single database, no discussion of scaling
- No message queue
- Forget about media storage

**Good signs:**
- Separates WebSocket servers from REST
- Mentions message queue for async processing
- Discusses caching layer
- Separates media storage

---

## PHASE 3: DEEP DIVE TRACKS (30-40 min)

Pick 2-3 of these based on what you want to test or where candidate is strong:

---

### TRACK A: DATABASE DESIGN

**You ask:**
> "Let's design the database schema. What tables do you need and how would you structure them?"

**Questions to drill into:**
1. "What database would you choose and why?"
2. "Show me the schema for users, chats, and messages"
3. "How would you handle group chats?"
4. "How do you query 'get last 50 messages for a chat'?"
5. "What if I want to get all chats for a user sorted by last message time?"
6. "How would you shard this as you scale?"

**What good looks like:**

**Tables they should have:**
- `users` (user_id, username, phone, created_at)
- `chats` (chat_id, type, created_at)
- `chat_members` (chat_id, user_id, joined_at)
- `messages` (message_id, chat_id, sender_id, content, timestamp)
- Optional: `user_messages` (for inbox view per user)

**Database choice:**
- SQL (Postgres) for users/chats → relational data
- NoSQL (Cassandra/MongoDB) for messages → write-heavy, time-series

**Good answer on sharding:**
- Shard messages by `chat_id` (all messages in a chat together)
- Shard by `user_id` for user's inbox view
- May need both views (duplicate data)

**Red flags:**
- Everything in one table
- No thought about indexing
- Can't explain how to get "last N messages" efficiently
- Doesn't consider write-heavy vs read-heavy patterns

---

### TRACK B: REAL-TIME MESSAGING

**You ask:**
> "How does a message get from User A to User B in real-time?"

**Questions to drill into:**
1. "Why use WebSockets instead of HTTP polling?"
2. "What happens when User B is offline?"
3. "How do you route the message to the right WebSocket server?"
4. "What if the WebSocket server crashes mid-delivery?"
5. "How do you ensure messages arrive in order?"
6. "What if User B is connected on multiple devices?"

**What good looks like:**

**Message flow:**
1. Client → WebSocket Server A
2. Server publishes to message queue (Kafka)
3. Message service persists to DB
4. Check if recipient online (Redis lookup)
5. Route to correct WebSocket server
6. Deliver or send push notification

**Key points:**
- Connection mapping in Redis (user_id → ws_server_address)
- Message queue for async processing
- ACKs for delivery confirmation
- Sequence numbers for ordering

**Red flags:**
- Direct server-to-server calls (tight coupling)
- No thought about offline users
- Can't explain how to find which server user is connected to
- No consideration of message ordering

---

### TRACK C: GROUP CHAT SCALING

**You ask:**
> "How would you handle a group chat with 100 members? What about 10,000?"

**Questions to drill into:**
1. "Describe the message flow when someone posts to a 100-person group"
2. "Fan-out on write vs fan-out on read - which would you choose?"
3. "How do you handle read receipts in a group?"
4. "What if 50 people send messages at the same time?"
5. "How would you optimize for very large groups (10K+ members)?"

**What good looks like:**

**For 100 members - Fan-out on write:**
- One message stored
- Create entry in each member's inbox
- Send to all online members via WebSocket
- Queue notifications for offline members

**For 10K members - Fan-out on read:**
- Store message once
- Users fetch on demand
- Cache recent messages
- Skip individual read receipts (just message author)

**Key considerations:**
- Write amplification in fan-out on write
- Read latency in fan-out on read
- Threshold for switching strategies

**Red flags:**
- Same approach for 100 and 10K members
- Doesn't consider write amplification
- No thought about read receipts complexity

---

### TRACK D: MEDIA HANDLING

**You ask:**
> "A user wants to send a 10MB video in chat. Walk me through it."

**Questions to drill into:**
1. "Where do you store the media?"
2. "How do you handle uploads?"
3. "What about previews/thumbnails?"
4. "How do you optimize for global users?"
5. "What if upload fails halfway?"
6. "How do you prevent abuse (spam, inappropriate content)?"

**What good looks like:**

**Upload flow:**
1. Client requests upload URL (pre-signed S3 URL)
2. Client uploads directly to S3
3. Client sends message with media URL
4. Server generates thumbnail
5. Recipients fetch from CDN

**Optimizations:**
- Compression before upload
- CDN for low latency
- Lazy loading
- Different quality levels

**Security:**
- Signed URLs (expire after X minutes)
- File size limits
- Content type validation
- Rate limiting per user

**Red flags:**
- Upload through backend server (bandwidth waste)
- No CDN
- Store in database
- No thought about failed uploads

---

### TRACK E: ONLINE/OFFLINE STATUS

**You ask:**
> "How do you show which users are online?"

**Questions to drill into:**
1. "How do you detect when someone goes offline?"
2. "What if they just have a bad connection temporarily?"
3. "How do you handle 'last seen' timestamp?"
4. "What about privacy (user wants to hide online status)?"
5. "Scale: 1M users online, how do you handle this?"

**What good looks like:**

**Approach:**
- Heartbeat every 30 seconds from client
- Store in Redis: `user:123:online` (TTL 45 seconds)
- If no heartbeat → mark offline
- Publish status changes to interested users

**Last seen:**
- Update on disconnect
- Store in database
- Cache in Redis

**Privacy:**
- User settings table
- Three modes: everyone, contacts, nobody
- Filter before broadcasting status

**Red flags:**
- Poll database constantly
- No heartbeat mechanism
- Doesn't consider privacy
- Store in database only (slow)

---

### TRACK F: ENCRYPTION

**You ask:**
> "Let's add end-to-end encryption. How would you implement it?"

**Questions to drill into:**
1. "What's the difference between transport encryption and E2E encryption?"
2. "How do you exchange keys securely?"
3. "What happens when a new user joins a group?"
4. "How do you handle multiple devices per user?"
5. "Can the server still do message search if it's encrypted?"

**What good looks like:**

**Key concepts:**
- Transport: TLS (server can see content)
- E2E: Only sender/receiver can decrypt

**Implementation (Signal Protocol):**
- Each user has public/private key pair
- Diffie-Hellman key exchange
- Double ratchet algorithm
- Perfect forward secrecy

**Group chats:**
- Sender key protocol
- Key rotation when member joins/leaves

**Multiple devices:**
- Each device has own keys
- Send message multiple times (once per device)

**Trade-offs:**
- Server can't index for search
- Can't show message previews in notifications
- More complex key management

**Red flags:**
- Thinks HTTPS is E2E encryption
- Suggests storing keys on server
- No thought about key distribution
- Doesn't understand forward secrecy

---

### TRACK G: MESSAGE DELIVERY GUARANTEES

**You ask:**
> "How do you ensure messages are never lost?"

**Questions to drill into:**
1. "What if the server crashes right after receiving a message?"
2. "What if the network fails during delivery?"
3. "How do you handle duplicates?"
4. "At-most-once vs at-least-once vs exactly-once - which do you choose?"
5. "What if User B's phone is off for 3 days?"

**What good looks like:**

**Delivery levels:**
- At-most-once: Fast, but may lose messages ❌
- At-least-once: May duplicate, but won't lose ✅
- Exactly-once: Hard in distributed systems

**Choose at-least-once:**
- Acknowledge only after DB write
- Retry on failure
- Deduplicate using message_id

**Implementation:**
```
1. Receive message
2. Persist to DB (durable)
3. Send ACK to sender
4. Attempt delivery to recipient
5. Retry if failed (exponential backoff)
6. Store in offline queue if still offline
```

**Handling long offline:**
- Message queue with retention
- Push notification on reconnect
- Paginated fetch of missed messages

**Red flags:**
- ACK before persisting
- No retry mechanism
- No deduplication
- Doesn't understand CAP theorem

---

### TRACK H: RATE LIMITING & ABUSE PREVENTION

**You ask:**
> "How do you prevent spam and abuse?"

**Questions to drill into:**
1. "How do you rate limit message sending?"
2. "What if someone spams a group with 1000 messages?"
3. "How do you detect bots?"
4. "What about media spam (large files)?"
5. "How do you implement this without slowing down legit users?"

**What good looks like:**

**Rate limiting approaches:**
- Token bucket algorithm
- Sliding window counter
- Fixed window counter

**Implementation:**
- Redis for counter storage
- Per-user limits: 100 msgs/min
- Per-group limits: 1000 msgs/min
- Media limits: 50MB/hour

**Abuse detection:**
- Pattern detection (same message to many users)
- Exponential backoff after violations
- Captcha after threshold
- Temporary bans

**Red flags:**
- No rate limiting
- Only frontend validation
- Database-based rate limiting (slow)
- Same limits for all users

---

## PHASE 4: WRAP UP (5-10 min)

**You ask:**
> "What are the biggest bottlenecks? How would you monitor this system?"

**Look for:**
- **Bottlenecks:** Database writes, WebSocket connections, message queue throughput
- **Metrics:** Message latency (p95, p99), delivery success rate, connection count, queue lag
- **Monitoring:** Prometheus, Grafana, distributed tracing
- **Alerting:** Spike in errors, queue backup, connection drops

---

## SCORING RUBRIC

### Junior (0-2 years)
- Basic architecture with main components
- Simple DB schema
- Understands need for WebSockets
- May miss scaling considerations

### Mid (2-5 years)
- Solid architecture with proper separation
- Good DB design with indexing
- Handles offline scenarios
- Considers caching and queuing
- Discusses some scaling strategies

### Senior (5+ years)
- Comprehensive design with trade-offs
- Multiple DB strategies (SQL + NoSQL)
- Deep understanding of consistency models
- Scaling strategies (sharding, replication)
- Monitoring and observability
- Handles edge cases proactively

### Staff+ (8+ years)
- All of above +
- Discusses cost implications
- Organization/team structure considerations
- Migration strategies
- A/B testing approach
- Capacity planning

---

## QUESTION FLOW CHEAT SHEET

**Start broad:**
→ "Design a chat system"

**Get requirements:**
→ "What should we clarify?"

**High-level:**
→ "Draw the architecture"

**Then pick your path:**
- Want to test DB skills? → Track A (Database Design)
- Want to test real-time systems? → Track B (Real-time Messaging)
- Want to test scale thinking? → Track C (Group Chat Scaling)
- Want to test infrastructure? → Track D (Media Handling)
- Want to test distributed systems? → Track E/F/G
- Want to test security? → Track H

**Mix and match** - spend 10-15 min on each track you choose