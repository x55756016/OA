DELIMITER $$

DROP PROCEDURE IF EXISTS UpdateDefaultTrackingProfile $$
/*
 * Create a new version of the default tracking profile.
 */	
CREATE PROCEDURE UpdateDefaultTrackingProfile
(
	IN p_VERSION VARCHAR(32)
	,IN p_TRACKING_PROFILE_XML LONGTEXT
)
BEGIN
	INSERT INTO DEFAULT_TRACKING_PROFILE
	(
		VERSION
		,TRACKING_PROFILE_XML
		,INSERT_DATE_TIME
	)
	VALUES
	(
		p_VERSION
		,p_TRACKING_PROFILE_XML
		,UTC_TIMESTAMP()
	);
END $$

DELIMITER ;
