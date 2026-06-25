import client from './client';

export interface ApplicationStats {
  total: number;
  saved: number;
  applied: number;
  interviewing: number;
  offers: number;
  placed: number;
  rejected: number;
}

export interface JobStats {
  totalScored: number;
  savedJobs: number;
  averageMatchScore: number;
  highMatchCount: number;
}

export interface UsageStats {
  cvsTailored: number;
  ir35Screens: number;
}

export interface SubscriptionSummary {
  tier: string;
  status: string;
  isActive: boolean;
}

export interface RecentApplication {
  id: string;
  jobTitle: string;
  company: string;
  status: string;
  matchScore: number | null;
  createdAt: string;
}

export interface DashboardData {
  applicationStats: ApplicationStats;
  jobStats: JobStats;
  usageStats: UsageStats;
  subscription: SubscriptionSummary;
  recentApplications: RecentApplication[];
}

export async function getDashboard(): Promise<DashboardData> {
  const res = await client.get('/api/dashboard');
  return res.data;
}