import client from './client';
import type { Job, JobFilters, PagedResult } from '../types';

export const getJobs = (filters: JobFilters = {}) =>
  client.get<PagedResult<Job>>('/api/jobs', { params: filters });

export const getJob = (id: string) =>
  client.get<Job>(`/api/jobs/${id}`);

export const saveJob = (id: string) =>
  client.post(`/api/jobs/${id}/save`);

export const unsaveJob = (id: string) =>
  client.delete(`/api/jobs/${id}/save`);

export const scoreAllJobs = () =>
  client.post('/api/matching/score-all');

export const getSavedJobs = (page = 1, pageSize = 20) =>
  client.get<PagedResult<Job>>('/api/jobs/saved', { params: { page, pageSize } });