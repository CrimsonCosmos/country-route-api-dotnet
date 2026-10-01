import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// The production build lands in the ASP.NET Core app's wwwroot, so a single
// Azure App Service serves both the UI and the API.
export default defineConfig({
  plugins: [react()],
  build: { outDir: '../src/CountryRoute.Api/wwwroot', emptyOutDir: true },
  server: { proxy: { '/api': 'http://localhost:5080' } },
});
