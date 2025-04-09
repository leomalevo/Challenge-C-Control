import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppComponent } from './app.component';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';


// Importar el módulo de formularios para el login
import { FormsModule } from '@angular/forms';

// Importar el módulo de sockets para la comunicación en tiempo real
import { SocketIoModule, SocketIoConfig } from 'ngx-socket-io';

// Configuración del servidor WebSocket
const config: SocketIoConfig = { url: 'http://localhost:5000', options: {} };

// Importar los componentes
import { LoginComponent } from './components/login/login.component';
import { ControlPanelComponent } from './components/control-panel/control-panel.component';
import { WindowManagerComponent } from './components/window-manager/window-manager.component';
import { WindowComponent } from './components/window/window.component';

// Importar los servicios
import { SocketService } from './services/socket.service';
import { WindowStateService } from './services/window-state.service';

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    FormsModule,
    SocketIoModule.forRoot(config),
	LoginComponent, // Importar los componentes standalone
    ControlPanelComponent,
    WindowManagerComponent,
    WindowComponent,
	CommonModule,
	HttpClientModule
  ],
  providers: [SocketService, WindowStateService],
  bootstrap: [AppComponent]
})
export class AppModule { }