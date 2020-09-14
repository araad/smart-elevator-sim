# smart-elevator-sim

smart-elevator-sim is a .Net Core application that simulates how modern smart elevators work. It is based on a scheduling algorithm that will assign the next available elevator to the caller. As of now, the algorithm has a naive implementation that will compare elevators based on total duration of all trips assigned to each elevator and returns the one that will be available next.

This project contains 4 parts:

- **Scheduling service**: Single instance service repsonsible for receiving requests to use an elevator and assigning the next available elevator to the caller.
- **Call Panel service**: Represents the Call Panel found on each floor used to input which floor the caller wants to go to. The Call Panel service will call the Scheduling service and informs the caller which elevator to go to.
- **Client app**: A simple UI (built with Angular 10) where the caller can input the floor they wish to go to. For this demo app, it also shows the entire state of the elevators in an `NxM` grid where `N` is the number of elevators and `M` is the number of floors. The UI also shows every elevator and its state along with its trip request queue to help while testing and debugging.
- **Common lib**: Includes common files and configuration.

To get a real-time reading of each elevator's state and its trip request queues, this project uses SignalR to create a `hub` on the Scheduling service that is publishing the data to the client. The client is using the the following npm package https://www.npmjs.com/package/@microsoft/signalr which contains a TypeScript client library used to subscribe to those notifications.

In a real world scenario, there would be a Call Panel service instance running for each physical Call Panel found in the building. For testing purposes, the Call Panel service includes a background service that randomly generates trip requests for different floors. For now, this can be turned off by commenting out the following line in the corresponding Startup.cs file:

```csharp
services.AddHostedService<CallSimulatorService>();
```

Or can be configured by modifying the arguments passed to the random number generator in the `CallSimulatorService.cs` file shown below:

```csharp
// Wait between 2 - 20 seconds to generate a new trip request
Thread.Sleep(rand.Next(2, 20) * 1000);
```

## Run the project

1. Build and run the services

```bash
# scheduling-service on port 5000
cd scheduling-service
dotnet run

# call-panel-service on port 5002
cd call-panel-service
dotnet run
```

2. Build and run the client app

```bash
# client-app on port 4200
cd client-app
npm install
npm run start
```

3. Open browser and go to http://localhost:4200

## TODO

- Use Docker Compose
- Add ASP<span></span>.NET Core Identity
- Persistent storage with a database using Entity Framework Core
- Improve scheduling algorithm (Inspiration: http://www.columbia.edu/~cs2035/courses/ieor4405.S13/p14.pdf)

## Screenshots

![Alt](images/elevator-grid.png 'Elevator grid')

![Alt](images/call-panel-input.png 'Call Panel: select floor')

![Alt](images/call-panel-result.png 'Call Panel: showing available elevator')

![Alt](images/elevator-queues.png 'Elevator states and queues')
