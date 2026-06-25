import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { useBilling } from '../hooks/useBilling';
import PlanBadge from './PlanBadge';
import './Navbar.css';

// ---------------------------------------------------------------------------
// Nav link definitions
// ---------------------------------------------------------------------------

const NAV_LINKS = [
  { to: '/dashboard',    label: 'Dashboard' },
  { to: '/jobs',         label: 'Jobs'      },
  { to: '/saved',        label: 'Saved'     },
  { to: '/profile',      label: 'Profile'   },
  { to: '/applications', label: 'Tracker'   },
  { to: '/pricing',      label: 'Pricing'   },
];

// ---------------------------------------------------------------------------
// Plan badge — only renders when billing data is available
// ---------------------------------------------------------------------------

function NavBillingBadge() {
  const { status } = useBilling();
  if (!status) return null;
  return <PlanBadge tier={status.tier} isActive={status.isActive} />;
}

// ---------------------------------------------------------------------------
// Navbar
// ---------------------------------------------------------------------------

export default function Navbar() {
  const { user, logout, isAuthenticated } = useAuth();
  const navigate  = useNavigate();
  const location  = useLocation();

  const isActive = (to: string) =>
    to === '/'
      ? location.pathname === '/'
      : location.pathname.startsWith(to);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav className="navbar">
      <div className="navbar-inner">

        {/* Logo */}
        <Link to="/" className="navbar-logo">
          ContractorIQ
        </Link>

        {/* Links */}
        {isAuthenticated && (
          <>
            <div className="navbar-links" role="navigation" aria-label="Main navigation">
              {NAV_LINKS.map(({ to, label }) => (
                <Link
                  key={to}
                  to={to}
                  className={`navbar-link${isActive(to) ? ' active' : ''}`}
                >
                  {label}
                </Link>
              ))}
            </div>

            {/* Right side */}
            <div className="navbar-right">
              <NavBillingBadge />
              {user?.firstName && (
                <span className="navbar-user">{user.firstName}</span>
              )}
              <button className="navbar-logout" onClick={handleLogout}>
                Logout
              </button>
            </div>
          </>
        )}
      </div>
    </nav>
  );
}