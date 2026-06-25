import client from './client';
import type { Profile } from '../types';

export const getProfile = () =>
  client.get<Profile>('/api/profile');

export const updateProfile = (data: Partial<Profile>) =>
  client.put<Profile>('/api/profile', data);

export const uploadCv = (file: File) => {
  const form = new FormData();
  form.append('file', file);
  return client.post('/api/profile/cv', form, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
};

export const getAlertSettings = () =>
  client.get('/api/alerts/settings');

export const updateAlertSettings = (data: {
  alertsEnabled: boolean;
  alertKeywords?: string;
  alertMinDayRate: number;
  alertIr35Preference?: string;
  alertMinMatchScore: number;
}) => client.put('/api/alerts/settings', data);

export const sendTestAlert = () =>
  client.post('/api/alerts/test');

export const downloadCv = () =>
  client.get('/api/profile/cv', { responseType: 'blob' });