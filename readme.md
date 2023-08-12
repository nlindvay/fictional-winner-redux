## Fiddle project to look at no mediation and simply using queues to fulfill typical handler / mediation process.
Based on https://github.com/MassTransit/Sample-Twitch

## TO run just download this repo and boot up docker desktop. Then type ```docker compose up```.
### Technologies used
+ MassTransit
+ OpenTelemetry
+ Jaeger (http://localhost:16686/)
+ Serilog
+ RabbitMQ (to connect to docker container from browser http://localhost:15672/)
+ .NET6.0
+ Docker
+ Swagger (http://localhost:5004/)
+ MongoDB (mongodb://localhost:27017)

### to connect to docker container from browser
+ Jaeger UI: http://localhost:16686/
+ Swagger UI: http://localhost:5004/
+ RabbitMQ UI: http://localhost:15672/
+ MongoDB (use Compass): mongodb://localhost:27017
