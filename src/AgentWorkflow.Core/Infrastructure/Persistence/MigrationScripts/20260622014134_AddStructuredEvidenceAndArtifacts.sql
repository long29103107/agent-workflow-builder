START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622014134_AddStructuredEvidenceAndArtifacts') THEN
    CREATE TABLE agent_executions (
        "Id" uuid NOT NULL,
        "RunId" uuid NOT NULL,
        "AgentName" character varying(256) NOT NULL,
        "Status" character varying(64) NOT NULL,
        "StartedAt" timestamp with time zone NOT NULL,
        "CompletedAt" timestamp with time zone,
        CONSTRAINT "PK_agent_executions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_agent_executions_workflow_runs_RunId" FOREIGN KEY ("RunId") REFERENCES workflow_runs ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622014134_AddStructuredEvidenceAndArtifacts') THEN
    CREATE TABLE artifacts (
        "Id" uuid NOT NULL,
        "RunId" uuid NOT NULL,
        "AgentExecutionId" uuid,
        "Name" character varying(500) NOT NULL,
        "Type" character varying(128) NOT NULL,
        "Content" text NOT NULL,
        "ContentType" character varying(256) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_artifacts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_artifacts_agent_executions_AgentExecutionId" FOREIGN KEY ("AgentExecutionId") REFERENCES agent_executions ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_artifacts_workflow_runs_RunId" FOREIGN KEY ("RunId") REFERENCES workflow_runs ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622014134_AddStructuredEvidenceAndArtifacts') THEN
    CREATE TABLE evidence_items (
        "Id" uuid NOT NULL,
        "RunId" uuid NOT NULL,
        "AgentExecutionId" uuid NOT NULL,
        "Kind" character varying(64) NOT NULL,
        "Summary" text NOT NULL,
        "SourceReference" text,
        "Action" text,
        "ToolName" character varying(256),
        "ToolResult" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_evidence_items" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_evidence_items_agent_executions_AgentExecutionId" FOREIGN KEY ("AgentExecutionId") REFERENCES agent_executions ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_evidence_items_workflow_runs_RunId" FOREIGN KEY ("RunId") REFERENCES workflow_runs ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622014134_AddStructuredEvidenceAndArtifacts') THEN
    CREATE INDEX "IX_agent_executions_RunId_StartedAt" ON agent_executions ("RunId", "StartedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622014134_AddStructuredEvidenceAndArtifacts') THEN
    CREATE INDEX "IX_artifacts_AgentExecutionId" ON artifacts ("AgentExecutionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622014134_AddStructuredEvidenceAndArtifacts') THEN
    CREATE INDEX "IX_artifacts_RunId_CreatedAt" ON artifacts ("RunId", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622014134_AddStructuredEvidenceAndArtifacts') THEN
    CREATE INDEX "IX_evidence_items_AgentExecutionId" ON evidence_items ("AgentExecutionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622014134_AddStructuredEvidenceAndArtifacts') THEN
    CREATE INDEX "IX_evidence_items_RunId_CreatedAt" ON evidence_items ("RunId", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622014134_AddStructuredEvidenceAndArtifacts') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260622014134_AddStructuredEvidenceAndArtifacts', '10.0.9');
    END IF;
END $EF$;
COMMIT;

