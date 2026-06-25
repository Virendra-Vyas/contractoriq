import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import apiClient from '../api/client';
import type { AuthResponse } from '../types';
import './LoginPage.css';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate   = useNavigate();

  const [email,    setEmail]    = useState('');
  const [password, setPassword] = useState('');
  const [error,    setError]    = useState('');
  const [loading,  setLoading]  = useState(false);

const handleSubmit = async (e: FormEvent) => {
  e.preventDefault();
  setError('');
  setLoading(true);
  try {
    const res = await apiClient.post<AuthResponse>('/auth/login', { email, password });
    login(res.data);          // ← pass the full AuthResponse to the hook
    navigate('/dashboard');
  } catch {
    setError('Invalid email or password. Please try again.');
  } finally {
    setLoading(false);
  }
};

  return (
    <div className="login-page">
      <div className="login-card">

        {/* Brand */}
        <div className="login-brand">
          <h1 className="login-logo">ContractorIQ</h1>
          <p className="login-tagline">Sign in to your account</p>
        </div>

        {/* Error */}
        {error && (
          <div className="login-error" role="alert">
            <i className="ti ti-alert-circle" aria-hidden="true" />
            {error}
          </div>
        )}

        {/* Form */}
        <form className="login-form" onSubmit={handleSubmit} noValidate>
          <div className="login-field">
            <label className="login-label" htmlFor="email">Email</label>
            <input
              id="email"
              className="login-input"
              type="email"
              autoComplete="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="you@example.com"
              required
            />
          </div>

          <div className="login-field">
            <label className="login-label" htmlFor="password">Password</label>
            <input
              id="password"
              className="login-input"
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="••••••••"
              required
            />
          </div>

          <button className="login-btn" type="submit" disabled={loading}>
            {loading ? 'Signing in…' : 'Sign in'}
          </button>
        </form>

        {/* Register link */}
        <p className="login-footer-text">
          No account?{' '}
          <Link to="/register">Create one free</Link>
        </p>

        {/* Trust signals */}
        <div className="login-features">
          <span className="login-feature-pill">
            <i className="ti ti-shield-check" aria-hidden="true" />
            IR35 aware
          </span>
          <span className="login-feature-pill">
            <i className="ti ti-robot" aria-hidden="true" />
            GPT-4o matching
          </span>
          <span className="login-feature-pill">
            <i className="ti ti-map-pin" aria-hidden="true" />
            UK focused
          </span>
        </div>

      </div>
    </div>
  );
}