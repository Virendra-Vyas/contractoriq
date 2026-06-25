import { useState, useEffect, useCallback } from 'react';
import { getJobs, scoreAllJobs } from '../api/jobs';
import type { Job, JobFilters, PagedResult } from '../types';
import JobCard from '../components/JobCard';

export default function JobsPage() {
  const [jobs, setJobs] = useState<PagedResult<Job> | null>(null);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState<JobFilters>({ page: 1, pageSize: 20, sortBy: 'posted_at' });
  const [keywords, setKeywords] = useState('');
  const [location, setLocation] = useState('');
  const [scoring, setScoring] = useState(false);

  const fetchJobs = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getJobs(filters);
      setJobs(res.data);
    } catch {}
    setLoading(false);
  }, [filters]);

  useEffect(() => { fetchJobs(); }, [fetchJobs]);

  const applySearch = () => {
    setFilters(f => ({ ...f, keywords, location, page: 1 }));
  };

  const setFilter = (key: keyof JobFilters, value: any) => {
    setFilters(f => ({ ...f, [key]: value || undefined, page: 1 }));
  };

  const handleScoreAll = async () => {
    setScoring(true);
    await scoreAllJobs();
    await fetchJobs();
    setScoring(false);
  };

  return (
    <div style={{ maxWidth: '1200px', margin: '0 auto', padding: '24px 16px' }}>
      {/* Search Bar */}
      <div style={{
        background: 'var(--surface-card)', borderRadius: '10px', padding: '20px',
        border: '1px solid var(--gray-200)', marginBottom: '20px',
        display: 'flex', gap: '12px', flexWrap: 'wrap',
      }}>
        <input
          placeholder="Keywords (e.g. .NET, React, Azure)"
          value={keywords}
          onChange={e => setKeywords(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && applySearch()}
          style={{
            flex: 2, minWidth: '200px', padding: '10px 12px',
            border: '1px solid var(--gray-300)', borderRadius: '8px', fontSize: '0.9rem',
          }}
        />
        <input
          placeholder="Location (e.g. London)"
          value={location}
          onChange={e => setLocation(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && applySearch()}
          style={{
            flex: 1, minWidth: '150px', padding: '10px 12px',
            border: '1px solid var(--gray-300)', borderRadius: '8px', fontSize: '0.9rem',
          }}
        />
        <button onClick={applySearch} style={{
          background: 'var(--primary)', color: 'white', border: 'none',
          borderRadius: '8px', padding: '10px 24px', fontWeight: 600, fontSize: '0.9rem',
        }}>
          Search
        </button>
        <button onClick={handleScoreAll} disabled={scoring} style={{
          background: 'var(--surface-card)', border: '1px solid var(--primary)',
          color: 'var(--primary)', borderRadius: '8px',
          padding: '10px 16px', fontWeight: 600, fontSize: '0.9rem',
          opacity: scoring ? 0.7 : 1, cursor: scoring ? 'not-allowed' : 'pointer',
        }}>
          {scoring ? '⏳ Scoring...' : '🤖 Score Jobs'}
        </button>
      </div>

      <div style={{ display: 'flex', gap: '20px' }}>
        {/* Sidebar Filters */}
        <div style={{
          width: '220px', flexShrink: 0,
          background: 'var(--surface-card)', border: '1px solid var(--gray-200)',
          borderRadius: '10px', padding: '20px', height: 'fit-content',
        }}>
          <h3 style={{ fontSize: '0.9rem', fontWeight: 600, marginBottom: '16px' }}>Filters</h3>

          <div style={{ marginBottom: '16px' }}>
            <label style={{ fontSize: '0.8rem', fontWeight: 500, display: 'block', marginBottom: '6px' }}>IR35</label>
            <select onChange={e => setFilter('ir35Status', e.target.value)}
              style={{ width: '100%', padding: '8px', border: '1px solid var(--gray-300)', borderRadius: '6px', fontSize: '0.85rem' }}>
              <option value="">All</option>
              <option value="outside">Outside IR35</option>
              <option value="inside">Inside IR35</option>
              <option value="unknown">Unknown</option>
            </select>
          </div>

          <div style={{ marginBottom: '16px' }}>
            <label style={{ fontSize: '0.8rem', fontWeight: 500, display: 'block', marginBottom: '6px' }}>Source</label>
            <select onChange={e => setFilter('source', e.target.value)}
              style={{ width: '100%', padding: '8px', border: '1px solid var(--gray-300)', borderRadius: '6px', fontSize: '0.85rem' }}>
              <option value="">All</option>
              <option value="reed">Reed</option>
              <option value="adzuna">Adzuna</option>
            </select>
          </div>

          <div style={{ marginBottom: '16px' }}>
            <label style={{ fontSize: '0.8rem', fontWeight: 500, display: 'block', marginBottom: '6px' }}>Min Day Rate</label>
            <input type="number" placeholder="e.g. 400"
              onChange={e => setFilter('dayRateMin', Number(e.target.value))}
              style={{ width: '100%', padding: '8px', border: '1px solid var(--gray-300)', borderRadius: '6px', fontSize: '0.85rem' }}
            />
          </div>

          <div style={{ marginBottom: '16px' }}>
            <label style={{ fontSize: '0.8rem', fontWeight: 500, display: 'block', marginBottom: '6px' }}>Sort By</label>
            <select onChange={e => setFilter('sortBy', e.target.value)}
              style={{ width: '100%', padding: '8px', border: '1px solid var(--gray-300)', borderRadius: '6px', fontSize: '0.85rem' }}>
              <option value="posted_at">Newest First</option>
              <option value="day_rate">Highest Rate</option>
              <option value="match_score">Best Match</option>
            </select>
          </div>

          <label style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '0.85rem', cursor: 'pointer' }}>
            <input type="checkbox" onChange={e => setFilter('remoteOnly', e.target.checked)} />
            Remote Only
          </label>
        </div>

        {/* Job List */}
        <div style={{ flex: 1 }}>
          {/* Stats bar */}
          <div style={{
            display: 'flex', justifyContent: 'space-between', alignItems: 'center',
            marginBottom: '16px',
          }}>
            <p style={{ fontSize: '0.85rem', color: 'var(--gray-500)' }}>
              {loading ? 'Loading...' : `${jobs?.totalCount ?? 0} jobs found`}
            </p>
            {jobs && jobs.totalPages > 1 && (
              <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
                <button
                  disabled={!jobs.hasPreviousPage}
                  onClick={() => setFilters(f => ({ ...f, page: (f.page ?? 1) - 1 }))}
                  style={{
                    padding: '6px 12px', border: '1px solid var(--gray-300)',
                    borderRadius: '6px', background: 'var(--surface-card)', fontSize: '0.8rem',
                    opacity: !jobs.hasPreviousPage ? 0.4 : 1,
                  }}>
                  ← Prev
                </button>
                <span style={{ fontSize: '0.8rem', color: 'var(--gray-500)' }}>
                  {jobs.page} / {jobs.totalPages}
                </span>
                <button
                  disabled={!jobs.hasNextPage}
                  onClick={() => setFilters(f => ({ ...f, page: (f.page ?? 1) + 1 }))}
                  style={{
                    padding: '6px 12px', border: '1px solid var(--gray-300)',
                    borderRadius: '6px', background: 'var(--surface-card)', fontSize: '0.8rem',
                    opacity: !jobs.hasNextPage ? 0.4 : 1,
                  }}>
                  Next →
                </button>
              </div>
            )}
          </div>

          {/* Cards */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
            {loading ? (
              Array.from({ length: 5 }).map((_, i) => (
                <div key={i} style={{
                  height: '160px', background: 'var(--surface-card)',
                  border: '1px solid var(--gray-200)', borderRadius: '10px',
                }} />
              ))
            ) : jobs?.items.length === 0 ? (
              <div style={{
                textAlign: 'center', padding: '60px',
                background: 'var(--surface-card)', borderRadius: '10px', border: '1px solid var(--gray-200)',
              }}>
                <p style={{ color: 'var(--gray-400)', fontSize: '1rem' }}>No jobs found</p>
                <p style={{ color: 'var(--gray-400)', fontSize: '0.85rem', marginTop: '8px' }}>
                  Try different keywords or remove some filters
                </p>
              </div>
            ) : (
              jobs?.items.map(job => (
                <JobCard key={job.id} job={job} />
              ))
            )}
          </div>
        </div>
      </div>
    </div>
  );
}