import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// El front corre en el puerto 5173 (el API ya lo permite con CORS)
export default defineConfig({
  plugins: [react()],
  server: { port: 5173 },
});
