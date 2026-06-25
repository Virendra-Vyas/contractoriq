import client from './client';

export interface ApplicationRecord {
  id: string;
  jobId: string;
  status: string;
  notes: string | null;
  dayRateQuoted: number | null;
  recruiterName: string | null;
  recruiterEmail: string | null;
  recruiterPhone: string | null;
  followUpAt: string | null;
  appliedAt: string;
  updatedAt: string;
  jobTitle: string;
  company: string;
  location: string;
  ir35Status: string;
  dayRateMin: number | null;
  dayRateMax: number | null;
  techStack: string;
  sourceUrl: string;
  matchScore: number | null;
}

export const getApplications = () =>
  client.get<ApplicationRecord[]>('/api/applications');

export const updateApplication = (id: string, data: {
  status?: string;
  notes?: string;
  dayRateQuoted?: number;
  recruiterName?: string;
  recruiterEmail?: string;
  recruiterPhone?: string;
  followUpAt?: string;
}) => client.put<ApplicationRecord>(`/api/applications/${id}`, data);

export const applyToJob = (jobId: string) =>
  client.post<ApplicationRecord>(`/api/applications/apply/${jobId}`);

export const deleteApplication = (id: string) =>
  client.delete(`/api/applications/${id}`);