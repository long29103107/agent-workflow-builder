START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621132104_AddDurableWorkflowStateMachine') THEN
    ALTER TABLE workflow_runs ADD "Attempt" integer NOT NULL DEFAULT 1;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621132104_AddDurableWorkflowStateMachine') THEN
    ALTER TABLE workflow_runs ADD "FailureDetails" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621132104_AddDurableWorkflowStateMachine') THEN
    ALTER TABLE workflow_runs ADD "Stage" character varying(64) NOT NULL DEFAULT 'Created';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621132104_AddDurableWorkflowStateMachine') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260621132104_AddDurableWorkflowStateMachine', '10.0.9');
    END IF;
END $EF$;
COMMIT;

