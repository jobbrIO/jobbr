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
        manualChunks: {
          vendor: ['react', 'react-dom'],
          router: ['react-router-dom'],
          bootstrap: ['bootstrap', 'react-bootstrap'],
          smoothie: ['smoothie'],
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