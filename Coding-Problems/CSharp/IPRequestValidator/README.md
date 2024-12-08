# IP Request Validator
Tracks and validates IP requests based on frequency in a fixed-size window

## Key Operations
- IsValid: Returns whether IP is valid (≤5 occurrences in window)

## Description
Validates IPs based on frequency within fixed window of most recent 1M requests.
An IP is considered invalid if it appears more than 5 times in that window.