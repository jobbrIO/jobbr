import React, { useState, useEffect } from 'react';
import { Table, Spinner, Alert } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { useApi } from '../context/ApiContext';
import { JobDto, PagedResult } from '../types';
import { safeFormatDate } from '../utils/formatters';

const Jobs: React.FC = () => {
  const { apiClient } = useApi();
  const navigate = useNavigate();
  const [jobs, setJobs] = useState<PagedResult<JobDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadJobs();
  }, []);

  const loadJobs = async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await apiClient.getAllJobs();
      setJobs(result);
    } catch (err) {
      setError(`Failed to load jobs: ${err instanceof Error ? err.message : String(err)}`);
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString?: string): string => {
    return safeFormatDate(dateString);
  };

  const handleJobClick = (jobId: number) => {
    navigate(`/jobs/${jobId}`);
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
        <p>{error}</p>
        <button className="btn btn-outline-danger" onClick={loadJobs}>
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