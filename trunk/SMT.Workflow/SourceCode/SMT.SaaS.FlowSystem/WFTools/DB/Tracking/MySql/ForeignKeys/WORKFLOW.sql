ALTER TABLE WORKFLOW
ADD CONSTRAINT FK_WF_WF_TYPE
FOREIGN KEY (WORKFLOW_TYPE_ID) REFERENCES TYPE (TYPE_ID); 