import { HttpClient } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit, OnDestroy {
  connection: HubConnection;
  elevators: Array<any>;
  floors: Array<any>;

  constructor(private httpClient: HttpClient, private dialog: MatDialog) {}

  ngOnInit() {
    this.connection = new HubConnectionBuilder()
      .withUrl('/elevator-tracking')
      .build();

    this.connection.on('newTripRequest', (elevator, trip) => {
      console.log('newTripRequest \televator:', elevator, '\ttrip:', trip);
      let _elevator = this.elevators.find((e) => e.id === elevator.id);
      _elevator.currentFloor = elevator.currentFloor;
      _elevator.doorsOpen = elevator.doorsOpen;
      _elevator.currentDirection = elevator.currentDirection;
      _elevator.queue = elevator.queue;
      let queue: Array<any> = _elevator.queue;

      let _trip = queue.find((t) => t.id === trip.id);

      if (!_trip) {
        queue.push(trip);
      }
    });

    this.connection.on('tripStarted', (elevator, trip) => {
      console.log('tripStarted \televator:', elevator, ' \ttrip:', trip);
      let _elevator = this.elevators.find((e) => e.id === elevator.id);
      _elevator.currentFloor = elevator.currentFloor;
      _elevator.currentDirection = elevator.currentDirection;
      _elevator.doorsOpen = elevator.doorsOpen;
      _elevator.queue = elevator.queue;
      let queue: Array<any> = _elevator.queue;
      let _trip = queue.find((t) => t.id === trip.id);

      if (!_trip) {
        queue.unshift(trip);
      }
    });

    this.connection.on('tripUpdate', (elevator, trip) => {
      console.log('tripUpdate \televator:', elevator, ' \ttrip:', trip);
      let _elevator = this.elevators.find((e) => e.id === elevator.id);
      _elevator.currentFloor = elevator.currentFloor;
      _elevator.currentDirection = elevator.currentDirection;
      _elevator.doorsOpen = elevator.doorsOpen;
      _elevator.queue = elevator.queue;
      let queue: Array<any> = _elevator.queue;
      let _trip = queue.find((t) => t.id === trip.id);
      if (_trip) {
        _trip.status = trip.status;
      } else {
        queue.unshift(trip);
      }
    });

    this.connection.on('tripEnded', (elevator, trip) => {
      console.log('tripEnded \televator:', elevator, ' \ttrip:', trip);
      let _elevator = this.elevators.find((e) => e.id === elevator.id);
      _elevator.currentFloor = elevator.currentFloor;
      _elevator.doorsOpen = elevator.doorsOpen;
      _elevator.currentDirection = elevator.currentDirection;
      _elevator.queue = elevator.queue;
      let queue: Array<any> = _elevator.queue;

      let _trip = queue.find((t) => t.id === trip.id);
      if (_trip && queue.indexOf(_trip) === 0) {
        queue.shift();
      }
    });

    this.connection.start().then(() => console.log('started'));

    this.httpClient
      .get('/api/schedule/floor-count')
      .subscribe((result: number) => {
        this.floors = [];
        for (let i = result; i > 0; i--) {
          this.floors.push({ level: i });
        }
      });

    this.httpClient
      .get('/api/schedule/elevators')
      .subscribe((result: Array<any>) => {
        this.elevators = result;
      });
  }

  ngOnDestroy() {
    if (this.connection) {
      this.connection.stop();
    }
  }

  onCallButtonClick(origin: number, tpl) {
    this.dialog.open(tpl, {
      width: '100vw',
      height: '100vh',
      maxWidth: 500,
      maxHeight: 500,
      data: {
        submitted: false,
        response: null,
        origin,
      },
    });
  }

  onFloorButtonClick(data, destination: number) {
    data.submitted = true;
    data.destination = destination;
    this.httpClient
      .post('/api/call-panel', { origin: data.origin, destination })
      .subscribe((elevatorId) => {
        data.response = elevatorId;
      });
  }
}
