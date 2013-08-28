 CREATE TABLE WORKFLOW_INSTANCE_STATUS
 (
	WORKFLOW_INSTANCE_STATUS_ID NUMBER(4) NOT NULL
	,DESCRIPTION VARCHAR2(128) NOT NULL
);

ALTER TABLE WORKFLOW_INSTANCE_STATUS ADD CONSTRAINT WORKFLOW_INSTANCE_STATUS_PK PRIMARY KEY ( WORKFLOW_INSTANCE_STATUS_ID );

INSERT INTO WORKFLOW_INSTANCE_STATUS (WORKFLOW_INSTANCE_STATUS_ID, DESCRIPTION) VALUES (0, 'Created');
INSERT INTO WORKFLOW_INSTANCE_STATUS (WORKFLOW_INSTANCE_STATUS_ID, DESCRIPTION) VALUES (1, 'Completed');
INSERT INTO WORKFLOW_INSTANCE_STATUS (WORKFLOW_INSTANCE_STATUS_ID, DESCRIPTION) VALUES (2, 'Idle');
INSERT INTO WORKFLOW_INSTANCE_STATUS (WORKFLOW_INSTANCE_STATUS_ID, DESCRIPTION) VALUES (3, 'Suspended');
INSERT INTO WORKFLOW_INSTANCE_STATUS (WORKFLOW_INSTANCE_STATUS_ID, DESCRIPTION) VALUES (4, 'Resumed');
INSERT INTO WORKFLOW_INSTANCE_STATUS (WORKFLOW_INSTANCE_STATUS_ID, DESCRIPTION) VALUES (5, 'Persisted');
INSERT INTO WORKFLOW_INSTANCE_STATUS (WORKFLOW_INSTANCE_STATUS_ID, DESCRIPTION) VALUES (6, 'Unloaded');
INSERT INTO WORKFLOW_INSTANCE_STATUS (WORKFLOW_INSTANCE_STATUS_ID, DESCRIPTION) VALUES (7, 'Loaded');
INSERT INTO WORKFLOW_INSTANCE_STATUS (WORKFLOW_INSTANCE_STATUS_ID, DESCRIPTION) VALUES (8, 'Exception');
INSERT INTO WORKFLOW_INSTANCE_STATUS (WORKFLOW_INSTANCE_STATUS_ID, DESCRIPTION) VALUES (9, 'Terminated');
INSERT INTO WORKFLOW_INSTANCE_STATUS (WORKFLOW_INSTANCE_STATUS_ID, DESCRIPTION) VALUES (10, 'Aborted');
INSERT INTO WORKFLOW_INSTANCE_STATUS (WORKFLOW_INSTANCE_STATUS_ID, DESCRIPTION) VALUES (11, 'Changed');
INSERT INTO WORKFLOW_INSTANCE_STATUS (WORKFLOW_INSTANCE_STATUS_ID, DESCRIPTION) VALUES (12, 'Started');
