# Cafetería – Front-end (React)

Pantallas para usar el API de la cafetería: productos, clientes y pedidos.

## Qué necesitas
- Node.js (versión LTS) – https://nodejs.org
- El API de C# ejecutándose (en Visual Studio, con F5) en http://localhost:5101

## Cómo correrlo
1. Abre una terminal dentro de esta carpeta (`cafeteria-front`).
2. Instala las dependencias (solo la primera vez):
   npm install
3. Arranca el front:
   npm run dev
4. Abre http://localhost:5173 en el navegador.

## Si algo falla
- "No se pudo conectar con el API": el API no está corriendo. Ejecútalo primero en Visual Studio.
- Si el API usa otro puerto, cambia `URL_BASE` en `src/api.js`.
- Si el front usa otro puerto distinto al 5173, agrégalo en `WithOrigins(...)` en el `Program.cs` del API.
