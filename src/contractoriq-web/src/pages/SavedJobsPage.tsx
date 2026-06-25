import { useState, useEffect } from 'react';
import { getSavedJobs } from '../api/jobs';
import type { Job, PagedResult } from '../types';
import JobCard from '../components/JobCard';

export default function SavedJobsPage() {
  const [jobs, setJobs] = useState<PagedResult<Job> | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getSavedJobs().then(res => {
      setJobs(res.data);
      setLoading(false);
    });
  }, []);

  const handleUnsave = (id: string) => {
    setJobs(prev => prev ? {
      ...prev,
      items: prev.items.filter(j => j.id !== id),
      totalCount: prev.totalCount - 1,
    } : prev);
  };

  return (
    <div style={{ maxWidth: '900px', margin: '0 auto', padding: '24px 16px' }}>
      <h1 style={{ fontSize: '1.3rem', fontWeight: 700, marginBottom: '20px' }}>
        Saved Jobs {jobs && `(${jobs.totalCount})`}
      </h1>
      <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
        {loading ? (
         <p style={{ color: 'var(--text-secondary)' }}>Loading...</p>
        ) : jobs?.items.length === 0 ? (
          <div style={{
            textAlign: 'center', padding: '60px',
            background: 'var(--surface-card)', borderRadius: '10px', border: '1px solid var(--gray-200)',
          }}>
            <p style={{ color: 'var(--text-secondary)' }}>No saved jobs yet</p>
          </div>
        ) : (
          jobs?.items.map(job => (
            <JobCard
              key={job.id}
              job={job}
              onSaveToggle={(id, saved) => { if (!saved) handleUnsave(id); }}
            />
          ))
        )}
      </div>
    </div>
  );
}