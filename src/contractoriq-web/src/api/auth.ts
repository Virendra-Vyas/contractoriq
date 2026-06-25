import client from './client';
import type { AuthResponse } from '../types';

export const login = (email: string, password: string) =>
  client.post<AuthResponse>('/api/auth/login', { email, password });

export const register = (email: string, password: string, firstName: string, lastName: string) =>
  client.post<AuthResponse>('/api/auth/register', { email, password, firstName, lastName });

export const getMe = () =>
  client.get<AuthResponse>('/api/auth/me');