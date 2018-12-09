# OSDR Microservices Pilot #


## RabbitMQ ##

Run RabbitMQ with management

`docker run -d --hostname osdr-rabbit --name osdr-rabbit -p 8080:15672 -p 5671:5671 -p 5672:5672 rabbitmq:3-management`

Access RabbitMQ management in browser at localhost:8080, use `guest/quest` to login

Stop and remove osdr-rabbit container

`docker rm -f osdr-rabbit`

## Redis ##

Run Redis with persistant storage

`docker run --name osdr-redis -d -p 6379:6379 redis redis-server --appendonly yes`

There is a Redis management tool: https://redisdesktop.com/download

## Imaging Worker ##

### Run Imaging Worker locally ##

### Build Imaging Worker docker image ##

### Run Imaging Worker in docker ##
