DELIMITER $$

DROP PROCEDURE IF EXISTS InsertTrackingDataAnnotation $$
/*
 * Insert a single tracking data annotation.
 */	
CREATE PROCEDURE InsertTrackingDataAnnotation
(
	IN p_WORKFLOW_INSTANCE_ID BIGINT UNSIGNED
	,IN p_TRACKING_DATA_ITEM_ID BIGINT UNSIGNED
	,IN p_ANNOTATION VARCHAR(1024)
)
sproc:BEGIN

	INSERT INTO TRACKING_DATA_ITEM_ANNOTATION
	(
		WORKFLOW_INSTANCE_ID
		,TRACKING_DATA_ITEM_ID
		,ANNOTATION
	)
	VALUES
	(
		p_WORKFLOW_INSTANCE_ID
		,p_TRACKING_DATA_ITEM_ID
		,p_ANNOTATION
	);
	
END $$

DELIMITER ;
