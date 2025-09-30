import React, { useState, useEffect } from 'react';
import { Row, Col, Card, Table, Button, Spinner, Badge } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { useApi } from '../context/ApiContext';
import { JobRunDto, DiskInfoDto } from '../types';
import { safeToFixed, safeFormatDate, formatEnhancedDuration } from '../utils/formatters';
import CpuGraph from '../components/CpuGraph';
import MemoryGraph from '../components/MemoryGraph';

const Dashboard: React.FC = () => {
  const { apiClient } = useApi();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [runningJobRuns, setRunningJobRuns] = useState<JobRunDto[]>([]);
  const [failedJobRuns, setFailedJobRuns] = useState<JobRunDto[]>([]);
  const [diskInfo, setDiskInfo] = useState<DiskInfoDto[]>([]);

  const RUNNING_UPDATE_INTERVAL = 2000;
  const FAILED_UPDATE_INTERVAL = 5000;

  useEffect(() => {
    loadDashboardData();
    
    const runningInterval = setInterval(loadRunningJobRuns, RUNNING_UPDATE_INTERVAL);
    const failedInterval = setInterval(loadFailedJobRuns, FAILED_UPDATE_INTERVAL);

    return () => {
      clearInterval(runningInterval);
      clearInterval(failedInterval);
    };
  }, []);

  const loadDashboardData = async () => {
    try {
      await Promise.all([
        loadRunningJobRuns(),
        loadFailedJobRuns(),
        loadSystemInfo()
      ]);
    } catch (error) {
      console.error('Error loading dashboard data:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadRunningJobRuns = async () => {
    try {
      const result = await apiClient.getRunningJobRuns();
      setRunningJobRuns(result.items);
    } catch (error) {
      console.error('Error loading running job runs:', error);
    }
  };

  const loadFailedJobRuns = async () => {
    try {
      const result = await apiClient.getLastFailedJobRuns();
      setFailedJobRuns(result.items);
    } catch (error) {
      console.error('Error loading failed job runs:', error);
    }
  };

  const loadSystemInfo = async () => {
    try {
      const disks = await apiClient.getDiskInfo();
      setDiskInfo(disks);
    } catch (error) {
      console.error('Error loading system info:', error);
    }
  };

  const handleRetryJobRun = async (jobRun: JobRunDto) => {
    try {
      await apiClient.retryJobRun(jobRun);
      await loadFailedJobRuns();
      console.log('Job run retried successfully');
    } catch (error) {
      console.error('Error retrying job run:', error);
    }
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

  if (loading) {
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
            <Card.Header>
              <h5 className="mb-0">
                <i className="fas fa-sync-alt fa-spin me-2"></i>
                Scheduled & Running Jobs
              </h5>
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