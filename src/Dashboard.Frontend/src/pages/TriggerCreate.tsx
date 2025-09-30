import React, { useState, useEffect } from 'react';
import { Card, Row, Col, Spinner, Alert, Button, Form, ButtonGroup } from 'react-bootstrap';
import { useParams, useNavigate } from 'react-router-dom';
import { useApi } from '../context/ApiContext';
import { JobDto } from '../types';

const TriggerCreate: React.FC = () => {
  const { apiClient } = useApi();
  const navigate = useNavigate();
  const { jobId } = useParams<{ jobId: string }>();
  
  const [job, setJob] = useState<JobDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Form state
  const [triggerType, setTriggerType] = useState<string>('Instant');
  const [isActive, setIsActive] = useState<boolean>(true);
  const [comment, setComment] = useState<string>('');
  const [userId, setUserId] = useState<string>('');
  const [userDisplayName, setUserDisplayName] = useState<string>('');
  const [definition, setDefinition] = useState<string>('');
  const [startDateTimeUtc, setStartDateTimeUtc] = useState<string>('');
  const [endDateTimeUtc, setEndDateTimeUtc] = useState<string>('');
  const [delayedMinutes, setDelayedMinutes] = useState<number>(0);
  const [parameters, setParameters] = useState<string>('{}');

  useEffect(() => {
    if (jobId) {
      loadJobDetails();
    }
  }, [jobId]);

  const loadJobDetails = async () => {
    if (!jobId) return;
    
    try {
      setLoading(true);
      setError(null);
      
      const jobIdNum = parseInt(jobId);
      const jobResult = await apiClient.getJob(jobIdNum);
      setJob(jobResult);
      
      // Set default values
      const now = new Date();
      now.setMinutes(now.getMinutes() + 5);
      setStartDateTimeUtc(now.toISOString().slice(0, 16));
    } catch (err) {
      setError(`Failed to load job details: ${err instanceof Error ? err.message : String(err)}`);
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      setError(null);

      // Basic validation
      if (triggerType === 'Recurring' && !definition.trim()) {
        setError('Cron expression is required for recurring triggers');
        return;
      }

      if ((triggerType === 'Scheduled' || triggerType === 'Instant') && !startDateTimeUtc) {
        setError('Start date/time is required');
        return;
      }

      // Parse parameters JSON
      let parsedParameters = null;
      if (parameters.trim()) {
        try {
          parsedParameters = JSON.parse(parameters);
        } catch {
          setError('Invalid JSON in parameters');
          return;
        }
      }

      let newTrigger: any; // Will be a specific trigger type

      const baseProps = {
        isActive,
        comment: comment.trim(),
        userId: userId.trim() || undefined,
        userDisplayName: userDisplayName.trim() || undefined,
        parameters: parsedParameters,
      };

      if (triggerType === 'Instant') {
        newTrigger = {
          triggerType: 'Instant' as const,
          ...baseProps,
          delayedMinutes,
        };
      } else if (triggerType === 'Scheduled') {
        newTrigger = {
          triggerType: 'Scheduled' as const,
          ...baseProps,
          startDateTimeUtc,
        };
      } else if (triggerType === 'Recurring') {
        newTrigger = {
          triggerType: 'Recurring' as const,
          ...baseProps,
          definition: definition.trim(),
          startDateTimeUtc,
          endDateTimeUtc: endDateTimeUtc || undefined,
        };
      } else {
        setError('Invalid trigger type');
        return;
      }

      await apiClient.createTrigger(newTrigger, parseInt(jobId!));
      
      // Navigate back to job detail
      navigate(`/jobs/${jobId}`);
    } catch (err) {
      setError(`Failed to create trigger: ${err instanceof Error ? err.message : String(err)}`);
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
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

  if (error && !job) {
    return (
      <Alert variant="danger">
        <Alert.Heading>Error</Alert.Heading>
        <p>{error}</p>
        <Button variant="outline-danger" onClick={loadJobDetails}>
          Try Again
        </Button>
      </Alert>
    );
  }

  if (!job) {
    return (
      <Alert variant="warning">
        <Alert.Heading>Not Found</Alert.Heading>
        <p>Job not found</p>
      </Alert>
    );
  }

  return (
    <div className="container-fluid">
      <div className="row">
        <div className="col-12">
          <h3>
            <i className="fas fa-plus me-2"></i>
            Create Trigger for {job.title}
          </h3>
          <p className="text-muted">{job.type}</p>
        </div>
      </div>

      {error && (
        <Alert variant="danger" className="mb-3">
          {error}
        </Alert>
      )}

      <Card>
        <Card.Body>
          <Row>
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>Type</Form.Label>
                <ButtonGroup className="d-block">
                  <Button 
                    variant={triggerType === 'Instant' ? 'primary' : 'outline-primary'}
                    onClick={() => setTriggerType('Instant')}
                  >
                    Instant
                  </Button>
                  <Button 
                    variant={triggerType === 'Scheduled' ? 'primary' : 'outline-primary'}
                    onClick={() => setTriggerType('Scheduled')}
                  >
                    Scheduled
                  </Button>
                  <Button 
                    variant={triggerType === 'Recurring' ? 'primary' : 'outline-primary'}
                    onClick={() => setTriggerType('Recurring')}
                  >
                    Recurring
                  </Button>
                </ButtonGroup>
              </Form.Group>
            </Col>
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>Status</Form.Label>
                <ButtonGroup className="d-block">
                  <Button 
                    variant={isActive ? 'success' : 'outline-success'}
                    onClick={() => setIsActive(true)}
                  >
                    Enabled
                  </Button>
                  <Button 
                    variant={!isActive ? 'danger' : 'outline-danger'}
                    onClick={() => setIsActive(false)}
                  >
                    Disabled
                  </Button>
                </ButtonGroup>
              </Form.Group>
            </Col>
          </Row>

          <Row>
            <Col md={12}>
              <Form.Group className="mb-3">
                <Form.Label>Comment</Form.Label>
                <Form.Control 
                  type="text" 
                  value={comment}
                  onChange={(e) => setComment(e.target.value)}
                  placeholder="Enter a comment for this trigger"
                />
              </Form.Group>
            </Col>
          </Row>

          <Row>
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>User ID</Form.Label>
                <Form.Control 
                  type="text" 
                  value={userId}
                  onChange={(e) => setUserId(e.target.value)}
                  placeholder="Enter user ID (optional)"
                />
              </Form.Group>
            </Col>
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>User Display Name</Form.Label>
                <Form.Control 
                  type="text" 
                  value={userDisplayName}
                  onChange={(e) => setUserDisplayName(e.target.value)}
                  placeholder="Enter user display name (optional)"
                />
              </Form.Group>
            </Col>
          </Row>

          {triggerType === 'Recurring' && (
            <Row>
              <Col md={12}>
                <Form.Group className="mb-3">
                  <Form.Label>Cron Expression *</Form.Label>
                  <Form.Control 
                    type="text" 
                    value={definition}
                    onChange={(e) => setDefinition(e.target.value)}
                    placeholder="0 0 12 * * ?"
                    required
                  />
                  <Form.Text className="text-muted">
                    Enter a valid cron expression (e.g., "0 0 12 * * ?" for daily at noon)
                  </Form.Text>
                </Form.Group>
              </Col>
            </Row>
          )}

          <Row>
            {triggerType !== 'Instant' && (
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Start Date/Time *</Form.Label>
                  <Form.Control 
                    type="datetime-local" 
                    value={startDateTimeUtc}
                    onChange={(e) => setStartDateTimeUtc(e.target.value)}
                    required
                  />
                </Form.Group>
              </Col>
            )}
            {triggerType === 'Recurring' && (
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>End Date/Time</Form.Label>
                  <Form.Control 
                    type="datetime-local" 
                    value={endDateTimeUtc}
                    onChange={(e) => setEndDateTimeUtc(e.target.value)}
                  />
                  <Form.Text className="text-muted">
                    Optional end date for recurring triggers
                  </Form.Text>
                </Form.Group>
              </Col>
            )}
            {triggerType === 'Instant' && (
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Delay (minutes)</Form.Label>
                  <Form.Control 
                    type="number" 
                    min="0"
                    step="1"
                    value={delayedMinutes}
                    onChange={(e) => setDelayedMinutes(parseInt(e.target.value) || 0)}
                  />
                  <Form.Text className="text-muted">
                    Number of minutes to delay execution (0 = immediate)
                  </Form.Text>
                </Form.Group>
              </Col>
            )}
          </Row>

          <Row>
            <Col md={12}>
              <Form.Group className="mb-4">
                <Form.Label>
                  <i className="fas fa-wrench me-2"></i>
                  Trigger Parameters
                </Form.Label>
                <Form.Control 
                  as="textarea"
                  rows={6}
                  value={parameters}
                  onChange={(e) => setParameters(e.target.value)}
                  placeholder='{"key": "value"}'
                  style={{ fontFamily: 'monospace' }}
                />
                <Form.Text className="text-muted">
                  Optional JSON parameters for this trigger
                </Form.Text>
              </Form.Group>
            </Col>
          </Row>

          <Row>
            <Col md={12}>
              <div className="d-flex gap-2">
                <Button 
                  variant="primary" 
                  onClick={handleSave}
                  disabled={saving}
                >
                  {saving ? (
                    <>
                      <Spinner animation="border" size="sm" className="me-2" />
                      Creating...
                    </>
                  ) : (
                    'Create Trigger'
                  )}
                </Button>
                <Button 
                  variant="secondary" 
                  onClick={handleCancel}
                  disabled={saving}
                >
                  Cancel
                </Button>
              </div>
            </Col>
          </Row>
        </Card.Body>
      </Card>
    </div>
  );
};

export default TriggerCreate;