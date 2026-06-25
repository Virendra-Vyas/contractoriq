import { useState, useEffect, useCallback } from 'react';
import {
  getSubscriptionStatus,
  createCheckoutSession,
  createPortalSession,
} from '../api/billing';
import type { SubscriptionStatus } from '../api/billing';
import { useAuth } from './useAuth';

export function useBilling() {
  const { isAuthenticated, initialising } = useAuth();
  const [status, setStatus] = useState<SubscriptionStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    if (!isAuthenticated) {
      setLoading(false);
      return;
    }
    try {
      setLoading(true);
      const data = await getSubscriptionStatus();
      setStatus(data);
    } catch {
      setError('Failed to load subscription status');
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    if (initialising) return;
    refresh();
  }, [refresh, initialising]);

  const checkout = useCallback(async (tier: string) => {
    const url = await createCheckoutSession(tier);
    window.location.href = url;
  }, []);

  const openPortal = useCallback(async () => {
    const url = await createPortalSession();
    window.location.href = url;
  }, []);

  return { status, loading, error, checkout, openPortal, refresh };
}