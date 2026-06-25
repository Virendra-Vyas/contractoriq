import client from './client';

export interface Ir35Analysis {
  jobId: string;
  riskScore: number;
  verdict: 'low' | 'medium' | 'high';
  substitutionScore: number;
  controlScore: number;
  mooScore: number;
  sdcRisk: string;
  redFlags: string[];
  greenFlags: string[];
  summary: string;
  fromCache: boolean;
  analysedAt: string;
}

export const getIr35Analysis = (jobId: string) =>
  client.get<Ir35Analysis>(`/api/ir35/${jobId}`);