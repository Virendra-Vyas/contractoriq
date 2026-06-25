import { useState, useEffect, useRef } from "react";
import {
  getProfile,
  updateProfile,
  uploadCv,
  downloadCv,
} from "../api/profile";
import type { Profile } from "../types";

export default function ProfilePage() {
  const [profile, setProfile] = useState<Profile | null>(null);
  const [saving, setSaving]     = useState(false);
  const [uploading, setUploading] = useState(false);
  const [saved, setSaved]       = useState(false);
  const [error, setError]       = useState("");
  const fileRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    getProfile()
      .then((res) => setProfile(res.data))
      .catch(() => setError("Failed to load profile"));
  }, []);

  const handleChange = (field: keyof Profile, value: string | boolean | number) => {
    setProfile((prev) => (prev ? { ...prev, [field]: value } : prev));
  };

  const handleSave = async () => {
    if (!profile) return;
    setSaving(true);
    setError("");
    try {
      const res = await updateProfile({
        jobTitle: profile.jobTitle,
        summary: profile.summary,
        skills: profile.skills,
        preferredLocation: profile.preferredLocation,
        remoteOnly: profile.remoteOnly,
        desiredDayRateMin: profile.desiredDayRateMin,
        desiredDayRateMax: profile.desiredDayRateMax,
        ir35Preference: profile.ir35Preference,
        noticePeriod: profile.noticePeriod,
        linkedInUrl: profile.linkedInUrl,
      });
      setProfile(res.data);
      setSaved(true);
      setTimeout(() => setSaved(false), 2500);
    } catch {
      setError("Failed to save profile");
    }
    setSaving(false);
  };

  const handleCvUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploading(true);
    try {
      await uploadCv(file);
      const res = await getProfile();
      setProfile(res.data);
    } catch {
      setError("CV upload failed");
    }
    setUploading(false);
  };

  const handleCvDownload = async () => {
    try {
      const res = await downloadCv();
      const url = URL.createObjectURL(res.data);
      const a = document.createElement("a");
      a.href = url;
      a.download = profile?.masterCvFileName ?? "cv.pdf";
      a.click();
      URL.revokeObjectURL(url);
    } catch {
      setError("CV download failed");
    }
  };

  if (!profile) return (
    <div style={{ maxWidth: "700px", margin: "60px auto", padding: "0 16px", color: "var(--text-secondary)" }}>
      Loading profile...
    </div>
  );

  const scoreColor =
    profile.profileCompletionScore >= 80 ? "var(--success)"
    : profile.profileCompletionScore >= 50 ? "var(--warning)"
    : "var(--danger)";

  return (
    <div style={{ maxWidth: "700px", margin: "0 auto", padding: "24px 16px" }}>

      {/* ── Header card ── */}
      <div style={{
        background: "var(--surface-card)",
        borderRadius: "10px",
        padding: "24px",
        border: "1px solid var(--border-default)",
        marginBottom: "16px",
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
      }}>
        <div>
          <h1 style={{ fontSize: "1.2rem", fontWeight: 700, marginBottom: "4px", color: "var(--gray-900)" }}>
            My Profile
          </h1>
          <p style={{ fontSize: "0.85rem", color: "var(--text-secondary)" }}>
            Keep your profile complete for better AI job matching
          </p>
        </div>

        <div style={{ textAlign: "center" }}>
          <div style={{
            width: "64px", height: "64px",
            borderRadius: "50%",
            border: `4px solid ${scoreColor}`,
            display: "flex", alignItems: "center", justifyContent: "center",
            fontSize: "1.1rem", fontWeight: 700,
            color: scoreColor,
          }}>
            {profile.profileCompletionScore}%
          </div>
          <p style={{ fontSize: "0.72rem", color: "var(--text-tertiary)", marginTop: "4px" }}>
            Complete
          </p>
        </div>
      </div>

      {/* ── Master CV card ── */}
      <div style={{
        background: "var(--surface-card)",
        borderRadius: "10px",
        padding: "20px",
        border: "1px solid var(--border-default)",
        marginBottom: "16px",
      }}>
        <h2 style={{ fontSize: "0.95rem", fontWeight: 600, marginBottom: "16px", color: "var(--gray-900)" }}>
          Master CV
        </h2>
        <div style={{ display: "flex", alignItems: "center", gap: "12px", flexWrap: "wrap" }}>
          {profile.hasCv ? (
            <>
              <span style={{
                background: "var(--primary-light)",
                color: "#818cf8",
                border: "1px solid rgba(99,102,241,0.2)",
                borderRadius: "6px",
                padding: "6px 12px",
                fontSize: "0.85rem",
                fontWeight: 500,
              }}>
                📄 {profile.masterCvFileName}
              </span>
              <button onClick={handleCvDownload} style={ghostBtnStyle}>
                Download
              </button>
              <button onClick={() => fileRef.current?.click()} style={ghostBtnStyle}>
                Replace
              </button>
            </>
          ) : (
            <button onClick={() => fileRef.current?.click()} style={{
              background: "var(--primary)", color: "white", border: "none",
              borderRadius: "6px", padding: "8px 16px", fontSize: "0.85rem", fontWeight: 600,
              cursor: "pointer",
            }}>
              {uploading ? "Uploading..." : "+ Upload CV"}
            </button>
          )}
          <input ref={fileRef} type="file" accept=".pdf,.doc,.docx"
            style={{ display: "none" }} onChange={handleCvUpload} />
        </div>
        <p style={{ fontSize: "0.78rem", color: "var(--text-tertiary)", marginTop: "10px" }}>
          PDF, DOC, or DOCX. Used for AI CV tailoring in Phase 8.
        </p>
      </div>

      {/* ── Profile form card ── */}
      <div style={{
        background: "var(--surface-card)",
        borderRadius: "10px",
        padding: "24px",
        border: "1px solid var(--border-default)",
        marginBottom: "16px",
      }}>
        <h2 style={{ fontSize: "0.95rem", fontWeight: 600, marginBottom: "20px", color: "var(--gray-900)" }}>
          Profile Details
        </h2>

        <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
          <Field label="Current / Target Job Title">
            <input
              value={profile.jobTitle ?? ""}
              onChange={(e) => handleChange("jobTitle", e.target.value)}
              placeholder="e.g. Senior .NET Developer"
              style={inputStyle}
            />
          </Field>

          <Field label="Professional Summary">
            <textarea
              value={profile.summary ?? ""}
              onChange={(e) => handleChange("summary", e.target.value)}
              placeholder="2-3 sentences about your experience and what you're looking for..."
              rows={4}
              style={{ ...inputStyle, resize: "vertical" }}
            />
          </Field>

          <Field label="Skills (comma-separated)">
            <input
              value={profile.skills ?? ""}
              onChange={(e) => handleChange("skills", e.target.value)}
              placeholder="e.g. .NET, C#, React, Azure, SQL Server, Entity Framework"
              style={inputStyle}
            />
          </Field>

          <Field label="Preferred Location">
            <input
              value={profile.preferredLocation ?? ""}
              onChange={(e) => handleChange("preferredLocation", e.target.value)}
              placeholder="e.g. London, Remote, South East England"
              style={inputStyle}
            />
          </Field>

          <div style={{ display: "flex", gap: "16px" }}>
            <Field label="Min Day Rate (£)" style={{ flex: 1 }}>
              <input
                type="number"
                value={profile.desiredDayRateMin ?? ""}
                onChange={(e) => handleChange("desiredDayRateMin", Number(e.target.value))}
                placeholder="e.g. 450"
                style={inputStyle}
              />
            </Field>
            <Field label="Max Day Rate (£)" style={{ flex: 1 }}>
              <input
                type="number"
                value={profile.desiredDayRateMax ?? ""}
                onChange={(e) => handleChange("desiredDayRateMax", Number(e.target.value))}
                placeholder="e.g. 650"
                style={inputStyle}
              />
            </Field>
          </div>

          <Field label="IR35 Preference">
            <select
              value={profile.ir35Preference ?? ""}
              onChange={(e) => handleChange("ir35Preference", e.target.value)}
              style={inputStyle}
            >
              <option value="">No preference</option>
              <option value="outside">Outside IR35 only</option>
              <option value="inside">Inside IR35 acceptable</option>
              <option value="either">Either</option>
            </select>
          </Field>

          <Field label="Notice Period">
            <input
              value={profile.noticePeriod ?? ""}
              onChange={(e) => handleChange("noticePeriod", e.target.value)}
              placeholder="e.g. Immediate, 1 week, 1 month"
              style={inputStyle}
            />
          </Field>

          <Field label="LinkedIn URL">
            <input
              value={profile.linkedInUrl ?? ""}
              onChange={(e) => handleChange("linkedInUrl", e.target.value)}
              placeholder="https://linkedin.com/in/yourprofile"
              style={inputStyle}
            />
          </Field>

          <label style={{ display: "flex", alignItems: "center", gap: "10px", fontSize: "0.875rem", cursor: "pointer", color: "var(--text-primary)" }}>
            <input
              type="checkbox"
              checked={profile.remoteOnly}
              onChange={(e) => handleChange("remoteOnly", e.target.checked)}
            />
            Remote only — exclude onsite roles
          </label>
        </div>
      </div>

      {/* ── Job alerts card ── */}
      <div style={{
        background: "var(--surface-card)",
        borderRadius: "10px",
        padding: "24px",
        border: "1px solid var(--border-default)",
        marginBottom: "16px",
      }}>
        <h2 style={{ fontSize: "0.95rem", fontWeight: 600, marginBottom: "4px", color: "var(--gray-900)" }}>
          Job Alerts
        </h2>
        <p style={{ fontSize: "0.8rem", color: "var(--text-secondary)", marginBottom: "16px" }}>
          Get emailed when new jobs match your profile after each scraper run.
        </p>

        <label style={{ display: "flex", alignItems: "center", gap: "10px", marginBottom: "16px", cursor: "pointer" }}>
          <input
            type="checkbox"
            checked={profile.alertsEnabled ?? false}
            onChange={(e) => handleChange("alertsEnabled", e.target.checked)}
          />
          <span style={{ fontSize: "0.875rem", fontWeight: 500, color: "var(--text-primary)" }}>
            Enable email alerts
          </span>
        </label>

        {profile.alertsEnabled && (
          <div style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
            <Field label="Alert Keywords (comma-separated)">
              <input
                value={profile.alertKeywords ?? ""}
                onChange={(e) => handleChange("alertKeywords", e.target.value)}
                placeholder="e.g. .NET, React, Azure"
                style={inputStyle}
              />
            </Field>
            <Field label="Minimum Day Rate (£)">
              <input
                type="number"
                value={profile.alertMinDayRate ?? 0}
                onChange={(e) => handleChange("alertMinDayRate", Number(e.target.value))}
                placeholder="e.g. 400"
                style={inputStyle}
              />
            </Field>
            <Field label="IR35 Preference for Alerts">
              <select
                value={profile.alertIr35Preference ?? ""}
                onChange={(e) => handleChange("alertIr35Preference", e.target.value)}
                style={inputStyle}
              >
                <option value="">Any</option>
                <option value="outside">Outside IR35 only</option>
                <option value="inside">Inside IR35 acceptable</option>
                <option value="either">Either</option>
              </select>
            </Field>
            <Field label={`Minimum Match Score (${profile.alertMinMatchScore ?? 60}%)`}>
              <input
                type="range" min="0" max="100"
                value={profile.alertMinMatchScore ?? 60}
                onChange={(e) => handleChange("alertMinMatchScore", Number(e.target.value))}
                style={{ width: "100%", accentColor: "var(--primary)" }}
              />
            </Field>
          </div>
        )}
      </div>

      {/* ── Error + Save ── */}
      {error && (
        <p style={{ color: "var(--danger)", fontSize: "0.85rem", marginBottom: "12px" }}>
          {error}
        </p>
      )}
      <button
        onClick={handleSave}
        disabled={saving}
        style={{
          width: "100%", padding: "12px",
          background: saved ? "var(--success)" : "var(--primary)",
          color: "white", border: "none", borderRadius: "8px",
          fontSize: "0.95rem", fontWeight: 600,
          transition: "background 0.2s",
          cursor: saving ? "not-allowed" : "pointer",
        }}
      >
        {saving ? "Saving..." : saved ? "✓ Saved" : "Save Profile"}
      </button>
    </div>
  );
}

/* ── Shared styles ── */
const inputStyle: React.CSSProperties = {
  width: "100%",
  padding: "9px 12px",
  background: "var(--surface-page)",
  border: "1px solid var(--border-default)",
  borderRadius: "7px",
  fontSize: "0.875rem",
  color: "var(--text-primary)",
  outline: "none",
  boxSizing: "border-box",
};

const ghostBtnStyle: React.CSSProperties = {
  background: "transparent",
  border: "1px solid var(--border-emphasis)",
  borderRadius: "6px",
  padding: "6px 12px",
  fontSize: "0.85rem",
  color: "var(--text-secondary)",
  cursor: "pointer",
};

function Field({
  label, children, style,
}: {
  label: string;
  children: React.ReactNode;
  style?: React.CSSProperties;
}) {
  return (
    <div style={style}>
      <label style={{
        display: "block",
        fontSize: "0.8rem",
        fontWeight: 500,
        marginBottom: "6px",
        color: "var(--text-secondary)",
      }}>
        {label}
      </label>
      {children}
    </div>
  );
}