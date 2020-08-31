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

  update(elevator) {
    let existingElevator = this.elevators.find((e) => e.id === elevator.id);
    existingElevator.currentFloor = elevator.currentFloor;
    existingElevator.doorsOpen = elevator.doorsOpen;
    existingElevator.currentDirection = elevator.currentDirection;
    existingElevator.queue = elevator.queue;
    existingElevator.currentTrip = elevator.currentTrip;
  }

  ngOnInit() {
    this.connection = new HubConnectionBuilder()
      .withUrl('/elevator-tracking')
      .build();

    this.connection.on('newTripRequest', (elevator) => {
      console.log('newTripRequest', elevator);
      this.update(elevator);
    });

    this.connection.on('tripStarted', (elevator) => {
      console.log('tripStarted', elevator);
      this.update(elevator);
    });

    this.connection.on('tripUpdate', (elevator) => {
      console.log('tripUpdate', elevator);
      this.update(elevator);
    });

    this.connection.on('tripEnded', (elevator) => {
      console.log('tripEnded', elevator);
      this.update(elevator);
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
