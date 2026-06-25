import './PlanBadge.css';

interface Props {
  tier: string;
  isActive?: boolean;
}

export default function PlanBadge({ tier, isActive = true }: Props) {
  const display = isActive ? tier : 'free';

  const label: Record<string, string> = {
    free: 'Free',
    individual: 'Individual',
    pro: 'Pro',
  };

  return (
    <span className={`plan-badge plan-badge--${display}`}>
      {display === 'pro' && '👑 '}
      {display === 'individual' && '⚡ '}
      {label[display] ?? display}
    </span>
  );
}