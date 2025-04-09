import { Injectable } from '@angular/core';
import { WebSocketSubject } from 'rxjs/webSocket';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SocketService {
  private socket$!: WebSocketSubject<any>;

  constructor() {
    this.connectWebSocket();
  }

  private connectWebSocket() {
	  //define endpoint to send messages to the backend
    this.socket$ = new WebSocketSubject('ws://localhost:5000/ws');

	
    this.socket$.subscribe({
      next: (message) => this.handleIncomingMessage(message),
      error: (err) => {
        console.error('WebSocket error:', err);
        setTimeout(() => this.connectWebSocket(), 5000); // Auto-reconnect
      },
      complete: () => {
        console.warn('WebSocket closed, reconnecting...');
        setTimeout(() => this.connectWebSocket(), 5000);
      }
    });
  }
// main method to send the messages to the backend
  sendAction(action: string, data: any) {
    const message = { action, ...data };
    console.log('Sending WebSocket message:', JSON.stringify(message));
    this.socket$.next(message);
  }

  onEvent(): Observable<any> {
    return this.socket$.asObservable();
  }

  private handleIncomingMessage(message: any) {
    console.log('Message received:', message);
  }
}