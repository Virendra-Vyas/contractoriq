import type { Job } from "../types";
import { saveJob, unsaveJob } from "../api/jobs";
import { useState } from "react";
import { tailorCv } from "../api/cv";
import Ir35Modal from "./Ir35Modal";
import MarketRateWidget from "./MarketRateWidget";
import { applyToJob } from "../api/applications";

interface Props {
  job: Job;
  onSaveToggle?: (id: string, saved: boolean) => void;
  onClick?: () => void;
}

export default function JobCard({ job, onSaveToggle, onClick }: Props) {
  const [saved, setSaved] = useState(job.isSaved);
  const [saving, setSaving] = useState(false);
  const [tailoring, setTailoring] = useState(false);
  const [showIr35, setShowIr35] = useState(false);
  const [applying, setApplying] = useState(false);
  const [applied, setApplied] = useState(false);

  // Left accent border + badge colour by IR35 status
  const ir35Color =
    job.ir35Status === "outside"
      ? "var(--outside)"
      : job.ir35Status === "inside"
        ? "var(--danger)"
        : "#475569";

  const techs = job.techStack ? job.techStack.split(",").filter(Boolean) : [];

  const handleSave = async (e: React.MouseEvent) => {
    e.stopPropagation();
    setSaving(true);
    try {
      if (saved) {
        await unsaveJob(job.id);
        setSaved(false);
        onSaveToggle?.(job.id, false);
      } else {
        await saveJob(job.id);
        setSaved(true);
        onSaveToggle?.(job.id, true);
      }
    } catch {}
    setSaving(false);
  };

  const postedDate = new Date(job.postedAt);
  const daysAgo = Math.floor(
    (Date.now() - postedDate.getTime()) / (1000 * 60 * 60 * 24),
  );

  const handleTailorCv = async (e: React.MouseEvent) => {
    e.stopPropagation();
    setTailoring(true);
    try {
      const res = await tailorCv(job.id);
      const url = URL.createObjectURL(
        new Blob([res.data], { type: "application/pdf" }),
      );
      const a = document.createElement("a");
      a.href = url;
      a.download = `tailored-cv-${job.title.replace(/[^a-z0-9]/gi, "-").toLowerCase()}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    } catch {
      alert("CV tailoring failed. Please try again.");
    }
    setTailoring(false);
  };

  const handleApply = async (e: React.MouseEvent) => {
    e.stopPropagation();
    setApplying(true);
    try {
      await applyToJob(job.id);
      setApplied(true);
    } catch {}
    setApplying(false);
  };

  return (
    <div
      onClick={onClick}
      style={{
        background: "var(--surface-card)",
        border: "1px solid var(--border-default)",
        borderLeft: `3px solid ${ir35Color}`,
        borderRadius: "10px",
        padding: "20px 20px 20px 18px",
        cursor: onClick ? "pointer" : "default",
        transition: "border-color 0.15s, box-shadow 0.15s",
        position: "relative",
      }}
      onMouseEnter={(e) => {
        e.currentTarget.style.boxShadow = "0 4px 20px rgba(0,0,0,0.35)";
        e.currentTarget.style.borderColor = "var(--border-emphasis)";
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.boxShadow = "none";
        e.currentTarget.style.borderColor = "var(--border-default)";
      }}
    >
      {/* Header */}
      <div style={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "flex-start",
        marginBottom: "8px",
      }}>
        <div style={{ flex: 1 }}>
          <h3 style={{
            fontSize: "1rem",
            fontWeight: 600,
            color: "var(--gray-900)",
            marginBottom: "4px",
          }}>
            {job.title}
          </h3>
          <p style={{ fontSize: "0.875rem", color: "var(--text-secondary)" }}>
            {job.company} · {job.location}
          </p>
        </div>
        <button
          onClick={handleSave}
          disabled={saving}
          style={{
            background: saved ? "var(--primary-light)" : "transparent",
            border: `1px solid ${saved ? "var(--primary)" : "var(--border-emphasis)"}`,
            borderRadius: "6px",
            padding: "6px 10px",
            fontSize: "0.8rem",
            color: saved ? "var(--info)" : "var(--text-secondary)",
            marginLeft: "12px",
            whiteSpace: "nowrap",
            cursor: "pointer",
          }}
        >
          {saved ? "★ Saved" : "☆ Save"}
        </button>
      </div>

      {/* Badges */}
      <div style={{
        display: "flex",
        gap: "8px",
        flexWrap: "wrap",
        marginBottom: "12px",
      }}>
        {/* IR35 status */}
        <span style={{
          background: `${ir35Color}22`,
          color: ir35Color,
          border: `1px solid ${ir35Color}50`,
          borderRadius: "20px",
          padding: "2px 10px",
          fontSize: "0.75rem",
          fontWeight: 600,
          textTransform: "uppercase",
        }}>
          {job.ir35Status === "outside"
            ? "✓ Outside IR35"
            : job.ir35Status === "inside"
              ? "✗ Inside IR35"
              : "IR35 Unknown"}
        </span>

        {/* Day rate */}
        {(job.dayRateMin || job.dayRateMax) && (
          <span style={{
            background: "var(--surface-elevated)",
            color: "var(--gray-800)",
            border: "1px solid var(--border-default)",
            borderRadius: "20px",
            padding: "2px 10px",
            fontSize: "0.75rem",
            fontWeight: 600,
          }}>
            £{job.dayRateMin ?? "?"} – £{job.dayRateMax ?? "?"}/day
          </span>
        )}

        {/* Applied button */}
        <button
          onClick={handleApply}
          disabled={applying || applied}
          style={{
            background: applied ? "rgba(34,197,94,0.12)" : "transparent",
            border: `1px solid ${applied ? "#4ade80" : "var(--border-emphasis)"}`,
            borderRadius: "6px",
            padding: "4px 10px",
            fontSize: "0.78rem",
            fontWeight: 600,
            cursor: applying || applied ? "not-allowed" : "pointer",
            color: applied ? "#4ade80" : "var(--text-secondary)",
          }}
        >
          {applied ? "✓ Applied" : applying ? "..." : "📤 Applied"}
        </button>

        {/* Match score */}
        {job.matchScore !== null && job.matchScore !== undefined && (
          <span style={{
            background:
              job.matchScore >= 70
                ? "rgba(16,185,129,0.12)"
                : job.matchScore >= 50
                  ? "rgba(245,158,11,0.12)"
                  : "rgba(239,68,68,0.12)",
            color:
              job.matchScore >= 70
                ? "#34d399"
                : job.matchScore >= 50
                  ? "#fbbf24"
                  : "#f87171",
            borderRadius: "20px",
            padding: "2px 10px",
            fontSize: "0.75rem",
            fontWeight: 700,
          }}>
            {job.matchScore}% match
          </span>
        )}

        {/* Remote */}
        {job.isRemote && (
          <span style={{
            background: "rgba(99,102,241,0.12)",
            color: "#818cf8",
            border: "1px solid rgba(99,102,241,0.25)",
            borderRadius: "20px",
            padding: "2px 10px",
            fontSize: "0.75rem",
            fontWeight: 600,
          }}>
            Remote
          </span>
        )}

        {/* Hybrid */}
        {job.isHybrid && (
          <span style={{
            background: "rgba(245,158,11,0.12)",
            color: "#fbbf24",
            border: "1px solid rgba(245,158,11,0.25)",
            borderRadius: "20px",
            padding: "2px 10px",
            fontSize: "0.75rem",
            fontWeight: 600,
          }}>
            Hybrid
          </span>
        )}

        {/* Source */}
        <span style={{
          background: "var(--surface-elevated)",
          color: "var(--text-secondary)",
          border: "1px solid var(--border-default)",
          borderRadius: "20px",
          padding: "2px 10px",
          fontSize: "0.75rem",
        }}>
          {job.source}
        </span>
      </div>

      {/* Tech stack */}
      {techs.length > 0 && (
        <div style={{
          display: "flex",
          gap: "6px",
          flexWrap: "wrap",
          marginBottom: "12px",
        }}>
          {techs.slice(0, 6).map((tech) => (
            <span key={tech} style={{
              background: "var(--primary-light)",
              color: "#818cf8",
              border: "1px solid rgba(99,102,241,0.2)",
              borderRadius: "4px",
              padding: "2px 8px",
              fontSize: "0.72rem",
              fontWeight: 500,
            }}>
              {tech}
            </span>
          ))}
          {techs.length > 6 && (
            <span style={{ fontSize: "0.72rem", color: "var(--text-tertiary)" }}>
              +{techs.length - 6}
            </span>
          )}
        </div>
      )}

      {/* Description */}
      <p style={{
        fontSize: "0.82rem",
        color: "var(--text-secondary)",
        lineHeight: "1.6",
        display: "-webkit-box",
        WebkitLineClamp: 2,
        WebkitBoxOrient: "vertical",
        overflow: "hidden",
        marginBottom: "12px",
      }}>
        {job.description.replace(/&amp;/g, "&").replace(/&nbsp;/g, " ")}
      </p>

      {/* Market Rate Widget */}
      {(job.dayRateMin || job.dayRateMax) && (
        <MarketRateWidget
          techStack={job.techStack}
          location={job.location}
          ir35Status={job.ir35Status}
          jobRate={job.dayRateMax ?? job.dayRateMin ?? undefined}
        />
      )}

      {/* Footer */}
      <div style={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        paddingTop: "12px",
        borderTop: "1px solid var(--border-default)",
        marginTop: "4px",
      }}>
        <span style={{ fontSize: "0.75rem", color: "var(--text-tertiary)" }}>
          {daysAgo === 0 ? "Today" : `${daysAgo}d ago`}
        </span>

        <div style={{ display: "flex", gap: "8px", alignItems: "center" }}>
          <button
            onClick={handleTailorCv}
            disabled={tailoring}
            style={{
              background: tailoring ? "var(--surface-elevated)" : "var(--primary-light)",
              color: "#818cf8",
              border: "1px solid rgba(99,102,241,0.3)",
              borderRadius: "6px",
              padding: "5px 12px",
              fontSize: "0.78rem",
              fontWeight: 600,
              cursor: tailoring ? "not-allowed" : "pointer",
            }}
          >
            {tailoring ? "⏳ Tailoring..." : "✨ Tailor CV"}
          </button>

          <a
            href={job.sourceUrl}
            target="_blank"
            rel="noopener noreferrer"
            onClick={(e) => e.stopPropagation()}
            style={{
              fontSize: "0.8rem",
              color: "var(--primary)",
              fontWeight: 500,
            }}
          >
            Apply →
          </a>

          <button
            onClick={(e) => { e.stopPropagation(); setShowIr35(true); }}
            style={{
              background: "var(--surface-elevated)",
              border: "1px solid var(--border-emphasis)",
              borderRadius: "6px",
              padding: "5px 12px",
              fontSize: "0.78rem",
              fontWeight: 600,
              cursor: "pointer",
              color: "var(--text-secondary)",
            }}
          >
            ⚖️ IR35
          </button>
        </div>
      </div>

      {showIr35 && (
        <Ir35Modal
          jobId={job.id}
          jobTitle={job.title}
          onClose={() => setShowIr35(false)}
        />
      )}
    </div>
  );
}