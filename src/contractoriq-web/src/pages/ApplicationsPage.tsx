import { useState, useEffect } from 'react';
import {
  getApplications, updateApplication, deleteApplication,
  type ApplicationRecord
} from '../api/applications';

// Dark-theme column definitions — bg/color are readable on dark surfaces
const COLUMNS: { key: string; label: string; color: string; bg: string }[] = [
  { key: 'saved',            label: '🔖 Saved',           color: '#818cf8', bg: 'rgba(99,102,241,0.12)'  },
  { key: 'applied',          label: '📤 Applied',          color: '#22d3ee', bg: 'rgba(8,145,178,0.12)'   },
  { key: 'recruiter_screen', label: '📞 Recruiter Screen', color: '#fbbf24', bg: 'rgba(217,119,6,0.12)'   },
  { key: 'client_interview', label: '🎯 Client Interview', color: '#a78bfa', bg: 'rgba(124,58,237,0.12)'  },
  { key: 'offer',            label: '🎉 Offer',            color: '#34d399', bg: 'rgba(5,150,105,0.12)'   },
  { key: 'placed',           label: '✅ Placed',           color: '#4ade80', bg: 'rgba(34,197,94,0.12)'   },
  { key: 'rejected',         label: '❌ Rejected',         color: '#f87171', bg: 'rgba(220,38,38,0.12)'   },
];

