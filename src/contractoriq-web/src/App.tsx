import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider, useAuth } from "./hooks/useAuth";
import Layout from "./components/Layout";
import LoginPage from "./pages/LoginPage";
import DashboardPage from "./pages/DashboardPage";
import JobsPage from "./pages/JobsPage";
import SavedJobsPage from "./pages/SavedJobsPage";
import ProfilePage from "./pages/ProfilePage";
import ApplicationsPage from "./pages/ApplicationsPage";
import PricingPage from "./pages/PricingPage";
import BillingSuccessPage from "./pages/BillingSuccessPage";

// Redirects to /login if not authenticated
const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const { isAuthenticated, initialising } = useAuth();
  if (initialising) return null;
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
};

// Auth check + Layout in one — use this for every app page
const AppLayout = ({ children }: { children: React.ReactNode }) => (
  <ProtectedRoute>
    <Layout>{children}</Layout>
  </ProtectedRoute>
);

function AppRoutes() {
  return (
    <Routes>
      {/* Public — no navbar, no layout */}
      <Route path="/login" element={<LoginPage />} />

      {/* Protected app pages */}
      <Route path="/dashboard"      element={<AppLayout><DashboardPage /></AppLayout>} />
      <Route path="/jobs"           element={<AppLayout><JobsPage /></AppLayout>} />
      <Route path="/saved"          element={<AppLayout><SavedJobsPage /></AppLayout>} />
      <Route path="/profile"        element={<AppLayout><ProfilePage /></AppLayout>} />
      <Route path="/applications"   element={<AppLayout><ApplicationsPage /></AppLayout>} />
      <Route path="/pricing"        element={<AppLayout><PricingPage /></AppLayout>} />
      <Route path="/billing/success" element={<AppLayout><BillingSuccessPage /></AppLayout>} />

      {/* Default */}
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </BrowserRouter>
  );
}