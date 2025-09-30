import React, { useState, useEffect } from 'react';
import { Card, Spinner, Alert, Button, Badge, Row, Col } from 'react-bootstrap';
import { useParams, useNavigate } from 'react-router-dom';
import { useApi } from '../context/ApiContext';
import { JobTriggerDto } from '../types';
import { safeFormatDate } from '../utils/formatters';

const TriggerDetail: React.FC = () => {
  const { apiClient } = useApi();
  const navigate = useNavigate();
  const { jobId, triggerId } = useParams<{ jobId: string; triggerId: string }>();
  
  const [trigger, setTrigger] = useState<JobTriggerDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (jobId && triggerId) {
      loadTriggerDetails();
    }
  }, [jobId, triggerId]);

  const loadTriggerDetails = async () => {
    if (!jobId || !triggerId) return;
    
    try {
      setLoading(true);
      setError(null);
      
      const result = await apiClient.getTrigger(parseInt(jobId), parseInt(triggerId));
      setTrigger(result);
    } catch (err) {
      setError(`Failed to load trigger details: ${err instanceof Error ? err.message : String(err)}`);
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString?: string): string => {
    return safeFormatDate(dateString);
  };

  // Safely render parameter values that might be objects
  const renderParameter = (param: any): string => {
    if (param === null || param === undefined) {
      return '';
    }
    
    if (typeof param === 'string') {
      return param;
    }
    
    if (typeof param === 'object') {
      try {
        return JSON.stringify(param, null, 2);
      } catch {
        return String(param);
      }
    }
    
    return String(param);
  };

  const handleViewJob = () => {
    navigate(`/jobs/${jobId}`);
  };

  const handleViewJobRuns = () => {
    navigate(`/jobs/${jobId}/runs`);
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

  if (error || !trigger) {
    return (
      <Alert variant="danger">
        <Alert.Heading>Error</Alert.Heading>
        <p>{error || 'Trigger not found'}</p>
        <Button variant="outline-danger" onClick={loadTriggerDetails}>
          Try Again
        </Button>
      </Alert>
    );
  }

  return (
    <div className="container-fluid">
      <div className="row">
        <div className="col-12">
          <h3>
            <i className="fas fa-clock me-2"></i>
            Trigger #{trigger.id}
          </h3>
          <p className="text-muted">Job {jobId} Trigger Details</p>
        </div>
      </div>

      <Row>
        <Col lg={8}>
          <Card className="mb-4">
            <Card.Header>
              <h5 className="mb-0">Trigger Information</h5>
            </Card.Header>
            <Card.Body>
              <Row className="mb-2">
                <Col sm={3}><strong>Trigger ID:</strong></Col>
                <Col sm={9}>{trigger.id}</Col>
              </Row>
              <Row className="mb-2">
                <Col sm={3}><strong>Job ID:</strong></Col>
                <Col sm={9}>
                  <Button variant="link" className="p-0" onClick={handleViewJob}>
                    {jobId}
                  </Button>
                </Col>
              </Row>
              <Row className="mb-2">
                <Col sm={3}><strong>Type:</strong></Col>
                <Col sm={9}>
                  <Badge bg="secondary">{trigger.triggerType}</Badge>
                </Col>
              </Row>
              <Row className="mb-2">
                <Col sm={3}><strong>Status:</strong></Col>
                <Col sm={9}>
                  <Badge bg={trigger.isActive ? 'success' : 'secondary'}>
                    {trigger.isActive ? 'Active' : 'Inactive'}
                  </Badge>
                </Col>
              </Row>
              <Row className="mb-2">
                <Col sm={3}><strong>User:</strong></Col>
                <Col sm={9}>{trigger.userDisplayName || trigger.userId || '-'}</Col>
              </Row>
              {trigger.triggerType === 'Instant' && trigger.delayedMinutes > 0 && (
                <Row className="mb-2">
                  <Col sm={3}><strong>Delayed Minutes:</strong></Col>
                  <Col sm={9}>{trigger.delayedMinutes}</Col>
                </Row>
              )}
              {trigger.comment && (
                <Row className="mb-2">
                  <Col sm={3}><strong>Comment:</strong></Col>
                  <Col sm={9}>{trigger.comment}</Col>
                </Row>
              )}
              {trigger.parameters && (
                <Row className="mb-2">
                  <Col sm={3}><strong>Parameters:</strong></Col>
                  <Col sm={9}>
                    <pre className="bg-light p-2 rounded small">{renderParameter(trigger.parameters)}</pre>
                  </Col>
                </Row>
              )}
            </Card.Body>
          </Card>
        </Col>

        <Col lg={4}>
          <Card>
            <Card.Header>
              <h5 className="mb-0">Actions</h5>
            </Card.Header>
            <Card.Body className="d-grid gap-2">
              <Button variant="primary" onClick={handleViewJob}>
                <i className="fas fa-calendar-alt me-2"></i>
                View Job
              </Button>
              <Button variant="outline-secondary" onClick={handleViewJobRuns}>
                <i className="fas fa-flag-checkered me-2"></i>
                View Job Runs
              </Button>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default TriggerDetail;