export default function ApplicationsPage() {
  const [applications, setApplications] = useState<ApplicationRecord[]>([]);
  const [loading, setLoading]           = useState(true);
  const [dragging, setDragging]         = useState<string | null>(null);
  const [editingId, setEditingId]       = useState<string | null>(null);
  const [noteText, setNoteText]         = useState('');

  useEffect(() => {
    getApplications().then(res => {
      setApplications(res.data);
      setLoading(false);
    });
  }, []);

  const byStatus = (status: string) =>
    applications.filter(a => a.status === status);

  const handleDragStart = (id: string) => setDragging(id);

  const handleDrop = async (status: string) => {
    if (!dragging) return;
    const app = applications.find(a => a.id === dragging);
    if (!app || app.status === status) return;
    setApplications(prev =>
      prev.map(a => a.id === dragging ? { ...a, status } : a)
    );
    await updateApplication(dragging, { status });
    setDragging(null);
  };

  const handleSaveNote = async (id: string) => {
    await updateApplication(id, { notes: noteText });
    setApplications(prev =>
      prev.map(a => a.id === id ? { ...a, notes: noteText } : a)
    );
    setEditingId(null);
  };

  const handleDelete = async (id: string) => {
    await deleteApplication(id);
    setApplications(prev => prev.filter(a => a.id !== id));
  };

  if (loading) return (
    <div style={{ padding: '40px', color: 'var(--text-secondary)' }}>
      Loading applications...
    </div>
  );

  if (applications.length === 0) return (
    <div style={{ maxWidth: '600px', margin: '60px auto', textAlign: 'center', padding: '0 16px' }}>
      <p style={{ fontSize: '2rem', marginBottom: '16px' }}>📋</p>
      <h2 style={{ fontSize: '1.2rem', fontWeight: 700, marginBottom: '8px', color: 'var(--gray-900)' }}>
        No applications yet
      </h2>
      <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
        Save or apply to jobs from the job feed to track them here.
      </p>
    </div>
  );

  return (
    <div style={{ padding: '24px 16px', overflowX: 'auto', minHeight: 'calc(100vh - 100px)' }}>
      <div style={{ marginBottom: '20px' }}>
        <h1 style={{ fontSize: '1.3rem', fontWeight: 700, color: 'var(--gray-900)' }}>
          Application Tracker
        </h1>
        <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginTop: '4px' }}>
          {applications.length} application{applications.length !== 1 ? 's' : ''} — drag cards between columns to update status
        </p>
      </div>

      {/* Kanban Board */}
      <div style={{ display: 'flex', gap: '12px', minWidth: '900px' }}>
        {COLUMNS.map(col => (
          <div
            key={col.key}
            onDragOver={e => e.preventDefault()}
            onDrop={() => handleDrop(col.key)}
            style={{
              flex: 1,
              minWidth: '180px',
              background: 'var(--surface-card)',
              borderRadius: '10px',
              border: '1px solid var(--border-default)',
              transition: 'border-color 0.15s',
            }}
            onDragEnter={e => (e.currentTarget.style.borderColor = col.color)}
            onDragLeave={e => (e.currentTarget.style.borderColor = 'var(--border-default)')}
          >
            {/* Column header */}
            <div style={{
              padding: '10px 12px',
              background: col.bg,
              borderRadius: '9px 9px 0 0',
              borderBottom: `1px solid ${col.color}30`,
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
            }}>
              <span style={{ fontWeight: 700, fontSize: '0.8rem', color: col.color }}>
                {col.label}
              </span>
              <span style={{
                background: col.color,
                color: '#060912',
                borderRadius: '12px',
                padding: '1px 8px',
                fontSize: '0.72rem',
                fontWeight: 700,
              }}>
                {byStatus(col.key).length}
              </span>
            </div>

            {/* Cards */}
            <div style={{ padding: '8px', minHeight: '100px' }}>
              {byStatus(col.key).map(app => (
                <div
                  key={app.id}
                  draggable
                  onDragStart={() => handleDragStart(app.id)}
                  onDragEnd={() => setDragging(null)}
                  style={{
                    background: 'var(--surface-elevated)',
                    border: '1px solid var(--border-default)',
                    borderRadius: '8px',
                    padding: '12px',
                    marginBottom: '8px',
                    cursor: 'grab',
                    opacity: dragging === app.id ? 0.5 : 1,
                    transition: 'opacity 0.15s',
                  }}
                >
                  {/* Title */}
                  <p style={{
                    fontWeight: 600,
                    fontSize: '0.82rem',
                    marginBottom: '3px',
                    color: 'var(--gray-900)',
                    lineHeight: 1.3,
                  }}>
                    {app.jobTitle}
                  </p>
                  <p style={{ fontSize: '0.72rem', color: 'var(--text-secondary)', marginBottom: '8px' }}>
                    {app.company}
                  </p>

                  {/* Badges */}
                  <div style={{ display: 'flex', gap: '4px', flexWrap: 'wrap', marginBottom: '8px' }}>
                    {app.dayRateMax && (
                      <span style={{
                        background: 'var(--surface-card)',
                        color: 'var(--gray-800)',
                        border: '1px solid var(--border-default)',
                        borderRadius: '10px',
                        padding: '1px 6px',
                        fontSize: '0.68rem',
                        fontWeight: 600,
                      }}>
                        £{app.dayRateMax}/d
                      </span>
                    )}

                    {app.matchScore && (
                      <span style={{
                        background: app.matchScore >= 70
                          ? 'rgba(16,185,129,0.12)'
                          : 'rgba(245,158,11,0.12)',
                        color: app.matchScore >= 70 ? '#34d399' : '#fbbf24',
                        borderRadius: '10px',
                        padding: '1px 6px',
                        fontSize: '0.68rem',
                        fontWeight: 700,
                      }}>
                        {app.matchScore}%
                      </span>
                    )}

                    <span style={{
                      background: app.ir35Status === 'outside'
                        ? 'rgba(16,185,129,0.12)'
                        : app.ir35Status === 'inside'
                          ? 'rgba(239,68,68,0.12)'
                          : 'rgba(71,85,105,0.15)',
                      color: app.ir35Status === 'outside'
                        ? '#34d399'
                        : app.ir35Status === 'inside'
                          ? '#f87171'
                          : 'var(--text-secondary)',
                      borderRadius: '10px',
                      padding: '1px 6px',
                      fontSize: '0.68rem',
                      fontWeight: 600,
                    }}>
                      {app.ir35Status === 'outside' ? 'Outside'
                        : app.ir35Status === 'inside' ? 'Inside'
                        : 'IR35?'}
                    </span>
                  </div>

                  {/* Notes */}
                  {editingId === app.id ? (
                    <div style={{ marginBottom: '8px' }}>
                      <textarea
                        value={noteText}
                        onChange={e => setNoteText(e.target.value)}
                        placeholder="Add notes..."
                        rows={3}
                        style={{
                          width: '100%',
                          fontSize: '0.75rem',
                          border: '1px solid var(--border-emphasis)',
                          borderRadius: '4px',
                          padding: '6px',
                          resize: 'none',
                          boxSizing: 'border-box',
                          background: 'var(--surface-page)',
                          color: 'var(--text-primary)',
                        }}
                      />
                      <div style={{ display: 'flex', gap: '4px', marginTop: '4px' }}>
                        <button onClick={() => handleSaveNote(app.id)} style={{
                          background: 'var(--primary)',
                          color: 'white',
                          border: 'none',
                          borderRadius: '4px',
                          padding: '3px 8px',
                          fontSize: '0.72rem',
                          cursor: 'pointer',
                        }}>
                          Save
                        </button>
                        <button onClick={() => setEditingId(null)} style={{
                          background: 'transparent',
                          border: '1px solid var(--border-emphasis)',
                          borderRadius: '4px',
                          padding: '3px 8px',
                          fontSize: '0.72rem',
                          cursor: 'pointer',
                          color: 'var(--text-secondary)',
                        }}>
                          Cancel
                        </button>
                      </div>
                    </div>
                  ) : app.notes ? (
                    <p
                      onClick={() => { setEditingId(app.id); setNoteText(app.notes ?? ''); }}
                      style={{
                        fontSize: '0.72rem',
                        color: 'var(--text-secondary)',
                        background: 'var(--surface-card)',
                        border: '1px solid var(--border-default)',
                        borderRadius: '4px',
                        padding: '5px 7px',
                        marginBottom: '8px',
                        cursor: 'pointer',
                        lineHeight: 1.4,
                      }}
                    >
                      📝 {app.notes}
                    </p>
                  ) : null}

                  {/* Follow-up date */}
                  {app.followUpAt && (
                    <p style={{
                      fontSize: '0.72rem',
                      color: '#fbbf24',
                      marginBottom: '6px',
                    }}>
                      🗓 Follow up: {new Date(app.followUpAt).toLocaleDateString('en-GB')}
                    </p>
                  )}

                  {/* Actions */}
                  <div style={{ display: 'flex', gap: '5px', marginTop: '6px' }}>
                    <button
                      onClick={() => { setEditingId(app.id); setNoteText(app.notes ?? ''); }}
                      style={{
                        background: 'transparent',
                        border: '1px solid var(--border-default)',
                        borderRadius: '4px',
                        padding: '2px 7px',
                        fontSize: '0.68rem',
                        cursor: 'pointer',
                        color: 'var(--text-secondary)',
                      }}
                    >
                      ✏️ Note
                    </button>
                    <a
                      href={app.sourceUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      style={{
                        background: 'transparent',
                        border: '1px solid var(--border-default)',
                        borderRadius: '4px',
                        padding: '2px 7px',
                        fontSize: '0.68rem',
                        color: 'var(--info)',
                        textDecoration: 'none',
                      }}
                    >
                      Apply →
                    </a>
                    <button
                      onClick={() => handleDelete(app.id)}
                      style={{
                        background: 'transparent',
                        border: '1px solid var(--border-default)',
                        borderRadius: '4px',
                        padding: '2px 7px',
                        fontSize: '0.68rem',
                        cursor: 'pointer',
                        color: 'var(--danger)',
                        marginLeft: 'auto',
                      }}
                    >
                      ✕
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}