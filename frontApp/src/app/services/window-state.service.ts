import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class WindowStateService {
  private windowsState: any[] = [];

  saveWindowState(id: number, x: number, y: number, width: number, height: number) {
    this.windowsState[id] = { x, y, width, height };
  }

  getWindowState(id: number) {
    return this.windowsState[id];
  }
}