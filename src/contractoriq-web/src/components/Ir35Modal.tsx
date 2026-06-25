import { useState, useEffect } from 'react';
import { getIr35Analysis, type Ir35Analysis } from '../api/ir35';

interface Props {
  jobId: string;
  jobTitle: string;
  onClose: () => void;
}

function ScoreBar({ label, score }: { label: string; score: number }) {
  const color = score <= 35 ? '#166534' : score <= 65 ? '#854d0e' : '#991b1b';
  const barColor = score <= 35 ? '#22c55e' : score <= 65 ? '#f59e0b' : '#ef4444';
  return (
    <div style={{ marginBottom: '12px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
        <span style={{ fontSize: '0.82rem', fontWeight: 500 }}>{label}</span>
        <span style={{ fontSize: '0.82rem', fontWeight: 700, color }}>{score}/100</span>
      </div>
      <div style={{ height: '8px', background: 'var(--gray-100)', borderRadius: '4px', overflow: 'hidden' }}>
        <div style={{
          height: '100%', width: `${score}%`,
          background: barColor,
          borderRadius: '4px', transition: 'width 0.5s',
        }} />
      </div>
    </div>
  );
}

export default function Ir35Modal({ jobId, jobTitle, onClose }: Props) {
  const [analysis, setAnalysis] = useState<Ir35Analysis | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    getIr35Analysis(jobId)
      .then(res => setAnalysis(res.data))
      .catch(() => setError('Analysis failed. Please try again.'))
      .finally(() => setLoading(false));
  }, [jobId]);

  const verdictColor = analysis?.verdict === 'low' ? '#166534'
    : analysis?.verdict === 'medium' ? '#854d0e'
    : '#991b1b';

  const verdictBg = analysis?.verdict === 'low' ? '#dcfce7'
    : analysis?.verdict === 'medium' ? '#fef9c3'
    : '#fee2e2';

  const verdictLabel = analysis?.verdict === 'low' ? '🟢 Low Risk — Outside IR35'
    : analysis?.verdict === 'medium' ? '🟡 Medium Risk — Borderline'
    : '🔴 High Risk — Inside IR35';

  return (
    <div style={{
      position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      zIndex: 1000, padding: '16px',
    }} onClick={onClose}>
      <div style={{
        background: 'var(--surface-card)', borderRadius: '12px', padding: '28px',
        maxWidth: '560px', width: '100%', maxHeight: '90vh', overflowY: 'auto',
      }} onClick={e => e.stopPropagation()}>

        {/* Header */}
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '20px' }}>
          <div>
            <h2 style={{ fontSize: '1.1rem', fontWeight: 700 }}>IR35 Analysis</h2>
            <p style={{ fontSize: '0.82rem', color: 'var(--gray-500)', marginTop: '2px' }}>{jobTitle}</p>
          </div>
          <button onClick={onClose} style={{
            background: 'none', border: 'none', fontSize: '1.4rem',
            cursor: 'pointer', color: 'var(--gray-400)', lineHeight: 1,
          }}>✕</button>
        </div>

        {loading && (
          <div style={{ textAlign: 'center', padding: '40px', color: 'var(--gray-400)' }}>
            <p style={{ fontSize: '1.5rem', marginBottom: '12px' }}>⚖️</p>
            <p>Analysing IR35 status...</p>
          </div>
        )}

        {error && (
          <p style={{ color: 'var(--danger)', textAlign: 'center', padding: '20px' }}>{error}</p>
        )}

        {analysis && (
          <>
            {/* Verdict */}
            <div style={{
              background: verdictBg, borderRadius: '10px', padding: '16px',
              textAlign: 'center', marginBottom: '24px',
            }}>
              <div style={{ fontSize: '2rem', marginBottom: '6px' }}>
                {analysis.verdict === 'low' ? '🟢' : analysis.verdict === 'medium' ? '🟡' : '🔴'}
              </div>
              <div style={{ fontWeight: 700, fontSize: '1.1rem', color: verdictColor }}>
                {verdictLabel}
              </div>
              <div style={{ fontSize: '0.82rem', color: verdictColor, marginTop: '4px', opacity: 0.8 }}>
                Risk Score: {analysis.riskScore}/100
              </div>
            </div>

            {/* Summary */}
            <p style={{
              fontSize: '0.875rem', lineHeight: 1.6,
              color: 'var(--gray-700)', marginBottom: '20px',
            }}>
              {analysis.summary}
            </p>

            {/* HMRC Tests */}
            <h3 style={{
              fontSize: '0.85rem', fontWeight: 700, marginBottom: '12px',
              color: 'var(--gray-600)', textTransform: 'uppercase', letterSpacing: '0.05em',
            }}>
              HMRC Test Scores
            </h3>
            <ScoreBar label="Right of Substitution" score={analysis.substitutionScore} />
            <ScoreBar label="Control" score={analysis.controlScore} />
            <ScoreBar label="Mutuality of Obligation" score={analysis.mooScore} />

            <div style={{ marginBottom: '20px' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ fontSize: '0.82rem', fontWeight: 500 }}>
                  SDC Risk (Supervision, Direction, Control)
                </span>
                <span style={{
                  fontSize: '0.75rem', fontWeight: 700, padding: '2px 8px',
                  borderRadius: '12px',
                  background: analysis.sdcRisk === 'low' ? '#dcfce7' : analysis.sdcRisk === 'medium' ? '#fef9c3' : '#fee2e2',
                  color: analysis.sdcRisk === 'low' ? '#166534' : analysis.sdcRisk === 'medium' ? '#854d0e' : '#991b1b',
                }}>
                  {analysis.sdcRisk.toUpperCase()}
                </span>
              </div>
            </div>

            {/* Red Flags */}
            {analysis.redFlags.length > 0 && (
              <div style={{ marginBottom: '16px' }}>
                <h3 style={{ fontSize: '0.85rem', fontWeight: 700, marginBottom: '8px', color: '#991b1b' }}>
                  🚩 Red Flags
                </h3>
                {analysis.redFlags.map((flag, i) => (
                  <div key={i} style={{
                    fontSize: '0.82rem', padding: '6px 10px', marginBottom: '4px',
                    background: '#fee2e2', borderRadius: '6px', color: '#7f1d1d',
                  }}>
                    {flag}
                  </div>
                ))}
              </div>
            )}

            {/* Green Flags */}
            {analysis.greenFlags.length > 0 && (
              <div style={{ marginBottom: '16px' }}>
                <h3 style={{ fontSize: '0.85rem', fontWeight: 700, marginBottom: '8px', color: '#166534' }}>
                  ✅ Green Flags
                </h3>
                {analysis.greenFlags.map((flag, i) => (
                  <div key={i} style={{
                    fontSize: '0.82rem', padding: '6px 10px', marginBottom: '4px',
                    background: '#dcfce7', borderRadius: '6px', color: '#14532d',
                  }}>
                    {flag}
                  </div>
                ))}
              </div>
            )}

            {analysis.fromCache && (
              <p style={{ fontSize: '0.72rem', color: 'var(--gray-400)', textAlign: 'right', marginTop: '8px' }}>
                Cached · {new Date(analysis.analysedAt).toLocaleDateString()}
              </p>
            )}
          </>
        )}
      </div>
    </div>
  );
}