ALTER TABLE TRACKING_PROFILE
ADD CONSTRAINT FK_TRK_PROFILE_WF_TYPE
FOREIGN KEY (WORKFLOW_TYPE_ID) REFERENCES TYPE (TYPE_ID); 