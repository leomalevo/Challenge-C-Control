# Challenge-C-Control
Aplicacion Angular que manipula 2 ventanas y mediante sockets se replica el movimiento en 2 instancias notepad.exe abiertas (mediante .net)

Este repositorio incluyen ambas aplicaciones Angular y .Net. La aplicacion Angular se ubica en la carpeta "FrontApp". La aplicacion .Net se abre con la solution "WebApplication3.sln"

- Una vez bajado el proyecto completo, actualizar los packages (npm install) y luego correr la aplicacion angular con "ng serve". Abrirá la aplicacion sobre http://localhost:4200/
- Abrir la solucion WebAppplication3, actualizar los paquetes nuget y luego ejecutar la aplicacion  mediante el boton de run. Abrirá en la direccion http://localhost:5000/

- Colocar la direccion http://localhost:4200/ en el browser y se mostrará el login de la aplicacion (por defecto, ya trae creado user: admin y pass: Admin@123)
- Cuando se loguea, automaticamente se abren 2 ventanas emulando las instancias de notepad.exe. Por detras, el backend abre 2 instancias de notepad.exe
- Si se modifica el tamaño, se arrastra o se cierra la ventana en browser, se replica en las instancias de notepad.exe abiertas por el backend. No permite el solapamiento de las ventanas

- DETALLES:
	- Se instaló SWAGGER y muestra los endpoints de la API Autenticacion (http://localhost:5000/swagger/index.html)
	- Se pueden crear mas usuarios mediante la API y se pueden probar en el login
	- Se han agregado comentarios en diferentes bloques de codigo a modo de documentación.
	- Por un problema técnico no pude instalar SQLEXPRESS y a falta de tiempo decidí suplirlo con un dbContext en memoria para almacenar los usuarios. No almacena los estados de las ventanas
	- Entre Backend y Frontend se muestra los mensajes recibidos a modo de logs.
   	- Se agregó un screenshot a modo de muestra de como se ve la aplicacion funcionando (Screenshot.jpg)
