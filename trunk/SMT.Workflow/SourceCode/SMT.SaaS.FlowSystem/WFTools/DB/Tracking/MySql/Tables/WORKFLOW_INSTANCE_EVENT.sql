DROP TABLE IF EXISTS WORKFLOW_INSTANCE_EVENT;
CREATE TABLE WORKFLOW_INSTANCE_EVENT
(
	WORKFLOW_INSTANCE_EVENT_ID BIGINT UNSIGNED AUTO_INCREMENT NOT NULL
	,WORKFLOW_INSTANCE_ID BIGINT UNSIGNED NOT NULL
	,WORKFLOW_INSTANCE_STATUS_ID SMALLINT UNSIGNED NOT NULL
	,EVENT_ORDER INT UNSIGNED NOT NULL
	,EVENT_ARG_TYPE_ID BIGINT UNSIGNED NULL
	,EVENT_ARG BLOB NULL
	,EVENT_DATE_TIME DATETIME NOT NULL
	,DB_EVENT_DATE_TIME DATETIME NOT NULL
	,PRIMARY KEY ( WORKFLOW_INSTANCE_EVENT_ID )
);

CREATE INDEX WORKFLOW_INSTANCE_EVENT_IDX01 ON WORKFLOW_INSTANCE_EVENT (WORKFLOW_INSTANCE_ID);