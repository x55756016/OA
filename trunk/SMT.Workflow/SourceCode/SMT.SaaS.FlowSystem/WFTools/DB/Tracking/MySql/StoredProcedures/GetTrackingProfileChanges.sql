DELIMITER $$

DROP PROCEDURE IF EXISTS GetTrackingProfileChanges $$
/*
 * Retrieve all changed tracking profiles since a specific date/time.
 */	
CREATE PROCEDURE GetTrackingProfileChanges
(
	IN p_LAST_CHECK DATETIME
	,p_NEXT_CHECK DATETIME
)
BEGIN
	SET p_NEXT_CHECK = UTC_TIMESTAMP();
	
	SELECT
		T.TYPE_FULL_NAME
		,T.ASSEMBLY_FULL_NAME
		,IFNULL(TP.TRACKING_PROFILE_XML, DTP.TRACKING_PROFILE_XML) TRACKING_PROFILE_XML
		,TP.INSERT_DATE_TIME
	FROM
		TRACKING_PROFILE TP
		INNER JOIN TYPE T
			ON TP.WORKFLOW_TYPE_ID = T.TYPE_ID
		LEFT OUTER JOIN DEFAULT_TRACKING_PROFILE DTP
			ON TP.VERSION = DTP.VERSION
	WHERE
		TP.INSERT_DATE_TIME > p_LAST_CHECK
		AND TP.INSERT_DATE_TIME <= p_NEXT_CHECK
		AND TP.TRACKING_PROFILE_ID IN (
			SELECT MAX(TRACKING_PROFILE_ID)
			FROM TRACKING_PROFILE TP
			GROUP BY WORKFLOW_TYPE_ID
		);
END $$

DELIMITER ;