import React from 'react'
import { BrowserRouter as Router, Routes, Route, useLocation } from 'react-router-dom'
import { Container } from 'react-bootstrap'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import Navigation from './components/Navigation'
import Dashboard from './pages/Dashboard'
import Jobs from './pages/Jobs'
import JobDetail from './pages/JobDetail'
import JobDetailRuns from './pages/JobDetailRuns'
import Runs from './pages/Runs'
import RunDetail from './pages/RunDetail'
import TriggerDetail from './pages/TriggerDetail'
import ErrorBoundary from './components/ErrorBoundary'
import { ApiProvider } from './context/ApiContext'
import { SystemInfoProvider } from './context/SystemInfoContext'

// Create a client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 2,
      refetchOnWindowFocus: false,
      staleTime: 5 * 60 * 1000, // 5 minutes
      gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
    },
    mutations: {
      retry: 1,
    },
  },
})

const AppContent: React.FC = () => {
  const location = useLocation();
  
  // Only enable system info polling when on dashboard routes
  const isDashboardRoute = location.pathname === '/' || location.pathname === '/dashboard';

  return (
    <SystemInfoProvider enabled={isDashboardRoute}>
      <div className="app">
        <Navigation />
        <Container fluid as="main" role="main">
          <ErrorBoundary>
            <Routes>
              <Route path="/" element={<ErrorBoundary><Dashboard /></ErrorBoundary>} />
              <Route path="/dashboard" element={<ErrorBoundary><Dashboard /></ErrorBoundary>} />
              <Route path="/jobs" element={<ErrorBoundary><Jobs /></ErrorBoundary>} />
              <Route path="/jobs/:id" element={<ErrorBoundary><JobDetail /></ErrorBoundary>} />
              <Route path="/jobs/:jobId/runs" element={<ErrorBoundary><JobDetailRuns /></ErrorBoundary>} />
              <Route path="/jobs/:jobId/triggers/:triggerId" element={<ErrorBoundary><TriggerDetail /></ErrorBoundary>} />
              <Route path="/runs" element={<ErrorBoundary><Runs /></ErrorBoundary>} />
              <Route path="/runs/:id" element={<ErrorBoundary><RunDetail /></ErrorBoundary>} />
            </Routes>
          </ErrorBoundary>
        </Container>
      </div>
    </SystemInfoProvider>
  );
};

function App() {
  return (
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <ApiProvider>
          <Router>
            <AppContent />
          </Router>
        </ApiProvider>
      </QueryClientProvider>
    </ErrorBoundary>
  )
}

export default App