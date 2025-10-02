# System Design Clarification Questions

## 1. Scope & Functional Requirements
1. What are the core user workflows/use cases that must be supported in v1?
2. Which features are must-haves vs. nice-to-haves for the initial release?
3. What is explicitly out of scope for this project?
4. What additional features are planned after initial release? How much do we need to consider evolvability?

## 2. Scale & Performance
5. How many users do we expect in first 6 months? 1 year? 5 years?
6. What is the expected request volume? (requests per second/minute at peak)
7. What is the expected read vs. write ratio?
8. What is the geographic distribution of users? (single region, multi-region, global?)

## 3. Data Requirements
9. What is the expected data volume? (number of records, total storage size)
10. What is the data retention policy? (how long to keep data, archival strategy)
11. Are there specific data residency requirements? (data must stay in certain regions/countries)

## 4. Non-Functional Requirements
12. What are the acceptable SLAs? (e.g., 99.9% uptime, p99 latency < 200ms)
13. What are the priorities if we face trade-offs? (consistency vs. availability during network partitions - CAP theorem)
14. What are the most critical non-functional requirements? (latency, availability, consistency, security, etc.)

## 5. Technical Constraints & Integrations
15. Are there any constraints on the technology stack? (e.g., must use existing infrastructure, specific programming languages, cloud providers)
16. What external systems/APIs must we integrate with?
17. Are there existing authentication/authorization systems we should use?

## 6. Operational & Maintenance
18. What monitoring and alerting capabilities are expected?
19. Who will operate and maintain this system? (internal team, ops team, fully managed)
20. What are the disaster recovery requirements? (RPO/RTO targets)

## 7. Business & Timeline
21. How much time do we have to implement the solution/system?
22. What is the budget for this project? (infrastructure costs, development costs)
23. Are there any legal or compliance requirements we need to consider? (GDPR, HIPAA, PCI-DSS, etc.)