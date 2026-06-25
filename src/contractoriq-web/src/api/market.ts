import client from './client';

export interface MarketRate {
  techStack: string;
  location: string;
  ir35Status: string;
  median: number;
  mean: number;
  p25: number;
  p75: number;
  min: number;
  max: number;
  sampleSize: number;
  jobPercentile: number | null;
  percentileLabel: string | null;
}

export const getMarketRate = (
  techStack?: string,
  location?: string,
  ir35Status?: string,
  jobRate?: number
) =>
  client.get<MarketRate>('/api/market/rates', {
    params: { techStack, location, ir35Status, jobRate },
  });