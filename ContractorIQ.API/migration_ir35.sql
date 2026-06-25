CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604210517_InitialCreate') THEN
    CREATE EXTENSION IF NOT EXISTS vector;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604210517_InitialCreate') THEN
    CREATE TABLE "Jobs" (
        "Id" uuid NOT NULL,
        "ExternalId" text NOT NULL,
        "Source" text NOT NULL,
        "Title" text NOT NULL,
        "Company" text NOT NULL,
        "Location" text,
        "IsRemote" boolean NOT NULL,
        "IsHybrid" boolean NOT NULL,
        "DayRateMin" numeric,
        "DayRateMax" numeric,
        "Ir35Status" text,
        "ContractLength" text,
        "Description" text NOT NULL,
        "TechStack" text,
        "RecruiterName" text,
        "RecruiterEmail" text,
        "RecruiterPhone" text,
        "SourceUrl" text NOT NULL,
        "Ir35RiskLevel" text,
        "Ir35RiskFlags" text,
        "DescriptionEmbedding" vector(1536),
        "IsActive" boolean NOT NULL,
        "PostedAt" timestamp with time zone NOT NULL,
        "ScrapedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Jobs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604210517_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "Email" text NOT NULL,
        "PasswordHash" text NOT NULL,
        "FirstName" text NOT NULL,
        "LastName" text NOT NULL,
        "PhoneNumber" text,
        "IsEmailVerified" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604210517_InitialCreate') THEN
    CREATE TABLE "Applications" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "JobId" uuid NOT NULL,
        "Status" text NOT NULL,
        "DayRateQuoted" numeric,
        "RecruiterName" text,
        "RecruiterEmail" text,
        "RecruiterPhone" text,
        "Notes" text,
        "TailoredCvBlobUrl" text,
        "CoverEmail" text,
        "FollowUpAt" timestamp with time zone,
        "AppliedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Applications" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Applications_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES "Jobs" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Applications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604210517_InitialCreate') THEN
    CREATE TABLE "Profiles" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "JobTitle" text,
        "Summary" text,
        "Skills" text,
        "PreferredLocation" text,
        "RemoteOnly" boolean NOT NULL,
        "DesiredDayRateMin" numeric NOT NULL,
        "DesiredDayRateMax" numeric NOT NULL,
        "Ir35Preference" text,
        "NoticePeriod" text,
        "LinkedInUrl" text,
        "MasterCvBlobUrl" text,
        "MasterCvFileName" text,
        "ProfileEmbedding" vector(1536),
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Profiles" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Profiles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604210517_InitialCreate') THEN
    CREATE TABLE "Subscriptions" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Tier" text NOT NULL,
        "StripeCustomerId" text,
        "StripeSubscriptionId" text,
        "Status" text NOT NULL,
        "CurrentPeriodEnd" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Subscriptions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Subscriptions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604210517_InitialCreate') THEN
    CREATE INDEX "IX_Applications_JobId" ON "Applications" ("JobId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604210517_InitialCreate') THEN
    CREATE INDEX "IX_Applications_UserId" ON "Applications" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604210517_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Jobs_ExternalId_Source" ON "Jobs" ("ExternalId", "Source");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604210517_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Profiles_UserId" ON "Profiles" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604210517_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Subscriptions_UserId" ON "Subscriptions" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604210517_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604210517_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260604210517_InitialCreate', '9.0.5');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260606210218_AddMatchScoreToJobs') THEN
    ALTER TABLE "Jobs" ADD "MatchScore" real;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260606210218_AddMatchScoreToJobs') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260606210218_AddMatchScoreToJobs', '9.0.5');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607154323_AddIr35Analysis') THEN
    ALTER TABLE "Jobs" ADD "MatchScore" real;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607154323_AddIr35Analysis') THEN
    CREATE TABLE "Ir35Analyses" (
        "Id" uuid NOT NULL,
        "JobId" uuid NOT NULL,
        "RiskScore" integer NOT NULL,
        "Verdict" text NOT NULL,
        "SubstitutionScore" integer NOT NULL,
        "ControlScore" integer NOT NULL,
        "MooScore" integer NOT NULL,
        "RedFlags" text NOT NULL,
        "GreenFlags" text NOT NULL,
        "Summary" text NOT NULL,
        "SdcRisk" text NOT NULL,
        "AnalysedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Ir35Analyses" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Ir35Analyses_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES "Jobs" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607154323_AddIr35Analysis') THEN
    CREATE UNIQUE INDEX "IX_Ir35Analyses_JobId" ON "Ir35Analyses" ("JobId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260607154323_AddIr35Analysis') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260607154323_AddIr35Analysis', '9.0.5');
    END IF;
END $EF$;
COMMIT;

