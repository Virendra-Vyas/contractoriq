import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './BillingSuccessPage.css';

export default function BillingSuccessPage() {
  const navigate = useNavigate();

  useEffect(() => {
    const timer = setTimeout(() => navigate('/jobs'), 4000);
    return () => clearTimeout(timer);
  }, [navigate]);

  return (
    <div className="billing-success-page">
      <div className="billing-success-card">
        <div className="billing-success-icon">🎉</div>
        <h1>Subscription Activated!</h1>
        <p>Your plan is now active. Taking you to Jobs…</p>
        <div className="billing-success-bar">
          <div className="billing-success-bar-fill" />
        </div>
        <button onClick={() => navigate('/jobs')}>Go to Jobs Now →</button>
      </div>
    </div>
  );
}