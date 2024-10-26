import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig(() => {
  return {
    plugins: [react()],
    define: {
      __ISSUES_SERVICE_API_URL__: JSON.stringify(process.env.ISSUES_SERVICE_API_URL ?? "http://localhost:7201")
    }
  }
})