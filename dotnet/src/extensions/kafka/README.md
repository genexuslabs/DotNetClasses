# GeneXus Messaging EXO for Apache Kafka

GeneXus Messaging EXO for Apache Kafka provides a Producer and Consumer API to integrate GeneXus applications with Apache Kafka, enabling publish-subscribe and stream processing capabilities.

## Features ##
-	Producer API: Publish messages to Kafka topics.
-	Consumer API: Subscribe to Kafka topics and process messages.

## Prerequisites ##
-	Apache Kafka: Ensure you have a running Kafka instance. You can use Confluent Kafka or set up Kafka locally using Docker.

## Setting up Kafka with Docker ##

You can use the docker-compose.yml file included in this repository to quickly set up a Kafka environment. The file is located at:
dotnet/src/extensions/kafka/test/docker-compose.yml

1.	Navigate to the directory containing the docker-compose.yml file:
	```cd dotnet/src/extensions/kafka/test```
   
2.	Start the Kafka environment using Docker Compose:

	```docker-compose up -d```

3. Verify that the Kafka and Zookeeper containers are running:
	```docker ps```
  
4.	Once the containers are running, you can use the Producer and Consumer APIs to interact with Kafka.

## Documentation ##
For detailed documentation, visit https://wiki.genexus.com/commwiki/wiki?40593,Kafka+Producer+and+Consumer+External+Objects
