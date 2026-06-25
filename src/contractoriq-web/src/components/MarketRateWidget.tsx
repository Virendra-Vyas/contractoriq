import { useState, useEffect } from 'react';
import { getMarketRate, type MarketRate } from '../api/market';

interface Props {
  techStack: string;
  location: string;
  ir35Status: string;
  jobRate?: number;
}

export default function MarketRateWidget({ techStack, location, ir35Status, jobRate }: Props) {
  const [data, setData] = useState<MarketRate | null>(null);

  useEffect(() => {
    if (!techStack && !location) return;
    getMarketRate(techStack, location, ir35Status, jobRate)
      .then(res => setData(res.data))
      .catch(() => {});
  }, [techStack, location, ir35Status, jobRate]);

  if (!data || data.sampleSize < 3) return null;

  const percentileColor = !data.jobPercentile ? 'var(--gray-500)'
    : data.jobPercentile >= 75 ? '#166534'
    : data.jobPercentile >= 50 ? '#854d0e'
    : '#991b1b';

  const barPercent = jobRate && data.max > 0
    ? Math.min(100, (jobRate / data.max) * 100)
    : null;

  return (
    <div style={{
      background: 'var(--gray-50)',
      border: '1px solid var(--gray-200)',
      borderRadius: '8px',
      padding: '12px 14px',
      marginTop: '10px',
    }}>
      <div style={{
        display: 'flex', justifyContent: 'space-between',
        alignItems: 'center', marginBottom: '8px',
      }}>
        <span style={{ fontSize: '0.75rem', fontWeight: 600, color: 'var(--gray-600)' }}>
          📊 Market Rate ({data.sampleSize} roles)
        </span>
        {data.percentileLabel && (
          <span style={{
            fontSize: '0.72rem', fontWeight: 700,
            color: percentileColor,
          }}>
            {data.percentileLabel}
          </span>
        )}
      </div>

      {/* Rate range bar */}
      <div style={{ position: 'relative', marginBottom: '6px' }}>
        <div style={{
          height: '6px', background: 'var(--gray-200)',
          borderRadius: '3px', position: 'relative',
        }}>
          {/* P25-P75 range */}
          <div style={{
            position: 'absolute',
            left: `${(data.p25 / data.max) * 100}%`,
            width: `${((data.p75 - data.p25) / data.max) * 100}%`,
            height: '100%',
            background: 'var(--primary)',
            opacity: 0.4,
            borderRadius: '3px',
          }} />
          {/* Job rate marker */}
          {barPercent !== null && (
            <div style={{
              position: 'absolute',
              left: `${barPercent}%`,
              top: '-3px',
              width: '12px',
              height: '12px',
              background: percentileColor,
              borderRadius: '50%',
              transform: 'translateX(-50%)',
              border: '2px solid white',
              boxShadow: '0 1px 3px rgba(0,0,0,0.2)',
            }} />
          )}
        </div>
      </div>

      {/* Stats row */}
      <div style={{
        display: 'flex', justifyContent: 'space-between',
        fontSize: '0.72rem', color: 'var(--gray-500)',
      }}>
        <span>£{data.p25}/d P25</span>
        <span style={{ fontWeight: 600, color: 'var(--gray-700)' }}>
          £{data.median}/d median
        </span>
        <span>£{data.p75}/d P75</span>
      </div>
    </div>
  );
}