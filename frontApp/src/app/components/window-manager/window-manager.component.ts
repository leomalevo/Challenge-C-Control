import { Component, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { SocketService } from '../../services/socket.service';

@Component({
  selector: 'app-window-manager',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './window-manager.component.html',
  styleUrls: ['./window-manager.component.css']
})
export class WindowManagerComponent {
  windows: any[] = [];
  activeWindow: any = null;
  offsetX = 0;
  offsetY = 0;

  constructor(private socketService: SocketService) {
    this.socketService.onEvent().subscribe((data: any) => {
      this.syncWindows(data);
    });
	
	// listen close event for closing the window
	this.socketService.onEvent().subscribe((data: any) => {
		if (data.action === "closed") {
		  this.removeWindow(data.id);
		} else {
		  this.syncWindows(data);
		}
	  });

  }


  syncWindows(updatedWindows: any) {
  
  const windowsArray = Array.isArray(updatedWindows) ? updatedWindows : [updatedWindows];

  windowsArray.forEach((newWin) => {
    const existingWin = this.windows.find(w => w.id === newWin.id);
    if (existingWin) {
      Object.assign(existingWin, newWin);
    } else {
      this.windows.push(newWin);
    }
  });
}

  startDrag(win: any, event: MouseEvent) {
    this.activeWindow = win;
    this.offsetX = event.clientX - win.x;
    this.offsetY = event.clientY - win.y;
  }

// start moving window
 @HostListener('document:mousemove', ['$event'])
	onMouseMove(event: MouseEvent) {
	  if (!this.activeWindow) return;

	  const newX = event.clientX - this.offsetX;
	  const newY = event.clientY - this.offsetY;

	  // check overlapping
	  const overlapping = this.windows.some(w => 
		w.id !== this.activeWindow.id && // should not compare to same window
		newX < w.x + w.width && newX + this.activeWindow.width > w.x &&
		newY < w.y + w.height && newY + this.activeWindow.height > w.y
	  );

	  if (!overlapping) { // only move if there no overlapping
		this.activeWindow.x = newX;
		this.activeWindow.y = newY;

		this.socketService.sendAction('move', { 
		  id: this.activeWindow.id, 
		  x: newX, 
		  y: newY, 
		  width: this.activeWindow.width, 
		  height: this.activeWindow.height 
		});
	  }
	}
// stop moving window
  @HostListener('document:mouseup')
  stopDrag() {
    this.activeWindow = null;
  }
// resize window
  resizeWindow(win: any, newWidth: number, newHeight: number) {
	  this.socketService.sendAction('resize', { 
		id: win.id, 
		width: newWidth, 
		height: newHeight,
		x: win.x,
		y: win.y 
	  });

	  win.width = newWidth;
	  win.height = newHeight;
}

//close window
  closeWindow(win: any) {
    this.windows = this.windows.filter(w => w.id !== win.id);
    this.socketService.sendAction('close', { id: win.id });
  }
//remove from the register of windows  
  removeWindow(id: number) {
	this.windows = this.windows.filter(w => w.id !== id);
	}

}