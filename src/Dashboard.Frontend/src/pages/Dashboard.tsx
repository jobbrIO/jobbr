import React from 'react';
import { Row, Col, Card, Table, Button, Spinner, Badge } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { useRunningJobRuns, useLastFailedJobRuns, useRetryJobRun } from '../hooks';
import { useDiskInfo } from '../hooks';
import { JobRunDto } from '../types';
import { safeToFixed, safeFormatDate, formatEnhancedDuration } from '../utils/formatters';
import CpuGraph from '../components/CpuGraph';
import MemoryGraph from '../components/MemoryGraph';

const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  
  // Use TanStack Query hooks for data fetching
  const { 
    data: runningJobRunsResult, 
    isLoading: isLoadingRunning,
    error: runningError,
    refetch: refetchRunning
  } = useRunningJobRuns();
  
  const { 
    data: failedJobRunsResult, 
    isLoading: isLoadingFailed,
    error: failedError,
    refetch: refetchFailed
  } = useLastFailedJobRuns();
  
  const { 
    data: diskInfo = [], 
    isLoading: isLoadingDisks,
    error: diskError,
    refetch: refetchDisks
  } = useDiskInfo();
  
  const retryJobRunMutation = useRetryJobRun();

  // Extract items from PagedResult
  const runningJobRuns = runningJobRunsResult?.items || [];
  const failedJobRuns = failedJobRunsResult?.items || [];

  const handleRetryJobRun = async (jobRun: JobRunDto) => {
    try {
      await retryJobRunMutation.mutateAsync(jobRun);
      console.log('Job run retried successfully');
    } catch (error) {
      console.error('Error retrying job run:', error);
    }
  };

  const handleRefreshAll = () => {
    refetchRunning();
    refetchFailed();
    refetchDisks();
  };

  const getStatusBadgeVariant = (state: string): string => {
    switch (state) {
      case 'Completed': return 'success';
      case 'Failed': return 'danger';
      case 'Started':
      case 'Processing':
      case 'Running': return 'info';
      case 'Scheduled': return 'warning';
      default: return 'secondary';
    }
  };

  const formatDate = (dateString?: string): string => {
    return safeFormatDate(dateString);
  };

  const isLoading = isLoadingRunning || isLoadingFailed || isLoadingDisks;
  const hasError = runningError || failedError || diskError;

  if (isLoading) {
    return (
      <div className="loading-spinner">
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </div>
    );
  }

  return (
    <div>
      {/* CPU / Memory Graphs */}
      <Row className="mb-4">
        <Col>
          <Card>
            <Card.Header>
              <h5 className="mb-0">
                <i className="fas fa-microchip me-2"></i>
                CPU / Memory
              </h5>
            </Card.Header>
            <Card.Body style={{ backgroundColor: '#22252B', padding: '1rem' }}>
              <Row>
                <Col lg={6} className="mb-3">
                  <h6 className="text-muted mb-2" style={{ color: '#ffc533' }}>CPU Usage</h6>
                  <CpuGraph height={100} enabled={true} />
                </Col>
                <Col lg={6} className="mb-3">
                  <h6 className="text-muted mb-2" style={{ color: '#ffc533' }}>Memory Usage</h6>
                  <MemoryGraph height={100} enabled={true} />
                </Col>
              </Row>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      <Row>
        {/* Running Job Runs */}
        <Col lg={6}>
          <Card>
            <Card.Header className="d-flex justify-content-between align-items-center">
              <h5 className="mb-0">
                <i className="fas fa-sync-alt fa-spin me-2"></i>
                Scheduled & Running Jobs
              </h5>
              <Button 
                variant="outline-primary" 
                size="sm" 
                onClick={handleRefreshAll}
                disabled={isLoading}
                title="Refresh all dashboard data"
              >
                <i className="fas fa-sync-alt me-1"></i>
                Refresh
              </Button>
            </Card.Header>
            <Card.Body>
              {runningJobRuns.length === 0 ? (
                <p className="text-muted mb-0">No jobs are currently running.</p>
              ) : (
                <Table hover size="sm">
                  <thead>
                    <tr>
                      <th>Job ID</th>
                      <th>Job Name</th>
                      <th>Status</th>
                      <th>Progress</th>
                      <th>Planned Start</th>
                      <th>Actual Start</th>
                    </tr>
                  </thead>
                  <tbody>
                    {runningJobRuns.map(jobRun => (
                      <tr 
                        key={jobRun.jobRunId} 
                        className="table-row-clickable"
                        onClick={() => navigate(`/runs/${jobRun.jobRunId}`)}
                      >
                        <td>
                          <span className="text-primary fw-bold">{jobRun.jobId}</span>
                        </td>
                        <td><strong>{jobRun.jobName}</strong></td>
                        <td>
                          <Badge bg={getStatusBadgeVariant(jobRun.state)}>
                            {jobRun.state}
                          </Badge>
                        </td>
                        <td>
                          {jobRun.actualStartUtc && (jobRun.progress || 0) > 0 ? (
                            <div className="d-flex align-items-center">
                              <div className="progress" style={{ width: '100px', height: '20px' }}>
                                <div 
                                  className="progress-bar d-flex align-items-center justify-content-center" 
                                  role="progressbar" 
                                  style={{ width: `${jobRun.progress || 0}%` }}
                                  aria-valuenow={jobRun.progress || 0}
                                  aria-valuemin={0}
                                  aria-valuemax={100}
                                >
                                  <small className="text-white fw-bold">{jobRun.progress || 0}%</small>
                                </div>
                              </div>
                            </div>
                          ) : (
                            <span className="text-muted">-</span>
                          )}
                        </td>
                        <td>{safeFormatDate(jobRun.plannedStartUtc)}</td>
                        <td>
                          <div>
                            <div>{safeFormatDate(jobRun.actualStartUtc)}</div>
                            {jobRun.actualStartUtc && jobRun.plannedStartUtc && (
                              <small className="text-muted">
                                {formatEnhancedDuration(jobRun.actualStartUtc, undefined, jobRun.plannedStartUtc)}
                              </small>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              )}
            </Card.Body>
          </Card>
        </Col>

        {/* Failed Job Runs */}
        <Col lg={6}>
          <Card>
            <Card.Header>
              <h5 className="mb-0">
                <i className="fas fa-exclamation-triangle me-2 text-warning"></i>
                Last Failed Jobs
              </h5>
            </Card.Header>
            <Card.Body>
              {failedJobRuns.length === 0 ? (
                <p className="text-muted mb-0">No recent failures.</p>
              ) : (
                <Table hover size="sm">
                  <thead>
                    <tr>
                      <th>Job Name</th>
                      <th>Failed</th>
                      <th>Action</th>
                    </tr>
                  </thead>
                  <tbody>
                    {failedJobRuns.map(jobRun => (
                      <tr key={jobRun.jobRunId}>
                        <td>
                          <span 
                            className="text-primary cursor-pointer fw-bold"
                            onClick={() => navigate(`/runs/${jobRun.jobRunId}`)}
                          >
                            {jobRun.jobName}
                          </span>
                        </td>
                        <td>{formatDate(jobRun.actualEndUtc)}</td>
                        <td>
                          <Button 
                            size="sm" 
                            variant="outline-primary"
                            onClick={() => handleRetryJobRun(jobRun)}
                          >
                            Retry
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>

      {/* Disk Information */}
      {diskInfo.length > 0 && (
        <Row className="mb-4">
          {diskInfo.map((disk, index) => (
            <Col md={4} key={index}>
              <Card>
                <Card.Body>
                  <Card.Title>{disk.name}</Card.Title>
                  <div className="mb-2">
                    <small className="text-muted">
                      {safeToFixed((disk.totalSpace - disk.freeSpace) / (1024 * 1024 * 1024), 1)} GB / {safeToFixed(disk.totalSpace / (1024 * 1024 * 1024), 1)} GB
                    </small>
                  </div>
                  <div className="progress">
                    <div 
                      className="progress-bar" 
                      role="progressbar" 
                      style={{ width: `${safeToFixed(100 - disk.freeSpacePercentage, 0)}%` }}
                    >
                      {`${safeToFixed(100 - disk.freeSpacePercentage, 0)}%`}
                    </div>
                  </div>
                </Card.Body>
              </Card>
            </Col>
          ))}
        </Row>
      )}
    </div>
  );
};

export default Dashboard;