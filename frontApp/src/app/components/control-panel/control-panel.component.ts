import { Component, Input  } from '@angular/core';
import { SocketService } from '../../services/socket.service';

@Component({
  selector: 'app-control-panel',
  templateUrl: './control-panel.component.html',
  styleUrls: ['./control-panel.component.css']
})
export class ControlPanelComponent {
  windowIds: number[] = [];
  private nextId = 1;
  private maxWindows = 2; // max limit of windows
  private x=300;
  private y=300;

 constructor(private socketService: SocketService) {}
 
 // listen to the login event
@Input() set loginTrigger(trigger: boolean) { 
		if (trigger) {
		  this.initializeWindows();
		}
	}
// start creating the windows
  initializeWindows() {
	  let positions = this.calculateNonOverlappingPositions(this.maxWindows);

	  for (let i = 0; i < this.maxWindows; i++) {
		this.x = positions[i].x;
		this.y = positions[i].y;
		this.openNewWindow();
	  }
}

// calculate position to avoid overlapping
calculateNonOverlappingPositions(windowCount: number): { x: number, y: number }[] {
  let positions: { x: number, y: number }[] = [];
  let step = 350; // minimum distance between windows 

  for (let i = 0; i < windowCount; i++) {
    let x = 400 + (i * step);
    let y = 100 + (i * step);
    positions.push({ x, y });
  }
  return positions;
}

// register the new opened window and send the message to the backend for opening notepad.exe
  openNewWindow() {
    this.windowIds.push(this.nextId);
    this.socketService.sendAction('open', { id: this.nextId });
	this.moveWindow(this.nextId,this.x,this.y);
    this.nextId++;
  }
// move message for the backend
  moveWindow(id: number, x: number, y: number) {
    this.socketService.sendAction('move', { id, x, y });
  }
// resize message for backend
  resizeWindow(id: number, width: number, height: number) {
    this.socketService.sendAction('resize', { id, width, height });
  }
// close message for the backend
  closeWindow(id: number) {
    this.socketService.sendAction('close', { id });
    this.windowIds = this.windowIds.filter(w => w !== id);
  }
}