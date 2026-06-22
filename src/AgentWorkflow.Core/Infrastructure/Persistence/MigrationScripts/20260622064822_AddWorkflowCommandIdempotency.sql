START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622064822_AddWorkflowCommandIdempotency') THEN
    CREATE TABLE workflow_commands (
        "Id" uuid NOT NULL,
        "RunId" uuid NOT NULL,
        "IdempotencyKey" character varying(256) NOT NULL,
        "Stage" character varying(64) NOT NULL,
        "AppliedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_workflow_commands" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_workflow_commands_workflow_runs_RunId" FOREIGN KEY ("RunId") REFERENCES workflow_runs ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622064822_AddWorkflowCommandIdempotency') THEN
    CREATE UNIQUE INDEX "IX_workflow_commands_RunId_IdempotencyKey" ON workflow_commands ("RunId", "IdempotencyKey");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622064822_AddWorkflowCommandIdempotency') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260622064822_AddWorkflowCommandIdempotency', '10.0.9');
    END IF;
END $EF$;
COMMIT;

