import React from 'react';
import { useParams } from 'react-router-dom';
import Runs from './Runs';

const JobDetailRuns: React.FC = () => {
  const { jobId } = useParams<{ jobId: string }>();
  
  // The Runs component will automatically filter by jobId when the parameter is present
  return <Runs />;
};

export default JobDetailRuns;