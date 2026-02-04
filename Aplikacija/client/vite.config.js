import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5000', // ili https://localhost:7014 ako koristiš https
        changeOrigin: true
      }
    }
  },
  plugins: [react()],
})
