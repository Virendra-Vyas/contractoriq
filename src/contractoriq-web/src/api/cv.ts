import client from './client';

export const tailorCv = (jobId: string) =>
  client.post(`/api/cv/tailor/${jobId}`, {}, { responseType: 'blob' });