import client from './client';

export interface SubscriptionStatus {
  tier: string;
  status: string;
  currentPeriodEnd: string | null;
  isActive: boolean;
}

export interface Plan {
  tier: string;
  displayName: string;
  monthlyPriceGbp: number;
  features: string[];
}

export async function getSubscriptionStatus(): Promise<SubscriptionStatus> {
  const res = await client.get('/api/billing/status');
  return res.data;
}

export async function getPlans(): Promise<Plan[]> {
  const res = await client.get('/api/billing/plans');
  return res.data;
}

export async function createCheckoutSession(tier: string): Promise<string> {
  const res = await client.post('/api/billing/checkout', { tier });
  return res.data.url;
}

export async function createPortalSession(): Promise<string> {
  const res = await client.post('/api/billing/portal');
  return res.data.url;
}