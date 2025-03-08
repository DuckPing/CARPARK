# CARPARK ASSESSMENT


Simple and Straightforward Demo

MSSQL Database migrated for storing CarPark static info inside CARPARK_MAIN.
Calling the API to get latest CARPARK_DETAIL info.

This project is built without frontend, only swagger to execute the main 2 APIs.

1. POST /api/CarPark/UpdateCarParkDetails, to update latest available car park info.
2. GET /api/CarPark/NearestCarParks, to get nearest available car park by given latitude and longitude.

