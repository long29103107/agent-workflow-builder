START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622022720_AddApprovalPolicyEngine') THEN
    CREATE TABLE approvals (
        "Id" uuid NOT NULL,
        "ProjectId" character varying(128) NOT NULL,
        "TaskId" character varying(128) NOT NULL,
        "WorkflowRunId" uuid,
        "Gate" character varying(64) NOT NULL,
        "Status" character varying(64) NOT NULL,
        "ArtifactHash" character varying(128),
        "TargetBranch" character varying(256),
        "CommitSha" character varying(128),
        "ApprovedBy" character varying(256) NOT NULL,
        "ApprovedAt" timestamp with time zone NOT NULL,
        "InvalidatedAt" timestamp with time zone,
        "InvalidationReason" text,
        CONSTRAINT "PK_approvals" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_approvals_projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES projects ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_approvals_workflow_runs_WorkflowRunId" FOREIGN KEY ("WorkflowRunId") REFERENCES workflow_runs ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622022720_AddApprovalPolicyEngine') THEN
    CREATE INDEX "IX_approvals_ProjectId_TaskId_Gate_ApprovedAt" ON approvals ("ProjectId", "TaskId", "Gate", "ApprovedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622022720_AddApprovalPolicyEngine') THEN
    CREATE INDEX "IX_approvals_WorkflowRunId" ON approvals ("WorkflowRunId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260622022720_AddApprovalPolicyEngine') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260622022720_AddApprovalPolicyEngine', '10.0.9');
    END IF;
END $EF$;
COMMIT;

