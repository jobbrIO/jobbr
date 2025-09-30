import React from 'react';
import { Table, Spinner, Alert } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { useJobs } from '../hooks';
import { safeFormatDate } from '../utils/formatters';

const Jobs: React.FC = () => {
  const navigate = useNavigate();
  
  // Use TanStack Query hook for data fetching
  const { 
    data: jobs, 
    isLoading: loading, 
    error, 
    refetch 
  } = useJobs();

  const formatDate = (dateString?: string): string => {
    return safeFormatDate(dateString);
  };

  const handleJobClick = (jobId: number) => {
    navigate(`/jobs/${jobId}`);
  };

  const handleRetry = () => {
    refetch();
  };

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '200px' }}>
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </div>
    );
  }

  if (error) {
    return (
      <Alert variant="danger">
        <Alert.Heading>Error</Alert.Heading>
        <p>{error instanceof Error ? error.message : String(error)}</p>
        <button className="btn btn-outline-danger" onClick={handleRetry}>
          Try Again
        </button>
      </Alert>
    );
  }

  return (
    <div className="container-fluid">
      <div className="row mt-4">
        <div className="col-sm-12">
          <h3>
            <i className="fas fa-calendar-alt me-2"></i>
            Jobs
          </h3>
          
          {(!jobs || jobs.totalItems === 0) ? (
            <p className="text-muted">No jobs found.</p>
          ) : (
            <>
              <p className="text-muted mb-3">
                Showing {jobs.items.length} of {jobs.totalItems} jobs
              </p>
              
              <Table hover responsive>
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Name</th>
                    <th>Type</th>
                    <th>Created</th>
                    <th>Last Update</th>
                  </tr>
                </thead>
                <tbody>
                  {jobs.items.map(job => (
                    <tr 
                      key={job.id} 
                      className="table-row-clickable"
                      onClick={() => handleJobClick(job.id)}
                      style={{ cursor: 'pointer' }}
                    >
                      <td>
                        <span className="text-primary fw-bold">{job.id}</span>
                      </td>
                      <td>
                        <strong>{job.uniqueName}</strong>
                        {job.title && job.title !== job.uniqueName && (
                          <div className="text-muted small">{job.title}</div>
                        )}
                      </td>
                      <td>
                        <span className="font-monospace text-muted">{job.type}</span>
                      </td>
                      <td>{formatDate(job.createdDateTimeUtc)}</td>
                      <td>{formatDate(job.updatedDateTimeUtc)}</td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default Jobs;