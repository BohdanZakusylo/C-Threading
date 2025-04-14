# C-Threading

#This is the project of our group ABMB for the C# Threading Class


## Description
This project allows you to search up hotels, flights and AirBnb all in one place. Not only do you get current prices but you alos get old prices with graph to compare how the price is going up or down.
Due to the fact that we are using threading, you can search for all three at the same time and get the results in a matter of seconds.
Due to the fact that we are using an old flight comparising csv, you can only look at flight that happend 10 years ago and that where in the states. Some example are
- New York to Dallas
- Dallas to New York
- New York to Miami

Mainly short distance flight. With the public api you can find any flight you want. Same applies to the other two.
Frontend also has a read me file, that explains how to run the frontend.

## Getting Started
To run this project we have build a docker image that contains all the dependencies. You can run the docker image with the following command:
```bash
docker compose up --build
```
This command should allow you to run the backend.
Please make sure port 8080 is not being used by any other application. If it is, please change the port in the docker compose file.
Please make sure port 5432 is not being used by any other application. If it is, please change the port in the docker compose file.
## API Endpoints
We follow strict conventions for our API endpoints.

## Issues
If you ran into any issues please don't hesitate to contact us.


## Aditional Information
If you need to migrate something in the database run the following command
docker exec -it abmb-webapi-1 dotnet ef database update

## File to use:
For flights, please use ABMB/Data/flights.csv
For hotels, please use ABMB/Data/new_hotels.csv
For airbnb, please use ABMB/Data/airbnb.csv

## Params Data Sample (only for example)
For flights, please use JFK - DFW , 16/04/2025, 18/04/2025, 1 passanger.
For hotels please use Amsterdam, 16/06/2025, 18/06/2025.
For airbnb, please use Turkey, Sapanca, 2025-08-7


## Authors
Bernardo Alves, Mehdi Sadeghi, Bogdan , Ayomide