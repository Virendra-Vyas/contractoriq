import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../api/client';
import { getApplications, type ApplicationRecord } from '../api/applications';
import type { Job, PagedResult } from '../types';
import './DashboardPage.css';

// ---------------------------------------------------------------------------
// Pipeline config — keys MUST match the status values stored in the DB
// (ApplicationsPage uses lowercase snake_case, so we do the same here)
// ---------------------------------------------------------------------------
const PIPELINE = [
  { key: 'saved',            label: 'Saved',     color: '#6366f1' },
  { key: 'applied',          label: 'Applied',   color: '#10b981' },
  { key: 'recruiter_screen', label: 'Screening', color: '#f59e0b' },
  { key: 'client_interview', label: 'Interview', color: '#f97316' },
  { key: 'offer',            label: 'Offer',     color: '#06b6d4' },
  { key: 'placed',           label: 'Placed',    color: '#22c55e' },
  { key: 'rejected',         label: 'Rejected',  color: '#ef4444' },
];

const STATUS_LABEL: Record<string, string> = {
  saved:            'Saved',
  applied:          'Applied',
  recruiter_screen: 'Screening',
  client_interview: 'Interview',
  offer:            'Offer',
  placed:           'Placed',
  rejected:         'Rejected',
};

// ---------------------------------------------------------------------------
// StatCard
// ---------------------------------------------------------------------------
interface StatCardProps {
  icon: string;
  value: number;
  label: string;
  accentClass?: string;
}

function StatCard({ icon, value, label, accentClass }: StatCardProps) {
  return (
    <div className={`db-stat-card${accentClass ? ` ${accentClass}` : ''}`}>
      <i className={`ti ${icon} db-stat-icon${accentClass === 'accent-indigo' ? ' db-indigo' : accentClass === 'accent-amber' ? ' db-amber' : ''}`} aria-hidden="true" />
      <p className={`db-stat-value${accentClass === 'accent-indigo' ? ' db-indigo' : accentClass === 'accent-amber' ? ' db-amber' : ''}`}>
        {value}
      </p>
      <p className="db-stat-label">{label}</p>
    </div>
  );
}

// ---------------------------------------------------------------------------
// MatchGauge
// ---------------------------------------------------------------------------
interface MatchGaugeProps {
  score: number | null;
  jobTitle: string | null;
  jobsScored: number;
  highMatches: number;
  savedCount: number;
  onScoreJobs: () => void;
}

function MatchGauge({ score, jobTitle, jobsScored, highMatches, savedCount, onScoreJobs }: MatchGaugeProps) {
  const ARC_LEN = Math.PI * 60;
  const fill = score !== null ? (score / 100) * ARC_LEN : 0;
  const scoreColor =
    score === null  ? 'var(--db-text-tertiary)'
    : score >= 80   ? 'var(--db-amber)'
    : score >= 60   ? 'var(--db-indigo)'
    :                 'var(--db-text-secondary)';

  return (
    <div className="db-panel db-gauge-panel">
      <div className="db-panel-header">
        <span className="db-panel-label">Best match</span>
        <button className="db-panel-link" onClick={onScoreJobs}>Score jobs →</button>
      </div>
      <div className="db-gauge-display">
        <svg viewBox="0 0 160 90" width="150" height="84"
          aria-label={score !== null ? `Best match: ${score.toFixed(1)}%` : 'No jobs scored yet'}>
          <path d="M 20 78 A 60 60 0 0 1 140 78" fill="none" strokeWidth="12"
            strokeLinecap="round" className="db-gauge-track" />
          {score !== null && (
            <path d="M 20 78 A 60 60 0 0 1 140 78" fill="none" strokeWidth="12"
              strokeLinecap="round"
              strokeDasharray={`${fill} ${ARC_LEN + 20}`}
              style={{ stroke: scoreColor, transition: 'stroke-dasharray 0.8s ease' }} />
          )}
        </svg>
        <p className="db-gauge-number" style={{ color: scoreColor }}>
          {score !== null ? score.toFixed(1) : '—'}
        </p>
        <p className="db-gauge-sub">
          {score !== null ? `% match${jobTitle ? ` · ${jobTitle}` : ''}` : 'No jobs scored yet'}
        </p>
      </div>
      <div className="db-gauge-stats">
        <div className="db-gauge-row"><span>Jobs scored</span><span>{jobsScored}</span></div>
        <div className="db-gauge-row"><span>High matches 70%+</span><span className="db-amber">{highMatches}</span></div>
        <div className="db-gauge-row"><span>Saved jobs</span><span>{savedCount}</span></div>
      </div>
    </div>
  );
}

