import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  css: {
    preprocessorOptions: {
      scss: {
      }
    }
  },
  build: {
    outDir: 'dist',
    assetsDir: 'assets',
    sourcemap: true,
    rollupOptions: {
      output: {
        manualChunks: (id) => {
          if (id.includes('node_modules/react') || id.includes('node_modules/react-dom')) {
            return 'vendor';
          }
          if (id.includes('node_modules/react-router-dom')) {
            return 'router';
          }
          if (id.includes('node_modules/bootstrap') || id.includes('node_modules/react-bootstrap')) {
            return 'bootstrap';
          }
          if (id.includes('node_modules/smoothie')) {
            return 'smoothie';
          }
        }
      }
    }
  },
  server: {
    port: 1338,
    proxy: {
      '/api': {
        target: 'http://localhost:1338',
        changeOrigin: true,
      },
      '/config': {
        target: 'http://localhost:1338',
        changeOrigin: true,
      },
      '/system': {
        target: 'http://localhost:1338',
        changeOrigin: true,
      },
      '/cron': {
        target: 'http://localhost:1338',
        changeOrigin: true,
      }
    }
  }
})