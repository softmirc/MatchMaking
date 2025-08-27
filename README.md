# MatchMaking

## Matchmaking Service

A simple matchmaking microservice that listens to a Kafka topic, processes user match requests, and stores match results in Redis.


## MatchMaking.Worker

`MatchMaking.Worker` is a background service responsible for consuming user requests from Kafka, batching them into matches, and publishing completed match data to another Kafka topic. It also stores match information in Redis for fast retrieval.

### Responsibilities:

- Listens to `KAFKA_CONSUME_TOPIC` for incoming matchmaking requests.
- Groups users into matches of size `MATCH_SIZE`.
- Publishes the match result to `KAFKA_PRODUCE_TOPIC`.
- Stores each user's match info in Redis under the key format: `match:{userId}`.
- Ensures thread-safe queue handling with in-memory buffering.

This service runs continuously in the background as part of the main container and is started via `IHostedService` when the application launches.

## Features

- Kafka consumer/producer integration
- Redis-based storage for match data
- Match batching logic
- REST API with Swagger documentation

## Getting Started

Build and run the services:

1. Install and runn [Docker Desktop] https://www.docker.com/products/docker-desktop
2. Open CMD or Terminal window
3. Go to MatchMaking solution folder cd D:\Projects\MatchMaking
4. Input command: docker-compose build
5. Input command: docker-compose up
6. Open your browser and go to swagger documentation: http://localhost:5000/swagger/index.html
7. Expand /match/search api point, click button 'try it out', input user id for example 'user1' and click Execute button
8. Repeat 6 step input userId values 'user2' and 'user3' for complete match
9. For get match info by user id Expand /match/{userId}, input user1, click execute button to see response
