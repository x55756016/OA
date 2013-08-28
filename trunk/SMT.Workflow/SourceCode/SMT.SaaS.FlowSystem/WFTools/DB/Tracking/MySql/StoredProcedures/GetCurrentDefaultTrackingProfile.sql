DELIMITER $$

DROP PROCEDURE IF EXISTS GetCurrentDefaultTrackingProfile $$
/*
 * Retrieve the current default tracking profile.
 */
CREATE PROCEDURE GetCurrentDefaultTrackingProfile()
BEGIN
	SELECT
		VERSION
		,TRACKING_PROFILE_XML
	FROM
		DEFAULT_TRACKING_PROFILE
	ORDER BY
		INSERT_DATE_TIME DESC, 
		VERSION DESC
	LIMIT 1;
	
END $$

DELIMITER ;
