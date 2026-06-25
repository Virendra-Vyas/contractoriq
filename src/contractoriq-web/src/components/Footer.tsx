import { Link } from 'react-router-dom';
import './Footer.css';

const PRODUCT = [
  { to: '/dashboard',    label: 'Dashboard' },
  { to: '/jobs',         label: 'Find jobs' },
  { to: '/saved',        label: 'Saved roles' },
  { to: '/applications', label: 'Application tracker' },
  { to: '/profile',      label: 'Your profile' },
  { to: '/pricing',      label: 'Pricing' },
];

const FEATURES = [
  'AI job matching',
  'CV tailoring',
  'IR35 screener',
  'Day rate intelligence',
  'Job alerts',
];

const LEGAL = [
  'Privacy policy',
  'Terms of service',
  'Cookie policy',
  'Contact us',
];

export default function Footer() {
  return (
    <footer className="footer">
      <div className="footer-inner">

        {/* Brand column */}
        <div className="footer-brand">
          <Link to="/" className="footer-logo">ContractorIQ</Link>
          <p className="footer-tagline">
            AI-powered job intelligence for UK IT contractors.
            Find the right contract, know your IR35 status,
            and negotiate your rate with confidence.
          </p>
          <div className="footer-pills">
            <span className="footer-pill">
              <i className="ti ti-map-pin" aria-hidden="true" />
              UK focused
            </span>
            <span className="footer-pill">
              <i className="ti ti-shield-check" aria-hidden="true" />
              IR35 aware
            </span>
            <span className="footer-pill">
              <i className="ti ti-robot" aria-hidden="true" />
              GPT-4o powered
            </span>
          </div>
        </div>

        {/* Product links */}
        <div className="footer-col">
          <p className="footer-col-title">Product</p>
          <ul className="footer-links">
            {PRODUCT.map(({ to, label }) => (
              <li key={to}>
                <Link to={to} className="footer-link">{label}</Link>
              </li>
            ))}
          </ul>
        </div>

        {/* Features */}
        <div className="footer-col">
          <p className="footer-col-title">Features</p>
          <ul className="footer-links">
            {FEATURES.map(label => (
              <li key={label}>
                <span className="footer-link-plain">{label}</span>
              </li>
            ))}
          </ul>
        </div>

        {/* Legal */}
        <div className="footer-col">
          <p className="footer-col-title">Company</p>
          <ul className="footer-links">
            {LEGAL.map(label => (
              <li key={label}>
                <span className="footer-link-plain">{label}</span>
              </li>
            ))}
          </ul>
        </div>

      </div>

      {/* Bottom bar */}
      <div className="footer-bottom">
        <div className="footer-bottom-left">
          <span>© 2026 Yug Solutions Ltd. All rights reserved.</span>
          <span className="footer-sep" aria-hidden="true">·</span>
          <span>Registered in England &amp; Wales</span>
          <span className="footer-sep" aria-hidden="true">·</span>
          <span>Eastbourne, East Sussex</span>
        </div>
        <div className="footer-bottom-right">
          <span className="footer-status">
            <span className="footer-status-dot" aria-hidden="true" />
            All systems operational
          </span>
        </div>
      </div>
    </footer>
  );
}