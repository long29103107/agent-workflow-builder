CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621050123_InitialPostgresPersistence') THEN
    CREATE TABLE projects (
        "Id" character varying(128) NOT NULL,
        "PayloadJson" jsonb NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_projects" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621050123_InitialPostgresPersistence') THEN
    CREATE TABLE workflow_runs (
        "Id" uuid NOT NULL,
        "TaskId" character varying(128) NOT NULL,
        "Status" character varying(64) NOT NULL,
        "StartedAt" timestamp with time zone NOT NULL,
        "CompletedAt" timestamp with time zone,
        "ResultJson" jsonb,
        CONSTRAINT "PK_workflow_runs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621050123_InitialPostgresPersistence') THEN
    CREATE TABLE engineering_tasks (
        "Id" character varying(128) NOT NULL,
        "ProjectId" character varying(128) NOT NULL,
        "Title" character varying(500) NOT NULL,
        "Description" text NOT NULL,
        "Status" character varying(64) NOT NULL,
        "Priority" character varying(32) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CompletedAt" timestamp with time zone,
        CONSTRAINT "PK_engineering_tasks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_engineering_tasks_projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES projects ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621050123_InitialPostgresPersistence') THEN
    CREATE TABLE workflow_events (
        "Id" uuid NOT NULL,
        "RunId" uuid NOT NULL,
        "Timestamp" timestamp with time zone NOT NULL,
        "Agent" character varying(256) NOT NULL,
        "Type" character varying(128) NOT NULL,
        "Message" text NOT NULL,
        CONSTRAINT "PK_workflow_events" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_workflow_events_workflow_runs_RunId" FOREIGN KEY ("RunId") REFERENCES workflow_runs ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621050123_InitialPostgresPersistence') THEN
    CREATE TABLE work_items (
        "Id" character varying(128) NOT NULL,
        "EngineeringTaskId" character varying(128) NOT NULL,
        "Source" character varying(32) NOT NULL,
        "SourceKey" character varying(256) NOT NULL,
        "Title" character varying(500) NOT NULL,
        "Description" text NOT NULL,
        "Status" character varying(128) NOT NULL,
        "Priority" character varying(64) NOT NULL,
        "Tags" text[] NOT NULL,
        CONSTRAINT "PK_work_items" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_work_items_engineering_tasks_EngineeringTaskId" FOREIGN KEY ("EngineeringTaskId") REFERENCES engineering_tasks ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621050123_InitialPostgresPersistence') THEN
    CREATE INDEX "IX_engineering_tasks_ProjectId_CreatedAt" ON engineering_tasks ("ProjectId", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621050123_InitialPostgresPersistence') THEN
    CREATE INDEX "IX_work_items_EngineeringTaskId" ON work_items ("EngineeringTaskId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621050123_InitialPostgresPersistence') THEN
    CREATE INDEX "IX_work_items_Source_SourceKey" ON work_items ("Source", "SourceKey");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621050123_InitialPostgresPersistence') THEN
    CREATE INDEX "IX_workflow_events_RunId_Timestamp" ON workflow_events ("RunId", "Timestamp");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621050123_InitialPostgresPersistence') THEN
    CREATE INDEX "IX_workflow_runs_TaskId" ON workflow_runs ("TaskId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621050123_InitialPostgresPersistence') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260621050123_InitialPostgresPersistence', '10.0.9');
    END IF;
END $EF$;
COMMIT;

