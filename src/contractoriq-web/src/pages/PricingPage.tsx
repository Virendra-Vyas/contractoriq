import { useEffect, useState } from 'react';
import { getPlans } from '../api/billing';
import type { Plan } from '../api/billing';
import { useBilling } from '../hooks/useBilling';
import PlanBadge from '../components/PlanBadge';
import './PricingPage.css';

const PLAN_ICONS: Record<string, string> = {
  free: '🆓',
  individual: '⚡',
  pro: '👑',
};

export default function PricingPage() {
  const [plans, setPlans] = useState<Plan[]>([]);
  const [plansLoading, setPlansLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const { status, loading, checkout, openPortal } = useBilling();

  useEffect(() => {
    getPlans()
      .then(setPlans)
      .finally(() => setPlansLoading(false));
  }, []);

  const handleSelect = async (tier: string) => {
    if (tier === 'free') return;
    setActionLoading(tier);
    try {
      if (status?.isActive && status.tier === tier) {
        await openPortal();
      } else {
        await checkout(tier);
      }
    } finally {
      setActionLoading(null);
    }
  };

  const getButtonLabel = (tier: string): string => {
    if (tier === 'free') return 'Current Plan';
    if (status?.isActive && status.tier === tier) return 'Manage Subscription';
    if (!status?.isActive || status.tier === 'free') return `Upgrade to ${tier === 'individual' ? 'Individual' : 'Pro'}`;
    return `Switch to ${tier === 'individual' ? 'Individual' : 'Pro'}`;
  };

  const isCurrentPlan = (tier: string): boolean =>
    (!status?.isActive && tier === 'free') ||
    (!!status?.isActive && status.tier === tier);

  if (plansLoading || loading) {
    return (
      <div className="pricing-loading">
        <div className="pricing-spinner" />
        <p>Loading plans…</p>
      </div>
    );
  }

  return (
    <div className="pricing-page">
      <div className="pricing-header">
        <h1>Choose Your Plan</h1>
        <p>Scale your contracting career with the right tools</p>

        {status?.isActive && status.tier !== 'free' && (
          <div className="pricing-current-banner">
            <PlanBadge tier={status.tier} isActive={status.isActive} />
            <span>
              You're on the <strong>{status.tier === 'individual' ? 'Individual' : 'Pro'}</strong> plan
              {status.currentPeriodEnd && (
                <> · renews {new Date(status.currentPeriodEnd).toLocaleDateString('en-GB')}</>
              )}
            </span>
          </div>
        )}
      </div>

      <div className="pricing-grid">
        {plans.map((plan) => (
          <div
            key={plan.tier}
            className={[
              'pricing-card',
              `pricing-card--${plan.tier}`,
              isCurrentPlan(plan.tier) ? 'pricing-card--current' : '',
            ].join(' ').trim()}
          >
            {plan.tier === 'individual' && (
              <div className="pricing-popular">Most Popular</div>
            )}

            <div className="pricing-card-header">
              <span className="pricing-icon">{PLAN_ICONS[plan.tier]}</span>
              <h2>{plan.displayName}</h2>
              <div className="pricing-price">
                {plan.monthlyPriceGbp === 0 ? (
                  <span className="pricing-amount-free">Free</span>
                ) : (
                  <>
                    <span className="pricing-currency">£</span>
                    <span className="pricing-amount">{plan.monthlyPriceGbp}</span>
                    <span className="pricing-period">/mo</span>
                  </>
                )}
              </div>
            </div>

            <ul className="pricing-features">
              {plan.features.map((f) => (
                <li key={f}>
                  <span className="pricing-check">✓</span>
                  {f}
                </li>
              ))}
            </ul>

            <button
              className={[
                'pricing-btn',
                `pricing-btn--${plan.tier}`,
                isCurrentPlan(plan.tier) ? 'pricing-btn--current' : '',
              ].join(' ').trim()}
              onClick={() => handleSelect(plan.tier)}
              disabled={
                plan.tier === 'free' ||
                actionLoading !== null ||
                (isCurrentPlan(plan.tier) && !status?.isActive)
              }
            >
              {actionLoading === plan.tier ? 'Redirecting…' : getButtonLabel(plan.tier)}
            </button>
          </div>
        ))}
      </div>

      {status?.isActive && status.tier !== 'free' && (
        <div className="pricing-portal-row">
          <button className="pricing-portal-btn" onClick={openPortal}>
            Manage Billing &amp; Invoices →
          </button>
        </div>
      )}
    </div>
  );
}