// ---------------------------------------------------------------------------
// PipelinePanel
// ---------------------------------------------------------------------------
interface PipelinePanelProps {
  counts: Record<string, number>;
  total: number;
  recentApps: ApplicationRecord[];
  onViewTracker: () => void;
}

function PipelinePanel({ counts, total, recentApps, onViewTracker }: PipelinePanelProps) {
  return (
    <div className="db-panel db-pipeline-panel">
      <div className="db-panel-header">
        <span className="db-panel-label">Application pipeline</span>
        <button className="db-panel-link" onClick={onViewTracker}>View tracker →</button>
      </div>

      {/* Progress bar */}
      <div className="db-pipeline-bar" role="img" aria-label="Pipeline breakdown">
        {total === 0 ? (
          <div className="db-pipeline-empty" />
        ) : (
          PIPELINE.map(({ key, color }) => {
            const n = counts[key] ?? 0;
            if (n === 0) return null;
            return (
              <div key={key} className="db-pipeline-segment"
                style={{ width: `${(n / total) * 100}%`, background: color }}
                title={`${STATUS_LABEL[key]}: ${n}`} />
            );
          })
        )}
      </div>

      {/* Legend */}
      <div className="db-pipeline-legend">
        {PIPELINE.map(({ key, label, color }) => (
          <span key={key} className="db-legend-item">
            <span className="db-legend-dot" style={{ background: color }} />
            {label} {counts[key] ?? 0}
          </span>
        ))}
      </div>

      {/* Recent applications */}
      <div className="db-recent">
        <p className="db-panel-label" style={{ marginBottom: 12 }}>Recent applications</p>
        {recentApps.length === 0 ? (
          <p className="db-empty-state">No applications yet — save a job to get started</p>
        ) : (
          recentApps.map(app => (
            <div key={app.id} className="db-recent-row">
              <div>
                {/* ApplicationRecord uses flat fields: jobTitle, company */}
                <p className="db-recent-title">{app.jobTitle}</p>
                <p className="db-recent-company">{app.company}</p>
              </div>
              <div className="db-recent-meta">
                {app.matchScore != null && (
                  <span className="db-match-score">{Math.round(app.matchScore)}%</span>
                )}
                <span className={`db-status-badge db-status-${app.status}`}>
                  {STATUS_LABEL[app.status] ?? app.status}
                </span>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}

// ---------------------------------------------------------------------------
// DashboardPage
// ---------------------------------------------------------------------------
export default function DashboardPage() {
  const navigate = useNavigate();
  const [applications, setApplications] = useState<ApplicationRecord[]>([]);
  const [jobs, setJobs]                 = useState<Job[]>([]);
  const [loading, setLoading]           = useState(true);

  const fetchData = useCallback(async () => {
    try {
      // Fetch independently so one failure doesn't zero the other
      const appsRes = await getApplications();
      setApplications(appsRes.data ?? []);
    } catch (err) {
      console.error('Dashboard: applications fetch failed:', err);
    }

    try {
      // Jobs API returns PagedResult<Job> — use .items not .jobs
      const jobsRes = await apiClient.get<PagedResult<Job>>('/jobs', {
        params: { pageSize: 200 },
      });
      setJobs(jobsRes.data?.items ?? []);
    } catch (err) {
      console.error('Dashboard: jobs fetch failed:', err);
    }

    setLoading(false);
  }, []);

  useEffect(() => { fetchData(); }, [fetchData]);

  // ---- Derived stats ----
  // Status keys must match the DB values (lowercase snake_case)
  const pipelineCounts = PIPELINE.reduce<Record<string, number>>((acc, { key }) => {
    acc[key] = applications.filter(a => a.status === key).length;
    return acc;
  }, {});

  const totalApps      = applications.length;
  const activePipeline = applications.filter(a =>
    ['applied', 'recruiter_screen', 'client_interview', 'offer'].includes(a.status)
  ).length;
  const interviews = applications.filter(a => a.status === 'client_interview').length;
  const offers     = applications.filter(a => a.status === 'offer').length;
  const savedCount = applications.filter(a => a.status === 'saved').length;

  const scoredJobs  = jobs.filter(j => j.matchScore !== null);
  const highMatches = scoredJobs.filter(j => (j.matchScore ?? 0) >= 70).length;
  const bestJob     = [...scoredJobs].sort((a, b) => (b.matchScore ?? 0) - (a.matchScore ?? 0))[0] ?? null;
  const ir35Screens = jobs.filter(j => j.ir35Status === 'outside' || j.ir35Status === 'inside').length;

  const recentApps = [...applications]
    .sort((a, b) => {
      // sort by id as fallback if createdAt isn't available
      const aDate = (a as any).createdAt ? new Date((a as any).createdAt).getTime() : 0;
      const bDate = (b as any).createdAt ? new Date((b as any).createdAt).getTime() : 0;
      return bDate - aDate;
    })
    .slice(0, 3);

  const greeting = () => {
    const h = new Date().getHours();
    return h < 12 ? 'Good morning' : h < 17 ? 'Good afternoon' : 'Good evening';
  };

  const todayStr = new Date().toLocaleDateString('en-GB', {
    weekday: 'long', day: 'numeric', month: 'long',
  });

  if (loading) {
    return (
      <div className="db-loading" role="status">
        <div className="db-spinner" />
        <p className="db-loading-text">Loading your dashboard...</p>
      </div>
    );
  }

  return (
    <div className="db-page">
      <div className="db-inner">

        {/* Header */}
        <div className="db-header">
          <div>
            <p className="db-greeting">{greeting()}</p>
            <h1 className="db-name">Virendra Vyas</h1>
            <p className="db-subtitle">Here's your contracting overview</p>
          </div>
          <div className="db-header-right">
            <p className="db-date">{todayStr}</p>
            <p className="db-location">Eastbourne, UK</p>
          </div>
        </div>

        {/* Stats row 1 — pipeline */}
        <div className="db-stats-grid">
          <StatCard icon="ti-file-text" value={totalApps}      label="Applications" />
          <StatCard icon="ti-rocket"    value={activePipeline} label="Active pipeline" />
          <StatCard icon="ti-users"     value={interviews}     label="Interviews" />
          <StatCard icon="ti-confetti"  value={offers}         label="Offers" />
        </div>

        {/* Stats row 2 — AI */}
        <div className="db-stats-grid" style={{ marginBottom: 28 }}>
          <StatCard icon="ti-robot"        value={scoredJobs.length} label="Jobs scored"       accentClass="accent-indigo" />
          <StatCard icon="ti-star"         value={highMatches}       label="High matches 70%+" accentClass="accent-amber" />
          <StatCard icon="ti-file-cv"      value={0}                 label="CVs tailored" />
          <StatCard icon="ti-shield-check" value={ir35Screens}       label="IR35 screens" />
        </div>

        {/* Panels */}
        <div className="db-panels">
          <PipelinePanel
            counts={pipelineCounts}
            total={totalApps}
            recentApps={recentApps}
            onViewTracker={() => navigate('/applications')}
          />
          <MatchGauge
            score={bestJob ? parseFloat((bestJob.matchScore ?? 0).toFixed(1)) : null}
            jobTitle={bestJob?.title ?? null}
            jobsScored={scoredJobs.length}
            highMatches={highMatches}
            savedCount={savedCount}
            onScoreJobs={() => navigate('/jobs')}
          />
        </div>

        {/* Quick actions */}
        <p className="db-section-label">Quick actions</p>
        <div className="db-quick-grid">
          <button className="db-quick-btn" onClick={() => navigate('/jobs')}>
            <i className="ti ti-robot db-quick-icon db-indigo" aria-hidden="true" />
            Score &amp; match
          </button>
          <button className="db-quick-btn" onClick={() => navigate('/jobs')}>
            <i className="ti ti-file-text db-quick-icon db-amber" aria-hidden="true" />
            Tailor a CV
          </button>
          <button className="db-quick-btn" onClick={() => navigate('/jobs')}>
            <i className="ti ti-shield-check db-quick-icon db-green" aria-hidden="true" />
            IR35 screen
          </button>
          <button className="db-quick-btn" onClick={() => navigate('/applications')}>
            <i className="ti ti-columns db-quick-icon" aria-hidden="true" />
            Update tracker
          </button>
        </div>

      </div>
    </div>
  );
}