# LRU (Least Recently Used) Cache Implementation
Provides fixed-size cache with O(1) operations for Get/Put with LRU eviction policy

## Key Operations
- Get: Retrieves value by key, marks as most recently used
- Put: Adds/updates key-value pair, evicts least recently used item when full

## Description
Original and SOLID implementations demonstrate different architectural approaches for the same functionality.