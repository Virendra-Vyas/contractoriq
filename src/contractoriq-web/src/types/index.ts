export interface Job {
  id: string;
  externalId: string;
  source: string;
  title: string;
  company: string;
  location: string;
  isRemote: boolean;
  isHybrid: boolean;
  dayRateMin: number | null;
  dayRateMax: number | null;
  ir35Status: string;
  contractLength: string | null;
  description: string;
  techStack: string;
  sourceUrl: string;
  postedAt: string;
  scrapedAt: string;
  isSaved: boolean;
  matchScore: number | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface JobFilters {
  keywords?: string;
  location?: string;
  ir35Status?: string;
  source?: string;
  dayRateMin?: number;
  dayRateMax?: number;
  remoteOnly?: boolean;
  techStack?: string;
  sortBy?: string;
  page?: number;
  pageSize?: number;
}

export interface AuthResponse {
  token: string;
  email: string;
  firstName: string;
  lastName: string;
  userId: string;
  tier: string;
  expiresAt: string;
}

export interface Profile {
  id: string;
  userId: string;
  jobTitle: string | null;
  summary: string | null;
  skills: string | null;
  preferredLocation: string | null;
  remoteOnly: boolean;
  desiredDayRateMin: number;
  desiredDayRateMax: number;
  ir35Preference: string | null;
  noticePeriod: string | null;
  linkedInUrl: string | null;
  masterCvFileName: string | null;
  hasCv: boolean;
  profileCompletionScore: number;
  updatedAt: string;
  alertsEnabled?: boolean;
  alertKeywords?: string;
  alertMinDayRate?: number;
  alertIr35Preference?: string;
  alertMinMatchScore?: number;
  lastAlertSentAt?: string;
}
