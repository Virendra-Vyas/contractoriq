BEGIN;

CREATE TABLE IF NOT EXISTS "Subscriptions" (
                                               "Id"                     UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId"                 UUID          NOT NULL,
    "Tier"                   VARCHAR(50)   NOT NULL DEFAULT 'free',
    "StripeCustomerId"       VARCHAR(100)  NULL,
    "StripeSubscriptionId"   VARCHAR(100)  NULL,
    "Status"                 VARCHAR(50)   NOT NULL DEFAULT 'active',
    "CurrentPeriodEnd"       TIMESTAMPTZ   NULL,
    "CreatedAt"              TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    "UpdatedAt"              TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_Subscriptions_Users_UserId"
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE
    );

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Subscriptions_UserId"
    ON "Subscriptions"("UserId");

CREATE INDEX IF NOT EXISTS "IX_Subscriptions_StripeSubscriptionId"
    ON "Subscriptions"("StripeSubscriptionId");

CREATE INDEX IF NOT EXISTS "IX_Subscriptions_StripeCustomerId"
    ON "Subscriptions"("StripeCustomerId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260608000001_AddSubscriptions', '9.0.0')
    ON CONFLICT DO NOTHING;

COMMIT;